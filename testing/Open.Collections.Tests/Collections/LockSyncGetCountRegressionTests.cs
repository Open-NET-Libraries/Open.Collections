using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression tests for <see cref="LockSynchronizedCollectionWrapper{T, TCollection}.GetCount"/>.
/// Before the fix, <c>Count</c> read <c>InternalSource.Count</c> without taking <c>Sync</c>, even
/// though every mutating member (<c>Add</c>/<c>Remove</c>/<c>Clear</c>/...) on the same type does take
/// it. For a backing collection like <see cref="List{T}"/>, Count is a single field, so an unlocked
/// read is harmless. But <see cref="Dictionary{TKey, TValue}"/>.Count is computed from two separate
/// fields (<c>_count - _freeCount</c>), so an unlocked reader can observe a transient value that was
/// never actually true of the collection at any single instant.
/// </summary>
public class LockSyncGetCountRegressionTests
{
	/// <summary>
	/// A minimal <see cref="ICollection{T}"/> stand-in that models the general hazard class - Count
	/// bookkeeping updated before the mutation is actually committed/visible - and lets the test
	/// deterministically pause an in-flight <see cref="Add"/> at that exact point, instead of trying to
	/// win the real BCL <see cref="Dictionary{TKey, TValue}"/>'s much narrower (a handful of CPU
	/// instructions) timing window. This is a simplified model of the defect class, not a byte-for-byte
	/// reproduction of Dictionary's internals (see <see cref="RealDictionary_ConcurrentChurn_CountStaysWithinLegalBounds"/>
	/// for a best-effort test against the real type).
	/// </summary>
	private sealed class SteppableCollection : ICollection<int>
	{
		private readonly List<int> _items = [];
		public readonly ManualResetEventSlim PausedMidAdd = new(false);
		public readonly ManualResetEventSlim ResumeAdd = new(false);

		/// <summary>The "torn-capable" computed Count, analogous to Dictionary's <c>_count - _freeCount</c>.</summary>
		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public void Add(int item)
		{
			Count++;              // Bookkeeping updated first...
			PausedMidAdd.Set();
			ResumeAdd.Wait();
			_items.Add(item);      // ...the item is only actually committed/visible after this.
		}

