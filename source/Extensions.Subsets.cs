using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{

		/// <summary>
		/// Enumerates the possible (ordered) subsets.
		/// If a buffer is supplied, the buffer is filled with the values and returned as the yielded value.
		/// If no buffer is supplised, a new array is created for each subset.
		/// </summary>
		public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count, T[]? buffer = null)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
			if (count > source.Count)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must be less than or equal to the length of the source set.");

			if (count == 1)
			{
				var result = buffer ?? new T[count];
				foreach (var e in source)
				{
					result[0] = e;
					yield return result;
				}
				yield break;
			}
			var pool = ArrayPool<int>.Shared;
			var indices = pool.Rent(count);
			try
			{
				for (int pos = 0, index = 0; ;)
				{
					var result = buffer ?? new T[count];
					for (; pos < count; pos++, index++)
					{
						indices[pos] = index;
						result[pos] = source[index];
					}
					yield return result;
					do
					{
						if (pos == 0) yield break;
						index = indices[--pos] + 1;
					}
					while (index > source.Count - count + pos);
				}
			}
			finally
			{
				pool.Return(indices);
			}
		}

		/// <summary>
		/// Iteratively updates a buffer with the possible subsets.
		/// </summary>
		/// <returns>An enumerable that yields a buffer that is at least the length of the count provided.</returns>
		public static IEnumerable<T[]> SubsetsBuffered<T>(this IReadOnlyList<T> source, int count)
		{
			var pool = ArrayPool<T>.Shared;
			var buffer = pool.Rent(count);

			try
			{
				return Subsets(source, count, buffer);
			}
			finally
			{
				pool.Return(buffer, true);
			}
		}
	}
}
