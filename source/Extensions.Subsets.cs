using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// The buffer is filled with the values and returned as the yielded value.
		/// </summary>
		/// <param name="source">The source list to derive from.</param>
		/// <param name="count">The maximum number of items in the result sets.</param>
		/// <param name="buffer">
		/// A buffer to use instead of returning new arrays for each iteration.
		/// It must be at least the length of the count.
		/// </param>
		/// <returns>An enumerable containing the resultant subsets.</returns>
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
			var pool = ArrayPool<int>.Shared;
			var indices = pool.Rent(count);
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
				pool.Return(indices);
			}
		}

		/// <summary>
		/// Enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// The yielded results are a buffer (array) that is at least the length of the provided count.
		/// NOTE: Do not retain the result array as it is returned to an array pool when complete.
		/// </summary>
		/// <param name="source">The source list to derive from.</param>
		/// <param name="count">The maximum number of items in the result sets.</param>
		/// <returns>An enumerable containing the resultant subsets as an buffer array.</returns>
		public static IEnumerable<T[]> SubsetsBuffered<T>(this IReadOnlyList<T> source, int count)
		{
			var pool = ArrayPool<T>.Shared;
			var buffer = pool.Rent(count);

			try
			{
				foreach (var subset in Subsets(source, count, buffer))
					yield return subset;
			}
			finally
			{
				pool.Return(buffer, true);
			}
		}


		/// <summary>
		/// Enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// A new array is created for each subset.
		/// </summary>
		/// <param name="source">The source list to derive from.</param>
		/// <param name="count">The maximum number of items in the result sets.</param>
		/// <returns>An enumerable containing the resultant subsets.</returns>
		public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count)
		{
			foreach (var subset in SubsetsBuffered(source, count))
				yield return subset.AsCopy(count);
		}

	}
}
