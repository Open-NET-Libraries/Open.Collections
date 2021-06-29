using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Enumerates all possible unique permutations of a given set.
		/// [A, B, C] results in [A, B, C], [A, C, B], [B, A, C], [C, A, B], [B, C, A], [C, B, A]
		/// </summary>]
		/// <param name="elements">The elements to derive from.</param>
		/// <param name="buffer">An optional buffer that is filled with the values and returned as the yielded value instead of a new array</param>
		public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> elements, T[]? buffer = null)
		{
			// based on: https://stackoverflow.com/questions/1145703/permutation-of-string-without-recursion

			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			var count = source.Count;
			if (count == 0) yield break;
			if (buffer != null && count > buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length is less than the number of elements.");

			var max = 1;
			for (var i = 2; i <= count; i++) max *= i;

			var a = new int[count];
			var pos = new List<T>(count);

			for (int j = 0; j < max; ++j)
			{
				pos.AddRange(source);

				int i;
				var n = j;
				var c = 0;

				for (i = count; i > 0; --i)
				{
					var m = n; n /= i;
					a[c++] = m % i;
				}

				// Avoid copy if not needed.
				var result = buffer ?? new T[count];
				for (i = 0; i < count; i++)
				{
					var index = a[i];
					result[i] = pos[index];
					pos.RemoveAt(index);
				}

				yield return result;
			}
		}

		/// <summary>
		/// Enumerates all possible unique permutations of a given set.
		/// Values are yielded as a reused array.
		/// 
		/// Example:
		/// [A, B, C] results in [A, B, C], [A, C, B], [B, A, C], [C, A, B], [B, C, A], [C, B, A]
		/// </summary>]
		/// <param name="elements">The elements to derive from.</param>
		public static IEnumerable<T[]> PermutationsBuffered<T>(this IReadOnlyList<T> elements)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			Contract.EndContractBlock();

			var count = elements.Count;
			if (count == 0) yield break;

			var pool = ArrayPool<T>.Shared;
			var buffer = pool.Rent(count);
			try
			{
				foreach (var p in Permutations(elements, buffer))
					yield return p;
			}
			finally
			{
				pool.Return(buffer, true);
			}
		}
	}
}
