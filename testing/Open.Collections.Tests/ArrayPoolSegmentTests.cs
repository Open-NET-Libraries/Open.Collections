using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests;

public class ArrayPoolSegmentTests
{
	[Fact]
	public void DisposingSliceDoesNotReleaseParentBuffer()
	{
		ArrayPool<int> pool = ArrayPool<int>.Create();

		var parent = new ArrayPoolSegment<int>(16, pool);
		int[] parentArray = parent.Segment.Array!;

		ArrayPoolSegment<int> slice = parent.Slice(4);

		// Disposing the slice must be a no-op: it does not own the buffer.
		slice.Dispose();

		// The parent's buffer must still be considered rented; renting again
		// from the pool must NOT hand back the same array instance while the
		// parent is still alive.
		int[] rentedAgain = pool.Rent(16);
		Assert.False(ReferenceEquals(parentArray, rentedAgain));

		pool.Return(rentedAgain);
		parent.Dispose();
	}

	[Fact]
	public void DisposingParentReturnsBufferToPool()
	{
		ArrayPool<int> pool = ArrayPool<int>.Create();

		var parent = new ArrayPoolSegment<int>(16, pool);
		int[] parentArray = parent.Segment.Array!;

		parent.Dispose();

		// After disposing the original (owning) segment, the array should be
		// available again from the pool.
		int[] rentedAgain = pool.Rent(16);
		Assert.True(ReferenceEquals(parentArray, rentedAgain));

		pool.Return(rentedAgain);
	}

	[Fact]
	public void SliceExposesCorrectElements()
	{
		ArrayPool<int> pool = ArrayPool<int>.Create();

		using var parent = new ArrayPoolSegment<int>(16, pool);
		for (int i = 0; i < parent.Segment.Count; i++)
			parent.Segment[i] = i;

		ArrayPoolSegment<int> slice = parent.Slice(4, 6);

		Assert.Equal(6, slice.Segment.Count);
		for (int i = 0; i < slice.Segment.Count; i++)
			Assert.Equal(i + 4, slice.Segment[i]);

		// The slice is a view over the same underlying array as the parent.
		Assert.True(ReferenceEquals(parent.Segment.Array, slice.Segment.Array));

		// Slice does not carry ownership of the pool.
		Assert.Null(slice.Pool);

		// Disposing the slice is safe and does not affect the parent's data.
		slice.Dispose();
		Assert.Equal(4, parent.Segment[4]);
	}

	[Fact]
	public void DisposingBothParentAndSliceCallsReturnExactlyOnce()
	{
		// ArrayPool's internal bucket/eviction behavior makes a double-Return
		// hard to observe reliably just by renting again afterwards (its
		// caching layers can mask the corruption depending on timing), so
		// this test counts calls to Return directly via a wrapping pool
		// instead of inferring correctness from subsequent Rent identity.
		var counting = new ReturnCountingArrayPool<int>(ArrayPool<int>.Create());

		var parent = new ArrayPoolSegment<int>(16, counting);
		int[] array = parent.Segment.Array!;
		ArrayPoolSegment<int> slice = parent.Slice(4);

		// Disposing the (non-owning) slice must not call Return at all.
		slice.Dispose();
		Assert.Equal(0, counting.ReturnCountFor(array));

		// Disposing the owning parent must call Return exactly once.
		parent.Dispose();
		Assert.Equal(1, counting.ReturnCountFor(array));
	}

	/// <summary>
	/// An <see cref="ArrayPool{T}"/> wrapper that records how many times
	/// <see cref="Return"/> is called for each distinct array instance, so
	/// tests can assert on Return call counts directly rather than inferring
	/// them from subsequent Rent behavior (which is not reliably observable
	/// due to the underlying pool's internal caching).
	/// </summary>
	private sealed class ReturnCountingArrayPool<T>(ArrayPool<T> inner) : ArrayPool<T>
	{
		private readonly Dictionary<T[], int> _returnCounts = new(ReferenceEqualityComparer.Instance);

		public override T[] Rent(int minimumLength) => inner.Rent(minimumLength);

		public override void Return(T[] array, bool clearArray = false)
		{
			lock (_returnCounts)
				_returnCounts[array] = _returnCounts.TryGetValue(array, out int count) ? count + 1 : 1;

			inner.Return(array, clearArray);
		}

		public int ReturnCountFor(T[] array)
		{
			lock (_returnCounts)
				return _returnCounts.TryGetValue(array, out int count) ? count : 0;
		}
	}
}
