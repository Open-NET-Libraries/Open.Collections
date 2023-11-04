using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public static partial class Extensions
{
	static IEnumerable<ArraySegment<T>> CombinationsCore<T>(IReadOnlyList<T> source, int length, bool distinctSet, T[]? buffer = null)
	{
		Debug.Assert(length != 0);
		Debug.Assert(buffer is null || buffer.Length >= length);
		buffer ??= new T[length];
		var bufferSegment = new ArraySegment<T>(buffer, 0, length);

		return CombinationsCore(source, distinctSet, bufferSegment);
	}

	static IEnumerable<ArraySegment<T>> CombinationsCore<T>(IReadOnlyList<T> source, bool distinctSet, ArraySegment<T> buffer)
	{
		Debug.Assert(buffer.Count != 0);
		int count = source.Count;
		Debug.Assert(count != 0);

		int length = buffer.Count;
		{
			var value = source[0];
			Span<T> span = buffer;
			for (int i = 0; i < length; i++) span[i] = value;
		}

		yield return buffer;
		if (count == 1) yield break;

		using var indexesOwner = new ArrayPoolSegment<int>(length, length > 128 ? ArrayPool<int>.Shared : null);
		var indexes = indexesOwner.Segment;
		if (indexesOwner.Pool is null)
		{
			Span<int> span = indexes;
			for (int i = 0; i < length; i++) span[i] = 0;
		}

		int lastIndex = length - 1;
		while (GetNext(distinctSet, count, indexes, lastIndex))
		{
			{
				Span<T> span = buffer;
				ReadOnlySpan<int> inxs = indexes;
				for (int i = 0; i < length; i++)
				{
					span[i] = source[inxs[i]];
				}
			}

			yield return buffer;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool GetNext(bool distinctSet, int count, Span<int> indexes, int lastIndex)
	{
		int i;
		for (i = lastIndex; i >= 0; --i)
		{
			int e = ++indexes[i];
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

		int length = indexes.Length;
		for (++i; i < length; ++i)
		{
			if (indexes[i] == count)
				indexes[i] = distinctSet ? indexes[i - 1] : 0;
		}

		return true;
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="length">The length of each result.</param>
	/// <param name="buffer">A buffer that is filled with the values and returned as the yielded value instead of a new array.</param>
	/// <inheritdoc cref="Combinations{T}(IEnumerable{T}, int)"/>
	public static IEnumerable<ArraySegment<T>> Combinations<T>(this IEnumerable<T> elements, int length, T[] buffer)
	{
		if (elements is null)
			throw new ArgumentNullException(nameof(elements));
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
		Contract.EndContractBlock();

		if (length == 0) return Enumerable.Empty<ArraySegment<T>>();
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		return source.Count == 0 ? Enumerable.Empty<ArraySegment<T>>() : CombinationsCore(source, length, false, buffer);
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">A buffer that is filled with the values and returned as the yielded value instead of a new array</param>
	/// <inheritdoc cref="Combinations{T}(IEnumerable{T}, int)"/>
	public static IEnumerable<ArraySegment<T>> Combinations<T>(this IEnumerable<T> elements, ArraySegment<T> buffer)
	{
		if (elements is null)
			throw new ArgumentNullException(nameof(elements));
		Contract.EndContractBlock();

		if (buffer.Count == 0) return Enumerable.Empty<ArraySegment<T>>();
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		return source.Count == 0 ? Enumerable.Empty<ArraySegment<T>>() : CombinationsCore(source, false, buffer);
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="length">The length of each result.</param>
	/// <param name="buffer">An optional buffer that is filled with the values and returned as the yielded value instead of a new array</param>
	/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T}, int)"/>
	public static IEnumerable<ArraySegment<T>> CombinationsDistinct<T>(this IEnumerable<T> elements, int length, T[] buffer)
	{
		if (elements is null)
			throw new ArgumentNullException(nameof(elements));
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length), length, "Cannot be less than zero.");
		Contract.EndContractBlock();

		if (length == 0) return Enumerable.Empty<ArraySegment<T>>();
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		return source.Count == 0 ? Enumerable.Empty<ArraySegment<T>>() : CombinationsCore(source, length, true, buffer);
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">A buffer that is filled with the values and returned as the yielded value instead of a new array</param>
	/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T}, int)"/>
	public static IEnumerable<ArraySegment<T>> CombinationsDistinct<T>(this IEnumerable<T> elements, ArraySegment<T> buffer)
	{
		if (elements is null)
			throw new ArgumentNullException(nameof(elements));
		Contract.EndContractBlock();

		if (buffer.Count == 0) return Enumerable.Empty<ArraySegment<T>>();
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		return source.Count == 0 ? Enumerable.Empty<ArraySegment<T>>() : CombinationsCore(source, true, buffer);
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
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		int count = source.Count;
		return count == 0
			? Enumerable.Empty<T[]>()
			: uniqueOnly
			? source.Subsets(length)
			: CombinationsCore(source, length, true).Select(e => e.Array.AsCopy(length));
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

		return CombinationsBufferedCore(elements, length);

		static IEnumerable<ReadOnlyMemory<T>> CombinationsBufferedCore(IEnumerable<T> elements, int length)
		{
			if (length == 0) yield break;

			using var owner = new ArrayPoolSegment<T>(length, length > 128 ? ArrayPool<T>.Shared : null);
			var segment = owner.Segment;
			ReadOnlyMemory<T> readBuffer = segment;
			foreach (var _ in Combinations(elements, segment))
				yield return readBuffer;
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

		return CombinationsDistinctBufferedCore(elements, length);

		static IEnumerable<ReadOnlyMemory<T>> CombinationsDistinctBufferedCore(IEnumerable<T> elements, int length)
		{
			if (length == 0) yield break;

			using var owner = new ArrayPoolSegment<T>(length, length > 128 ? ArrayPool<T>.Shared : null);
			var segment = owner.Segment;
			ReadOnlyMemory<T> readBuffer = segment;
			foreach (var _ in CombinationsDistinct(elements, segment))
				yield return readBuffer;
		}
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="length">The length of each result.</param>
	/// <inheritdoc cref="Combinations{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> elements, int length)
	{
		foreach (ReadOnlyMemory<T> c in CombinationsBuffered(elements, length))
			yield return c.ToArray();
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="length">The length of each result.</param>
	/// <inheritdoc cref="CombinationsDistinct{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> CombinationsDistinct<T>(this IEnumerable<T> elements, int length)
	{
		foreach (ReadOnlyMemory<T> c in CombinationsDistinctBuffered(elements, length))
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
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
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
		IReadOnlyList<T>? source = elements as IReadOnlyList<T> ?? elements.ToArray();
		return source.Count == 0 ? Enumerable.Empty<T[]>() : CombinationsDistinct(source, source.Count);
	}
}
