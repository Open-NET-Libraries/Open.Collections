using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
		/// <param name="buffer">
		/// A buffer to use instead of returning new arrays for each iteration.
		/// It must be at least the length of the count.
		/// </param>
		public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count, T[] buffer)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
			if (count > source.Count)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must be less than or equal to the length of the source set.");
			if (buffer is null)
				throw new ArgumentNullException(nameof(buffer));
			if (buffer.Length < count)
				throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length must be greater than or equal to the provided count.");


			if (count == 1)
			{
				foreach (var e in source)
				{
					buffer[0] = e;
					yield return buffer;
				}
				yield break;
			}

			var diff = source.Count - count;
			var pool = count>128 ? ArrayPool<int>.Shared : null;
			var indices = pool?.Rent(count) ?? new int[count];
			try
			{
				var pos = 0;
				var index = 0;

			loop:
				while (pos < count)
				{
					indices[pos] = index;
					buffer[pos] = source[index];
					++pos;
					++index;
				}

				yield return buffer;

				do
				{
					if (pos == 0) yield break;
					index = indices[--pos] + 1;
				}
				while (index > diff + pos);

				goto loop;
			}
			finally
			{
				pool?.Return(indices);
			}
		}

		/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
		/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
		/// <returns>An enumerable containing the resultant subsets as a memory buffer.</returns>
		public static IEnumerable<ReadOnlyMemory<T>> SubsetsBuffered<T>(this IReadOnlyList<T> source, int count)
		{
			var pool = count > 128 ? ArrayPool<T>.Shared : null;
			var buffer = pool?.Rent(count) ?? new T[count];
			var readBuffer = new ReadOnlyMemory<T>(buffer, 0, count);
			try
			{
				foreach (var _ in Subsets(source, count, buffer))
					yield return readBuffer;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}


		/// <summary>
		/// Enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// </summary>
		/// <param name="source">The source list to derive from.</param>
		/// <param name="count">The maximum number of items in the result sets.</param>
		/// <returns>An enumerable containing the resultant subsets.</returns>
		public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count)
		{
			foreach (var subset in SubsetsBuffered(source, count))
				yield return subset.ToArray();
		}

	}
}
