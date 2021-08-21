using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Open.Collections
{
	public static class Subsets
	{
		internal static IEnumerable<int[]> IndexesInternal(int sourceLength, int subsetLength, int[] buffer)
		{
			if (subsetLength == 1)
			{
				for (var i = 0; i < sourceLength; ++i)
				{
					buffer[0] = i;
					yield return buffer;
				}
				yield break;
			}

			var diff = sourceLength - subsetLength;

			var pos = 0;
			var index = 0;

		loop:
			while (pos < subsetLength)
			{
				buffer[pos] = index;
				++pos;
				++index;
			}

			yield return buffer;

			do
			{
				if (pos == 0) yield break;
				index = buffer[--pos] + 1;
			}
			while (index > diff + pos);

			goto loop;
		}

		/// <inheritdoc cref="Indexes(int, int)"/>
		/// <param name="buffer">
		/// A buffer to use instead of returning new arrays for each iteration.
		/// It must be at least the length of the count.
		/// </param>
		public static IEnumerable<int[]> Indexes(int sourceLength, int subsetLength, int[] buffer)
		{

			if (subsetLength < 1)
				throw new ArgumentOutOfRangeException(nameof(subsetLength), subsetLength, "Must greater than zero.");
			if (subsetLength > sourceLength)
				throw new ArgumentOutOfRangeException(nameof(subsetLength), subsetLength, "Must be less than or equal to the length of the source set.");
			if (buffer is null)
				throw new ArgumentNullException(nameof(buffer));
			if (buffer.Length < subsetLength)
				throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length must be greater than or equal to the provided count.");

			return IndexesInternal(sourceLength, subsetLength, buffer);
		}

		/// <inheritdoc cref="Indexes(int, int)"/>
		/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
		public static IEnumerable<ReadOnlyMemory<int>> IndexesBuffered(int sourceLength, int subsetLength)
		{
			var pool = subsetLength > 128 ? ArrayPool<int>.Shared : null;
			var buffer = pool?.Rent(subsetLength) ?? new int[subsetLength];
			var readBuffer = new ReadOnlyMemory<int>(buffer, 0, subsetLength);
			try
			{
				foreach (var _ in Indexes(sourceLength, subsetLength, buffer))
					yield return readBuffer;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}

		/// <summary>
		/// Enumerates all the possible subset indexes for a given source set length and subset length.
		/// </summary>
		/// <param name="sourceLength">The length of the source set.</param>
		/// <param name="subsetLength">The size of the desired subsets.</param>s
		public static IEnumerable<int[]> Indexes(int sourceLength, int subsetLength)
		{
			foreach (var i in IndexesBuffered(sourceLength, subsetLength))
				yield return i.ToArray();
		}

		/// <inheritdoc cref="Indexes(int, int)"/>
		public static IEnumerable<ImmutableArray<int>> IndexesImmutable(int sourceLength, int subsetLength)
		{
			foreach (var i in IndexesBuffered(sourceLength, subsetLength))
				yield return i.ToImmutableArray();
		}
	}

}
