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
/// The tests at the bottom of this file cover a related but distinct bug: the
/// <see cref="ConcurrentList{T}.Read(Action)"/> / <see cref="ConcurrentList{T}.Read{TResult}(Func{TResult})"/>
/// reentrant-drain hazard. A non-empty buffer touched from inside <c>Read</c> (directly, or via the
/// indexer, <c>IndexOf</c>, <c>Contains</c>, <c>CopyTo</c>, <c>Export</c>, or <c>Snapshot</c>) needs to
/// upgrade to a write lock to drain, which a plain read lock can never do under any recursion policy.
/// Rather than switching <c>Read</c> to an upgradable-read lock (which would serialize all <c>Read</c>
/// calls against each other), the affected members detect reentrancy via the thread-local
/// <see cref="ReaderWriterLockSlim.IsReadLockHeld"/> and, when already inside a read lock, read straight
/// through the drained list followed by the still-buffered tail instead of draining.
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
						bool valid = value.A == 49 || value.A == 50;
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
						if (capacity != 0 && capacity != 16 && capacity != 32)
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

	// Bug 4 (the deterministic repro from the bug report): Read(Action) holds a plain read lock while
	// invoking user code. If that code touches the indexer while the buffer is non-empty, DumpBuffer()
	// requests a write lock from inside a read lock and throws LockRecursionException - a plain read
	// lock can never legally upgrade to a write lock, under any recursion policy. The fix detects this
	// via the thread-local IsReadLockHeld and reads straight through InternalSource + the buffered tail
	// instead of draining.
	[Fact]
	public void Read_ReentrantIndexerAccessWithPendingBuffer_DoesNotThrow()
	{
		using var list = new ConcurrentList<int>();
		int observed = -1;

		var exception = Record.Exception(() =>
			list.Read(() =>
			{
				list.Add(1); // Buffers the item; does not drain or take any lock.
				observed = list[0]; // Must NOT try to drain (would throw); must read the buffer instead.
			}));

		exception.Should().BeNull();
		observed.Should().Be(1);
	}

	// Same reentrancy hazard via the Func<TResult> overload.
	[Fact]
	public void ReadOfT_ReentrantIndexerAccessWithPendingBuffer_DoesNotThrow()
	{
		using var list = new ConcurrentList<int>();

		int result = -1;
		var exception = Record.Exception(() =>
		{
			result = list.Read(() =>
			{
				list.Add(42);
				return list[0];
			});
		});

		exception.Should().BeNull();
		result.Should().Be(42);
	}

	// The indexer must correctly resolve BOTH sides of the drained/buffered split while reentrant:
	// indices below InternalSource.Count come straight from the list, indices at or above it come
	// from walking the still-buffered ConcurrentQueue tail in FIFO order.
	[Fact]
	public void Read_IndexerReentrant_ResolvesBothDrainedAndBufferedIndices()
	{
		using var list = new ConcurrentList<int>();
		for (int i = 0; i < 5; i++) list.Add(i);
		list.Count.Should().Be(5); // Force a drain: indices 0-4 now live in InternalSource.

		list.Read(() =>
		{
			for (int i = 5; i < 10; i++) list.Add(i); // Buffers 5-9; never drained inside Read().

			for (int i = 0; i < 10; i++)
				list[i].Should().Be(i, $"index {i} should resolve whether drained or still buffered");
		});
	}

	// Reading past the end of the logical (drained + buffered) range while reentrant must still throw
	// ArgumentOutOfRangeException, matching the non-reentrant indexer's contract, rather than silently
	// returning a wrong value or throwing an unrelated exception from the manual buffer walk.
	[Fact]
	public void Read_IndexerReentrant_OutOfRangeIndexThrows()
	{
		using var list = new ConcurrentList<int>();
		list.Read(() =>
		{
			list.Add(1); // One buffered item; index 0 is valid, index 1 is not.
			Action act = () => _ = list[1];
			act.Should().Throw<ArgumentOutOfRangeException>();
		});
	}

	// IndexOf/Contains must also see buffered-but-undrained items from inside Read().
	[Fact]
	public void Read_IndexOfAndContainsReentrant_SeeBufferedItems()
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Count.Should().Be(1); // Force a drain.

		list.Read(() =>
		{
			list.Add(2); // Buffered; never drained inside Read().

			list.IndexOf(1).Should().Be(0);
			list.IndexOf(2).Should().Be(1);
			list.IndexOf(99).Should().Be(-1);
			list.Contains(2).Should().BeTrue();
			list.Contains(99).Should().BeFalse();
		});
	}

	// Count must reflect buffered additions made from inside Read() itself (GetCount() reads the
	// Interlocked _count field directly and was never affected by this bug, but this locks the
	// end-to-end behavior in as part of the reentrancy fix).
	[Fact]
	public void Read_CountReflectsBufferedAdditionsMadeInsideTheDelegate()
	{
		using var list = new ConcurrentList<int>();
		list.Read(() =>
		{
			list.Add(1);
			list.Add(2);
			list.Count.Should().Be(2);
		});
	}

	// Snapshot(), Export(), CopyTo(T[], int), and CopyTo(Span<T>) must all see buffered-but-undrained
	// items when called reentrantly from inside Read(), the same as they do outside of Read().
	[Fact]
	public void Read_SnapshotExportAndCopyToReentrant_SeeBufferedItems()
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Count.Should().Be(1); // Force a drain.

		list.Read(() =>
		{
			list.Add(2);
			list.Add(3); // Buffered; never drained inside Read().

			list.Snapshot().Should().Equal(1, 2, 3);

			var exported = new List<int>();
			list.Export(exported);
			exported.Should().Equal(1, 2, 3);

			var array = new int[3];
			list.CopyTo(array, 0);
			array.Should().Equal(1, 2, 3);

			Span<int> span = new int[3];
			var result = list.CopyTo(span);
			result.ToArray().Should().Equal(1, 2, 3);
		});
	}

	// Nested Read() calls hit the identical hazard as the indexer: the inner Read() unconditionally
	// called DumpBuffer() too. Guarding that call the same way (skip it when this thread already holds
	// the read lock) fixes reentrant Read() itself, not just the members it might call.
	[Fact]
	public void Read_NestedReadWithPendingBuffer_DoesNotThrow()
	{
		using var list = new ConcurrentList<int>();
		int observed = -1;

		var exception = Record.Exception(() =>
			list.Read(() =>
			{
				list.Add(1); // Buffers; the outer Read() already holds the read lock.
				list.Read(() =>
				{
					observed = list[0]; // Reentrant indexer access one level deeper.
				});
			}));

		exception.Should().BeNull();
		observed.Should().Be(1);
	}

	// Mutating members cannot be made to work from inside Read() - draining still requires an illegal
	// read-to-write upgrade - but they should fail with a clear, purpose-built diagnostic instead of a
	// raw LockRecursionException leaking out of DumpBuffer()/RWLock internals.
	[Theory]
	[InlineData(nameof(ConcurrentList<int>.Insert))]
	[InlineData(nameof(ConcurrentList<int>.RemoveAt))]
	[InlineData(nameof(ConcurrentList<int>.Remove))]
	[InlineData(nameof(ConcurrentList<int>.Clear))]
	[InlineData("IndexerSet")]
	public void Read_MutatingFromWithinDelegate_ThrowsClearInvalidOperationException(string member)
	{
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Count.Should().Be(1); // Force a drain.

		Exception exception = null;
		list.Read(() =>
		{
			exception = Record.Exception(() =>
			{
				switch (member)
				{
					case nameof(ConcurrentList<int>.Insert): list.Insert(0, 2); break;
					case nameof(ConcurrentList<int>.RemoveAt): list.RemoveAt(0); break;
					case nameof(ConcurrentList<int>.Remove): list.Remove(1); break;
					case nameof(ConcurrentList<int>.Clear): list.Clear(); break;
					case "IndexerSet": list[0] = 2; break;
				}
			});
		});

		exception.Should().BeOfType<InvalidOperationException>();
	}

	// The zero-caller-misuse variant of the bug: no thread ever touches the indexer from inside its own
	// Read() delegate. Instead, one thread runs a purely read-only Read() that repeatedly indexes the
	// list, while a second thread concurrently calls the ordinary, unsynchronized Add() (which lands in
	// the buffer without taking any lock). Before the fix, the reader's own indexer calls would try to
	// drain a buffer that some unrelated thread filled, hitting the exact same illegal write-lock
	// upgrade from within its own held read lock.
	[Fact]
	public void Read_ConcurrentAddFromAnotherThreadWhileReading_DoesNotThrow()
	{
		using var list = new ConcurrentList<int>();
		for (int i = 0; i < 10; i++) list.Add(i);
		list.Count.Should().Be(10); // Force a drain before the race begins.

		using var cts = new CancellationTokenSource();
		var token = cts.Token;
		Exception readerFailure = null;
		Exception writerFailure = null;

		var reader = new Thread(() =>
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					list.Read(() =>
					{
						long sum = 0;
						int count = list.Count;
						for (int i = 0; i < count; i++) sum += list[i];
					});
				}
			}
			catch (Exception ex)
			{
				readerFailure = ex;
				cts.Cancel();
			}
		})
		{ IsBackground = true };

		var writer = new Thread(() =>
		{
			try
			{
				int next = 10;
				while (!token.IsCancellationRequested)
					list.Add(next++);
			}
			catch (Exception ex)
			{
				writerFailure = ex;
				cts.Cancel();
			}
		})
		{ IsBackground = true };

		reader.Start();
		writer.Start();
		cts.CancelAfter(TimeSpan.FromSeconds(1));
		reader.Join(TimeSpan.FromSeconds(10)).Should().BeTrue();
		writer.Join(TimeSpan.FromSeconds(10)).Should().BeTrue();

		readerFailure.Should().BeNull(readerFailure?.ToString());
		writerFailure.Should().BeNull(writerFailure?.ToString());
	}

	// The entire point of the tail-fallback design over an upgradable-read lock: concurrent Read() calls
	// from different threads must genuinely overlap, not serialize. Proven by tracking the high-water
	// mark of how many threads are simultaneously inside a Read() delegate at once: if Read() held an
	// exclusive lock (an upgradable-read lock allows only one holder at a time, by design), that
	// high-water mark could never exceed 1, no matter how the OS happens to schedule the threads. This
	// does not require every thread to land on one exact rendezvous instant (which would be flaky under
	// scheduler/CPU contention from the rest of the suite) - just that at least two threads are ever
	// observed inside at the same time during a short, deliberately-held window.
	[Fact]
	public void Read_ConcurrentCallsFromMultipleThreads_GenuinelyOverlap()
	{
		const int threadCount = 8;
		using var list = new ConcurrentList<int>();
		list.Add(1);
		list.Count.Should().Be(1); // Force a drain.

		int insideCount = 0;
		int maxObservedInside = 0;
		using var release = new ManualResetEventSlim(false);
		using var allDone = new CountdownEvent(threadCount);
		Exception failure = null;

		var threads = new Thread[threadCount];
		for (int t = 0; t < threadCount; t++)
		{
			threads[t] = new Thread(() =>
			{
				try
				{
					list.Read(() =>
					{
						int now = Interlocked.Increment(ref insideCount);
						int observedMax;
						do
						{
							observedMax = maxObservedInside;
							if (now <= observedMax) break;
						}
						while (Interlocked.CompareExchange(ref maxObservedInside, now, observedMax) != observedMax);

						// Hold this thread inside the delegate briefly so other threads have a real
						// chance to pile in concurrently before any of them leave.
						release.Wait(TimeSpan.FromSeconds(10));
						Interlocked.Decrement(ref insideCount);
					});
				}
				catch (Exception ex)
				{
					failure = ex;
				}
				finally
				{
					allDone.Signal();
				}
			})
			{ IsBackground = true };
		}

		foreach (var thread in threads) thread.Start();
		Thread.Sleep(TimeSpan.FromMilliseconds(500)); // Let threads pile up inside Read() before releasing.
		release.Set();

		allDone.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken).Should().BeTrue("all Read() calls should complete without deadlocking");

		failure.Should().BeNull(failure?.ToString());
		maxObservedInside.Should().BeGreaterThan(1,
			"multiple threads should be able to be inside Read() at the same time; an exclusive " +
			"(e.g. upgradable-read) lock would make this impossible regardless of scheduling");
	}

	[Fact]
	public void CopyTo_Reentrant_UndersizedArray_ThrowsArgumentException()
	{
		var list = new ConcurrentList<int>();
		list.Read(() =>
		{
			list.Add(1);
			list.Add(2);
			list.Add(3);

			// The reentrant branch writes the drained portion then the buffered tail by index.
			// Without an up-front length check that walk overruns with IndexOutOfRangeException,
			// where ICollection<T>.CopyTo is contractually an ArgumentException.
			var tooSmall = new int[2];
			Assert.Throws<ArgumentException>(() => list.CopyTo(tooSmall, 0));
		});
	}
}
