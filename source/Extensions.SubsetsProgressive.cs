using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Progressively enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// The buffer is filled with the values and returned as the yielded value.
		/// Note: Works especially well when the source is a LazyList.
		/// </summary>
		/// <param name="source">The source list to derive from.</param>
		/// <param name="count">The maximum number of items in the result sets.</param>
		/// <param name="buffer">
		/// A buffer to use instead of returning new arrays for each iteration.
		/// It must be at least the length of the count.
		/// </param>
		/// <returns>An enumerable containing the resultant subsets.</returns>
		public static IEnumerable<T[]> SubsetsProgressive<T>(this IReadOnlyList<T> source, int count, T[] buffer)
		{
			if (count < 1)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
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

			var pool = ArrayPool<int>.Shared;
			var indices = pool.Rent(count);
			try
			{
				using var e = source.GetEnumerator();

				// Setup the first result and make sure there's enough for the count.
				var n = 0;
				for (; n < count; ++n)
				{
					if (!e.MoveNext()) throw new ArgumentOutOfRangeException(nameof(count), count, "Is greater than the length of the source.");
					buffer[n] = e.Current;
					indices[n] = n;
				}

				// First result.
				yield return buffer;

				if (!e.MoveNext()) yield break; // Only one set.

				var lastSlot = count - 1;

				// Second result.
				buffer[lastSlot] = e.Current;
				yield return buffer;
				indices[lastSlot] = n;

				var nextToLastSlot = lastSlot - 1;

			loop:
				var prevIndex = n;
				var pos = nextToLastSlot;

				while (pos >= 0)
				{
					var firstRun = true;
					var index = indices[pos];
					while (index + 1 < prevIndex)
					{
						// Subsequent results.
						buffer[pos] = source[++index];
						indices[pos] = index;
						if (firstRun)
						{
							while (pos < nextToLastSlot && index + 1 < prevIndex)
							{
								buffer[++pos] = source[++index];
								indices[pos] = index;
							}
							prevIndex = indices[pos + 1];
							firstRun = false;
						}
						yield return buffer;
					}
					--pos;
					prevIndex = index;


				}

				if (!e.MoveNext()) yield break;

				// Update the last one.
				buffer[lastSlot] = e.Current;
				for (var i = 0; i < lastSlot; ++i)
				{
					buffer[i] = source[i];
					indices[i] = i;
				}
				yield return buffer;
				indices[lastSlot] = ++n;

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
		public static IEnumerable<ReadOnlyMemory<T>> SubsetsProgressiveBuffered<T>(this IReadOnlyList<T> source, int count)
		{
			var pool = ArrayPool<T>.Shared;
			var buffer = pool.Rent(count);
			var readBuffer = new ReadOnlyMemory<T>(buffer, 0, count);
			try
			{
				foreach (var _ in SubsetsProgressive(source, count, buffer))
					yield return readBuffer;
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
		public static IEnumerable<T[]> SubsetsProgressive<T>(this IReadOnlyList<T> source, int count)
		{
			foreach (var subset in SubsetsProgressiveBuffered(source, count))
				yield return subset.ToArray();
		}

	}
}
