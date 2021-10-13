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
		static IEnumerable<T[]> CombinationsCore<T>(IReadOnlyList<T> source, int length, bool distinctSet, T[] buffer)
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

			var pool = length > 128 ? ArrayPool<int>.Shared : null;
			var indexes = pool?.Rent(length) ?? new int[length];
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
					for (var i = 0; i < length; i++)
					{
						buffer![i] = source[indexes[i]];
					}
					yield return buffer!;
				}
			}
			finally
			{
				pool?.Return(indexes);
			}
		}

		static IEnumerable<T[]> CombinationsCore<T>(IReadOnlyList<T> source, int length, bool distinctSet)
		{
			var pool = length > 128 ? ArrayPool<T>.Shared : null;
			var buffer = pool?.Rent(length) ?? new T[length];
			try
			{
				foreach (var b in CombinationsCore(source, length, distinctSet, buffer))
					yield return b;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}


		/// <inheritdoc cref="Combinations{T}(IEnumerable{T}, int)"/>
		/// <param name="buffer">A buffer that is filled with the values and returned as the yielded value instead of a new array.</param>
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements, int length, T[] buffer)
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


		/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T}, int)"/>
		/// <param name="buffer">An optional buffer that is filled with the values and returned as the yielded value instead of a new array</param>
		public static IEnumerable<T[]> CombinationsDistinct<T>(this IEnumerable<T> elements, int length, T[] buffer)
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

			if (uniqueOnly) return source.Subsets(length);
			return uniqueOnly ? source.Subsets(length) : CombinationsCore(source, length, true).Select(e => e.AsCopy(length));
		}

		/// <inheritdoc cref="Combinations{T}(IEnumerable{T}, int)"/>
		/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
		/// <returns>An enumerable the yields as read only memory buffer that should not be retained as its array is returned to pool afterwards.</returns>
		public static IEnumerable<ReadOnlyMemory<T>> CombinationsBuffered<T>(this IEnumerable<T> elements, int length)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0) yield break;

			var pool = length > 128 ? ArrayPool<T>.Shared : null;
			var buffer = pool?.Rent(length) ?? new T[length];
			var readBuffer = new ReadOnlyMemory<T>(buffer, 0, length);
			try
			{
				foreach (var _ in Combinations(elements, length, buffer))
					yield return readBuffer;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}


		/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T}, int)"/>
		/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
		/// <returns>An enumerable the yields as read only memory buffer that should not be retained as its array is returned to pool afterwards.</returns>
		public static IEnumerable<ReadOnlyMemory<T>> CombinationsDistinctBuffered<T>(this IEnumerable<T> elements, int length)
		{
			if (elements is null)
				throw new ArgumentNullException(nameof(elements));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (length == 0) yield break;

			var pool = length > 128 ? ArrayPool<T>.Shared : null;
			var buffer = pool?.Rent(length) ?? new T[length];
			var readBuffer = new ReadOnlyMemory<T>(buffer, 0, length);
			try
			{
				foreach (var _ in CombinationsDistinct(elements, length, buffer))
					yield return readBuffer;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}

		/// <inheritdoc cref="Combinations{T}(IEnumerable{T})"/>
		/// <param name="length">The length of each result.</param>
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements, int length)
		{
			foreach (var c in CombinationsBuffered(elements, length))
				yield return c.ToArray();
		}

		/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T})"/>
		/// <param name="length">The length of each result.</param>
		public static IEnumerable<T[]> CombinationsDistinct<T>(this IEnumerable<T> elements, int length)
		{
			foreach (var c in CombinationsDistinctBuffered(elements, length))
				yield return c.ToArray();
		}


		/// <summary>
		/// Enumerates all possible combinations of values.
		/// Results can be different permutations of another set.
		/// </summary>
		/// <example>[0, 0], [0, 1], [1, 0], [1, 1] where [0, 1] and [1, 0] are a different permutatation of the same set.</example>
		/// <param name="elements">The elements to draw from.</param>
		public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements)
		{
			if (elements is null) throw new ArgumentNullException(nameof(elements));
			Contract.EndContractBlock();
			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			return source.Count == 0 ? Enumerable.Empty<T[]>() : Combinations(source, source.Count);
		}

		/// <summary>
		/// Enumerates all possible distinct set combinations.
		/// In contrast a set that has its items reordered is not distinct from the original.
		/// </summary>
		/// <example>[0, 0], [0, 1], [1, 1] where [1, 0] is not included as it is not a disticnt set from [0, 1].</example>
		/// <param name="elements">The elements to draw from.</param>
		public static IEnumerable<T[]> CombinationsDistinct<T>(this IEnumerable<T> elements)
		{
			if (elements is null) throw new ArgumentNullException(nameof(elements));
			Contract.EndContractBlock();
			var source = elements as IReadOnlyList<T> ?? elements.ToArray();
			return source.Count == 0 ? Enumerable.Empty<T[]>() : CombinationsDistinct(source, source.Count);
		}
	}
}
