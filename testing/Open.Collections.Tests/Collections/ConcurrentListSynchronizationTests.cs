using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression and characterization tests for a cluster of <see cref="ConcurrentList{T}"/> correctness
/// bugs that all stem from the same root cause: <c>DumpBuffer()</c> releases its write lock before
/// returning, so members that call it and then touch the underlying list without acquiring a lock of
/// their own are unsynchronized, and members that never call it at all silently miss buffered-but-
/// undrained items.
/// </summary>
/// <remarks>
/// This file intentionally does <b>not</b> cover the <see cref="ConcurrentList{T}.Read(Action)"/> /
/// <see cref="ConcurrentList{T}.Read{TResult}(Func{TResult})"/> reentrant-drain fix (a non-empty buffer
/// touched from inside <c>Read</c> needs to upgrade to a write lock, which a plain read lock can never
/// do under any recursion policy). That fix and its tests live in a separate, later change built on top
/// of this one.
/// </remarks>
public class ConcurrentListSynchronizationTests
{
	// Bug 2: Export(ICollection<T>) is inherited unchanged from ReadOnlyCollectionWrapper, so it
	// never drains the buffer and takes no lock. Buffered-but-undrained items were silently dropped.
	[Fact]
	public void Export_IncludesBufferedButUndrainedItems()
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Add(2);
		list.Add(3);

		var target = new List<int>();
		list.Export(target);

