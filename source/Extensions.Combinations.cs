using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Open.Collections
{
	public static partial class Extensions
	{
		static IEnumerable<T[]> CombinationsCore<T>(IReadOnlyList<T> source, int length, bool distinctSet, T[]? buffer = null)
		{
			Debug.Assert(length != 0);
			var count = source.Count;
			Debug.Assert(count != 0);

			{
				var value = source[0];
				var result = buffer ?? new T[length];
				for (var i = 0; i < length; i++) result[i] = value;
				yield return result;
				if (count == 1) yield break;
			}

			var pool = ArrayPool<int>.Shared;
			var indexes = pool.Rent(length);
			try
			{

				for (var i = 0; i < length; i++) indexes[i] = 0;

				var lastIndex = length - 1;
				bool GetNext()
				{
					int i;
					for (i = lastIndex; i >= 0; --i)
					{
						var e = ++indexes[i];
						if (count == e)
						{
							if (i == 0) return false;
						}
						else
						{
							if (i == lastIndex) return true;
							else break;
						}
					}

					for (++i; i < length; ++i)
					{
						if (indexes[i] == count)
							indexes[i] = distinctSet ? indexes[i - 1] : 0;
					}

					return true;
				}

				while (GetNext())
				{
					var result = buffer ?? new T[length];
					for (var i = 0; i < length; i++)
					{
						result[i] = source[indexes[i]];
					}
					yield return result;
				}
			}
			finally
			{
				pool.Return(indexes);
			}
		}

		/// <summary>
		/// Enumerates all possible combinations of values.
		/// Results can be different permutations of another set.
		/// Examples:
		/// [0, 0], [0, 1], [1, 0], [1, 1] where [0, 1] and [1, 0] are a different permutatation of the same set.
		/// </summary>
		/// <param name="elements">The elements to draw from.</param>
		/// <param name="length">The length of each result.</param>
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements, int length, T[]? buffer = null)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0) return Enumerable.Empty<T[]>();
			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			return source.Count == 0 ? Enumerable.Empty<T[]>() : CombinationsCore(source, length, false, buffer);
		}

		/// <summary>
		/// Enumerates all possible distinct set combinations.
		/// A set that has its items reordered is not distinct from the original.
		/// Examples:
		/// [0, 0], [0, 1], [1, 1] where [1, 0] is not included as it is not a disticnt set from [0, 1].
		///
		/// </summary>
		/// <param name="elements">The elements to draw from.</param>
		/// <param name="length">The length of each result.</param>
		public static IEnumerable<T[]> CombinationsDistinct<T>(this IEnumerable<T> elements, int length, T[]? buffer = null)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0) return Enumerable.Empty<T[]>();
			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			return source.Count == 0 ? Enumerable.Empty<T[]>() : CombinationsCore(source, length, true, buffer);
		}

		[Obsolete("Deprecated in favor of using .Subsets(length) or .Combinations(length) depending on intent.")]
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements, int length, bool uniqueOnly)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0) return Enumerable.Empty<T[]>();
			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			var count = source.Count;
			if (count == 0) return Enumerable.Empty<T[]>();

			return uniqueOnly ? source.Subsets(length) : CombinationsCore(source, length, true);
		}

		/// <summary>
		/// Enumerates all possible (order retained) combinations of the source elements up to the length.
		/// </summary>
		/// <param name="elements">The elements to draw from.</param>
		/// <param name="length">The length of each result.</param>
		/// <param name="uniqueOnly">Finds all possible subsets instead of all possible combinations of values.</param>
		public static IEnumerable<T[]> CombinationsBuffered<T>(this IEnumerable<T> elements, int length)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0)
			{
				return Enumerable.Empty<T[]>();
			}

			var arrayPool = ArrayPool<T>.Shared;
			var buffer = arrayPool.Rent(length);
			try
			{
				return Combinations(elements, length, buffer);
			}
			finally
			{
				arrayPool.Return(buffer, true);
			}
		}

	}
}