		public void Clear() { _items.Clear(); Count = 0; }
		public bool Contains(int item) => _items.Contains(item);
		public void CopyTo(int[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
		public bool Remove(int item) => _items.Remove(item);
		public IEnumerator<int> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	[Fact]
	public void GetCount_IsNotObservableMidMutation()
	{
		var backing = new SteppableCollection();
		var wrapper = new LockSynchronizedCollectionWrapper<int, SteppableCollection>(backing);

		backing.Count.Should().Be(0, "nothing has been added yet");

		Exception writerException = null;
		Exception readerException = null;
		int countObservedBeforeResume = -1;
		bool readerReturnedBeforeResume = false;

		var writer = new Thread(() =>
		{
			try
			{
				// Blocks inside Add (holding Sync) until the main thread signals ResumeAdd.
				wrapper.Add(999);
			}
			catch (Exception ex)
			{
				writerException = ex;
			}
		});

		var reader = new Thread(() =>
		{
			try
			{
				backing.PausedMidAdd.Wait();
				int observed = wrapper.Count; // The call under test.
				readerReturnedBeforeResume = !backing.ResumeAdd.IsSet;
				countObservedBeforeResume = observed;
			}
			catch (Exception ex)
			{
				readerException = ex;
			}
		});

		writer.Start();
#pragma warning disable xUnit1051 // Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken
		backing.PausedMidAdd.Wait(TimeSpan.FromSeconds(5))
			.Should().BeTrue("the writer thread should reach its pause point inside Add");
#pragma warning restore xUnit1051 // Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken

		reader.Start();
		// Give the reader every opportunity to race ahead of the writer if Count isn't locked.
		// This is generous relative to the trivial, non-blocking work the reader does when unlocked
		// (start a thread, wait on an already-set event, read a field) - not a timing coincidence.
		bool readerFinishedEarly = reader.Join(TimeSpan.FromMilliseconds(500));

		backing.ResumeAdd.Set();
		reader.Join(TimeSpan.FromSeconds(5)).Should().BeTrue();
		writer.Join(TimeSpan.FromSeconds(5)).Should().BeTrue();

		List<Exception> exceptions = [];
		if (writerException is not null) exceptions.Add(writerException);
		if (readerException is not null) exceptions.Add(readerException);
		if (exceptions.Count > 0) throw new AggregateException(exceptions);

		if (readerFinishedEarly && readerReturnedBeforeResume)
		{
			// The reader read Count without ever waiting on Sync - the exact condition that lets a
			// torn/premature read through. At this instant no item has actually been committed (the
			// add hasn't finished, _items is still empty), so the only truthful value is 0.
			countObservedBeforeResume.Should().Be(0,
				"GetCount() must not observe an Add's in-flight, not-yet-committed state");
		}
		// Otherwise the reader correctly blocked on Sync until the writer released it - the desired,
		// race-free behavior - and there is nothing further to assert about a mid-mutation read.

		wrapper.Count.Should().Be(1);
		backing.Count.Should().Be(1);
	}

	/// <summary>
	/// Best-effort, non-deterministic characterization test against the real
	/// <see cref="LockSynchronizedDictionary{TKey, TValue}"/> (backed by the actual BCL
	/// <see cref="Dictionary{TKey, TValue}"/>, whose Count genuinely is <c>_count - _freeCount</c>).
	/// Unlike <see cref="GetCount_IsNotObservableMidMutation"/> above, this test has no way to control
	/// the timing of the real tear window, which is only a handful of CPU instructions wide. It is
	/// <b>not a reliable regression guard</b>: it is not expected to reliably fail against the pre-fix
	/// code, and it passing does not prove the fix works. It's included only as an honest, best-effort
	/// real-world sanity check that heavy concurrent churn doesn't surface an impossible Count.
	/// </summary>
	[Fact]
	public void RealDictionary_ConcurrentChurn_CountStaysWithinLegalBounds()
	{
		var dictionary = new LockSynchronizedDictionary<int, int>();
		const int itemsPerWriter = 5_000;
		const int writerThreadCount = 4;

		List<Exception> exceptions = [];
		var readerObservations = new ConcurrentBag<int>();
		using var stop = new CancellationTokenSource();

		var reader = new Thread(() =>
		{
			try
			{
				while (!stop.IsCancellationRequested)
					readerObservations.Add(dictionary.Count);
			}
			catch (Exception ex)
			{
				lock (exceptions) exceptions.Add(ex);
			}
		});

		var writers = new Thread[writerThreadCount];
		for (int t = 0; t < writerThreadCount; t++)
		{
			int threadIndex = t;
			writers[t] = new Thread(() =>
			{
				try
				{
					for (int i = 0; i < itemsPerWriter; i++)
					{
						int key = threadIndex * itemsPerWriter + i;
						dictionary.Add(key, key);
						dictionary.Remove(key);
					}
				}
				catch (Exception ex)
				{
					lock (exceptions) exceptions.Add(ex);
				}
			});
		}

		reader.Start();
		foreach (var w in writers) w.Start();
		foreach (var w in writers) w.Join();
		stop.Cancel();
		reader.Join(TimeSpan.FromSeconds(5)).Should().BeTrue();

		if (exceptions.Count > 0) throw new AggregateException(exceptions);

		// Each writer has at most one item in flight at a time, so the true size is always within
		// [0, writerThreadCount]. A torn _count-_freeCount read could in principle surface as a
		// negative or otherwise out-of-range value.
		readerObservations.Should().NotBeEmpty();
		readerObservations.Should().OnlyContain(c => c >= 0 && c <= writerThreadCount);

		dictionary.Count.Should().Be(0);
	}
}