		target.Should().Equal(1, 2, 3);
	}

	// Bug 3: CopyTo(Span<T>) is inherited unchanged from ReadOnlyCollectionWrapper (unlike the
	// CopyTo(T[], int) overload, which IS overridden to dump first), so it never drains the buffer.
	[Fact]
	public void CopyToSpan_IncludesBufferedButUndrainedItems()
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Add(2);
		list.Add(3);

		Span<int> buffer = new int[3];
		var result = list.CopyTo(buffer);

		result.Length.Should().Be(3);
		result.ToArray().Should().Equal(1, 2, 3);
	}

	// Bug 1: the indexer called DumpBuffer() (which takes and releases a write lock internally)
	// and then read/wrote InternalSource[index] with no lock at all, racing with RemoveAt/Insert
	// (which ARE properly locked).
	//
	// A plain int element can't demonstrate this reliably: the CLI guarantees atomic reads/writes
	// of anything up to native word size, so a torn *value* is not observable even when the access
	// is completely unlocked (confirmed empirically: hundreds of millions of unsynchronized reads
	// against the pre-fix indexer never produced a corrupted int - single-word reads/writes just
	// can't tear). To make the race observable, TornProbe is an 8-word (64 byte) struct that is
	// always written with all eight fields equal. The underlying List<T> assigns the new element
	// field-by-field (eight separate stores for a type this size), so an unlocked concurrent reader
	// has a real chance of observing the slot mid-assignment. Any field disagreeing with the others
	// is unambiguous proof that a read raced an in-flight write with no lock in between.
	//
	// The list is kept well above the size needed for index 49 to remain in range at all times
	// (RemoveAt(49) followed by Insert(49, ...) only ever shrinks the list to 199 elements), so the
	// only two *consistent* values a correctly synchronized reader can ever observe at index 49 are
	// all-49s (steady state) or all-50s (the brief, valid window between RemoveAt(49) completing and
	// Insert(49, ...) running, during which the item formerly at index 50 has shifted down to index
	// 49). Any exception, any torn (fields disagree) value, or any consistent value outside that set,
	// indicates the read raced an in-flight mutation.
	private readonly struct TornProbe(long value)
	{
		public readonly long A = value;
		public readonly long B = value;
		public readonly long C = value;
		public readonly long D = value;
		public readonly long E = value;
		public readonly long F = value;
		public readonly long G = value;
		public readonly long H = value;

		public bool IsConsistent
			=> A == B && B == C && C == D && D == E && E == F && F == G && G == H;
	}

	// Dedicated OS threads (not ThreadPool Task.Run) are used deliberately: this suite has other
	// tests that saturate the ThreadPool (e.g. ParallelListTests' 10-million-iteration Parallel.For),
	// and queuing this test's work items onto that same pool made it possible for the whole 1.5s
	// budget to elapse before any reader/writer work item was even dequeued, in turn making the
	// "the readers ran at all" assertion flaky when run as part of the full suite. Raw threads get
	// scheduled by the OS directly and are unaffected by ThreadPool queue depth. A start barrier
	// ensures the race window doesn't begin counting down until every thread is actually running.
	[Fact]
	public void Indexer_ConcurrentReadDuringRemoveInsert_NeverThrowsOrObservesTornOrInvalidValue()
	{
		const int size = 200;
		const int index = 49;
		const int readerCount = 4;
		using var list = new ConcurrentList<TornProbe>();
		for (int i = 0; i < size; i++)
			list.Add(new TornProbe(i));
		list.Count.Should().Be(size); // Force a drain before the race begins.

		using var cts = new CancellationTokenSource();
		var token = cts.Token;
		using var barrier = new Barrier(readerCount + 1 + 1); // readers + writer + this (main) thread.

		Exception readerFailure = null;
		long reads = 0;

		var readers = new Thread[readerCount];
		for (int r = 0; r < readerCount; r++)
		{
			readers[r] = new Thread(() =>
			{
				barrier.SignalAndWait();
				try
				{
					while (!token.IsCancellationRequested)
					{
						TornProbe value = list[index];
						Interlocked.Increment(ref reads);
						bool valid = value.A is 49 or 50;
						if (!value.IsConsistent || !valid)
						{
							readerFailure = new Exception(
								$"Observed invalid/torn value ({value.A},{value.B},{value.C},{value.D},{value.E},{value.F},{value.G},{value.H}) " +
								$"at index {index}; expected all fields to equal 49 or all to equal 50.");
							cts.Cancel();
							return;
						}
					}
				}
				catch (Exception ex)
				{
					readerFailure = ex;
					cts.Cancel();
				}
			})
			{ IsBackground = true };
			readers[r].Start();
		}

		var writer = new Thread(() =>
		{
			barrier.SignalAndWait();
			try
			{
				while (!token.IsCancellationRequested)
				{
					list.RemoveAt(index);
					list.Insert(index, new TornProbe(49));
				}
			}
			catch (Exception ex)
			{
				readerFailure ??= ex;
				cts.Cancel();
			}
		})
		{ IsBackground = true };
		writer.Start();

		// This thread is a barrier participant too, so SignalAndWait here blocks until every
		// reader and the writer have reached the same point and are about to enter their loops.
		// Only then does the 1.5s race window start counting down.
		barrier.SignalAndWait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue("all threads should have started promptly");
		cts.CancelAfter(TimeSpan.FromSeconds(1.5));

		foreach (var thread in readers)
			thread.Join(TimeSpan.FromSeconds(10)).Should().BeTrue("reader thread should finish within the join timeout");
		writer.Join(TimeSpan.FromSeconds(10)).Should().BeTrue("writer thread should finish within the join timeout");

		readerFailure.Should().BeNull(readerFailure?.ToString());
		reads.Should().BeGreaterThan(0, "the readers should have had a chance to run at all");
	}

	// Empirical confirmation of the CRITICAL constraint behind shipping the indexer fix (bug 1) on
	// this branch: the indexer's new read lock (RWLock.ReadLock()) and Read(Action)'s existing read
	// lock (unchanged on this branch) can be acquired on the same thread, nested, when Read's delegate
	// touches the indexer. With an EMPTY buffer, DumpBuffer() inside the indexer is a no-op, so this
	// exercises pure read -> read recursion on the same thread - nothing here needs the write-lock
	// upgrade that a non-empty buffer would require (that's bug 4's territory, fixed on a separate,
	// later branch - see the class remarks). Recursive read -> read acquisition on the same thread is
	// legal ONLY under LockRecursionPolicy.SupportsRecursion; under the default NoRecursion policy it
	// throws LockRecursionException. This test was verified by hand both ways while implementing the
	// fix: constructing RWLock as `new ReaderWriterLockSlim()` (NoRecursion) makes it FAIL with
	// LockRecursionException, and `new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)`
	// (what ships on this branch) makes it PASS. Shipping the indexer fix without also switching to
	// SupportsRecursion would have introduced this new failure mode.
	[Fact]
	public void Read_IndexerAccessWithPreDrainedEmptyBuffer_RequiresSupportsRecursion()
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Count.Should().Be(1); // Force a drain; the buffer is empty going into Read().

		int observed = -1;
		var exception = Record.Exception(() =>
			list.Read(() =>
			{
				observed = list[0]; // Read -> Read, same thread, empty buffer (no write-lock upgrade needed).
			}));

		exception.Should().BeNull();
		observed.Should().Be(1);
	}

	// Bug (new, no test coverage before this branch): the Capacity getter read InternalSource.Capacity
	// with no lock at all. This is a plain correctness check - not a race repro - since Capacity's
	// getter/setter only ever exchange a word-sized int/array-reference and can't produce a torn value
	// on any current .NET runtime, locked or not (see the concurrent test below for the fuller
	// explanation of why this is a characterization test rather than a regression guard).
	[Fact]
	public void Capacity_Get_ReturnsUnderlyingListCapacity()
	{
		using var list = new ConcurrentList<int>(0);
		list.Capacity.Should().Be(0);

		list.Capacity = 16;
		list.Capacity.Should().Be(16);

		list.Add(1);
		list.Count.Should().Be(1); // Force a drain so growth (if any) has happened.
		list.Capacity.Should().BeGreaterThanOrEqualTo(1);
	}

	// Characterization test (passes both before and after the Capacity getter's lock was added): a
	// concurrent reader loop against Capacity while a writer thread repeatedly sets it. This is
	// deliberately NOT labeled a regression guard - unlike the indexer's TornProbe repro, there is no
	// way to make this fail pre-fix. List<T>.Capacity's setter only ever finishes by atomically
	// reassigning its internal array reference (a single word-sized store), so an unsynchronized
	// concurrent getter can only ever observe a fully-old or fully-new capacity, never a torn one -
	// with or without ConcurrentList's own lock around it. What the lock buys here is consistency with
	// the rest of the type's locking discipline (e.g. relative to Grow(), which resizes under a write
	// lock), not crash- or corruption-prevention, so this test exists purely to give the new lock some
	// coverage and to document that no observable failure could be forced for it.
	[Fact]
	public void Capacity_ConcurrentReadDuringConcurrentSet_NeverThrowsOrObservesInvalidValue()
	{
		const int readerCount = 4;
		using var list = new ConcurrentList<int>(0);

		using var cts = new CancellationTokenSource();
		var token = cts.Token;
		using var barrier = new Barrier(readerCount + 1 + 1); // readers + writer + this (main) thread.

		Exception failure = null;
		long reads = 0;

		var readers = new Thread[readerCount];
		for (int r = 0; r < readerCount; r++)
		{
			readers[r] = new Thread(() =>
			{
				barrier.SignalAndWait();
				try
				{
					while (!token.IsCancellationRequested)
					{
						int capacity = list.Capacity;
						Interlocked.Increment(ref reads);
						if (capacity is not 0 and not 16 and not 32)
						{
							failure = new Exception($"Observed unexpected Capacity value: {capacity}.");
							cts.Cancel();
							return;
						}
					}
				}
				catch (Exception ex)
				{
					failure = ex;
					cts.Cancel();
				}
			})
			{ IsBackground = true };
			readers[r].Start();
		}

		var writer = new Thread(() =>
		{
			barrier.SignalAndWait();
			try
			{
				bool toggle = false;
				while (!token.IsCancellationRequested)
				{
					list.Capacity = toggle ? 32 : 16;
					toggle = !toggle;
				}
			}
			catch (Exception ex)
			{
				failure ??= ex;
				cts.Cancel();
			}
		})
		{ IsBackground = true };
		writer.Start();

		barrier.SignalAndWait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue("all threads should have started promptly");
		cts.CancelAfter(TimeSpan.FromSeconds(1));

		foreach (var thread in readers)
			thread.Join(TimeSpan.FromSeconds(10)).Should().BeTrue("reader thread should finish within the join timeout");
		writer.Join(TimeSpan.FromSeconds(10)).Should().BeTrue("writer thread should finish within the join timeout");

		failure.Should().BeNull(failure?.ToString());
		reads.Should().BeGreaterThan(0, "the readers should have had a chance to run at all");
	}
}
