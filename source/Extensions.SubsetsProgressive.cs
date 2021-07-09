using System;
using System.Buffers;
using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <inheritdoc cref="SubsetsProgressive{T}(IReadOnlyList{T}, int)"/>
		/// <param name="buffer">
		/// A buffer to use instead of returning new arrays for each iteration.
		/// It must be at least the length of the count.
		/// </param>
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

			var lastSlot = count - 1;
			var pool = ArrayPool<int>.Shared;
			var indices = pool.Rent(lastSlot);
			try
			{
				using var e = source.GetEnumerator();

				// Setup the first result and make sure there's enough for the count.
				var n = 0;
				for (; n < count; ++n)
				{
					if (!e.MoveNext()) throw new ArgumentOutOfRangeException(nameof(count), count, "Is greater than the length of the source.");
					buffer[n] = e.Current;
				}

				// First result.
				yield return buffer;

				while (e.MoveNext())
				{
					buffer[lastSlot] = e.Current;
					foreach (var _ in Collections.Subsets.IndexesInternal(n, lastSlot, indices))
					{
						for(var i = 0; i<lastSlot; i++)
							buffer[i] = source[indices[i]];

						yield return buffer;
					}
					++n;
				}
			}
			finally
			{
				pool.Return(indices);
			}
		}

		/// <inheritdoc cref="SubsetsProgressive{T}(IReadOnlyList{T}, int)"/>
		/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
		/// <returns>An enumerable containing the resultant subsets as a memory buffer.</returns>
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
		/// Progressively enumerates the possible (ordered) subsets of the list, limited by the provided count.
		/// In contrast to the .Subsets(count) extension, this will produce consistent results regardless of source set size but is not as fast.
		/// Favorable for larger source sets that enumeration may cause evaluation.
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
