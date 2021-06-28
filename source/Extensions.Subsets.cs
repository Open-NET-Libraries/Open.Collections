using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Iteratively updates the buffer with the possible subsets.
		/// </summary>
		/// <returns>An enumerable that yields the buffer.</returns>
		public static IEnumerable<T[]> SubsetsBuffered<T>(this IReadOnlyList<T> source, int count, T[] buffer)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
			if (count > source.Count)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must be less than or equal to the length of the source set.");
			if (buffer is null)
				throw new ArgumentNullException(nameof(buffer));


			if (count == 1)
			{
				foreach (var e in source)
				{
					buffer[0] = e;
					yield return buffer;
				}
				yield break;
			}
			var pool = ArrayPool<int>.Shared;
			var indices = pool.Rent(count);
			try
			{
				for (int pos = 0, index = 0; ;)
				{
					for (; pos < count; pos++, index++)
					{
						indices[pos] = index;
						buffer[pos] = source[index];
					}
					yield return buffer;
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
				return SubsetsBuffered(source, count, buffer);
			}
			finally
			{
				pool.Return(buffer);
			}
		}

		/// <summary>
		/// Iteratively updates the provided buffer with the possible subsets of the length of the buffer.
		/// </summary>
		/// <returns>An enumerable that yields a buffer that is at least the length of the count provided.</returns>
		public static IEnumerable<T[]> SubsetsBuffered<T>(this IReadOnlyList<T> source, T[] buffer)
			=> SubsetsBuffered(source, buffer.Length, buffer);


		/// <summary>
		/// Iteratively returns the possible subsets of the source.
		/// </summary>
		/// <returns>An enumerable that yields each subset.</returns>
		public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count)
		{
			foreach (var s in SubsetsBuffered(source, count))
			{
				var a = new T[count];
				s.CopyTo(a, 0);
				yield return a;
			}
		}

	}
}
