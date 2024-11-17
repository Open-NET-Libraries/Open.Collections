using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public static partial class Extensions
{
	/// <summary>
	/// Uses Heap's algorithm to iterate through all possible permutations.
	/// </summary>
	public static IEnumerable<ReadOnlyMemory<T>> Permutations<T>(this Memory<T> set)
	{
		int n = set.Length;
		if (n == 0) yield break;
		yield return set;
		if (n == 1) yield break;

		var lease = MemoryPool<int>.Shared.Rent(n);
		var c = lease.Memory.Slice(0, n);
		c.Span.Clear(); // Initialize counter array

		int i = 0;
		while (i < n)
		{
			int cValue = c.Span[i];
			if (cValue < i)
			{
				var span = set.Span;
				if (i % 2 == 0)
					Swap(ref span[0], ref span[i]);
				else
					Swap(ref span[cValue], ref span[i]);

				yield return set;

				c.Span[i] = cValue + 1;
				i = 0;
			}
			else
			{
				c.Span[i] = 0;
				i++;
			}
		}
	}

	/// <inheritdoc cref="Permutations{T}(Memory{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> Permutations<T>(this T[] set)
		=> set.AsMemory().Permutations();

	/// <summary>
	/// <para>A fast, memory efficient way to get a unique permutation based upon an index of all possible permutations.</para>
	/// <para>Modifies the source span to produce the results. Be sure to copy the source if you want to keep it.</para>
	/// </summary>
	/// <param name="source">The set to permute.</param>
	/// <param name="n">The index of the permutation.</param>
	/// <remarks>Is not in lexicographic order.</remarks>
	/// <returns>The <paramref name="source"/> <see cref="Span{T}"/> permuted as a <see cref="ReadOnlySpan{T}"/>.</returns>
	public static ReadOnlySpan<T> Permutation<T>(this Span<T> source, BigInteger n)
	{
		if (n <= ulong.MaxValue)
		{
			return n < 0
				? throw new ArgumentOutOfRangeException(nameof(n), n, "Must be at least zero.")
				: Permutation(source, (ulong)n);
		}

		int size = source.Length;
		int i = 0, j;
		for (; i < size; ++i)
		{
			if (n <= long.MaxValue)
				return Permutation(source, (long)n, i);

			j = i + 1;
			int index = (int)(n % j);
			if (index != i)
				Swap(ref source[index], ref source[i]);
			n /= j;
		}

		return source;
	}

	/// <inheritdoc cref="Permutation{T}(Span{T}, BigInteger)"/>
	public static ReadOnlySpan<T> Permutation<T>(this Span<T> source, ulong n)
	{
		int size = source.Length;
		int i = 0;
		uint j = 0;
		for (; i < size; ++i)
		{
			if (n <= long.MaxValue)
				return Permutation(source, (long)n, i);

			int index = (int)(n % ++j);
			if (index != i)
				Swap(ref source[index], ref source[i]);
			n /= j;
		}

		return source;
	}

	/// <inheritdoc cref="Permutation{T}(Span{T}, BigInteger)"/>
	public static ReadOnlySpan<T> Permutation<T>(this Span<T> source, long n, int i = 0)
	{
		int size = source.Length;
		for (int j; i < size; ++i)
		{
			if (n < int.MaxValue)
				return Permutation(source, (int)n, i);

			j = i + 1;
			int index = (int)(n % j);
			if (index != i)
				Swap(ref source[index], ref source[i]);
			n /= j;
		}

		return source;
	}

	/// <inheritdoc cref="Permutation{T}(Span{T}, BigInteger)"/>
	public static ReadOnlySpan<T> Permutation<T>(this Span<T> source, int n, int i = 0)
	{
		if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), n, "Must be at least zero.");

		if (n == 0) return source;
		n--;

		int size = source.Length;
		for (int j; i < size; ++i)
		{
			j = i + 1;
			int index = n % j;
			if (index != i)
				Swap(ref source[index], ref source[i]);
			n /= j;
		}

		return source;
	}

	/// <summary>
	/// Permutes the <paramref name="span"/> to the next permutation.
	/// </summary>
	/// <param name="span">The sequence to permute.</param>
	/// <returns>
	/// <remarks>To reset the sequence after reaching the end (returns false) simply reverse the <paramref name="span"/>.</remarks>
	/// <see langword="true"/> if there are more permutations in the sequence; otherwise <see langword="false"/>.
	/// </returns>
	public static bool NextLexicographic<T>(this Span<T> span)
		where T : IComparable<T>
	{
		// 1. Find the largest index i such that array[i] < array[i + 1]
		int i = span.Length - 2;
		while (i >= 0 && span[i].CompareTo(span[i + 1]) >= 0) i--;

		// If no such index exists, the permutation is the last permutation
		if (i < 0)
			return false;

		// 2. Find the largest index j greater than i such that array[i] < array[j]
		int j = span.Length - 1;
		while (span[i].CompareTo(span[j]) >= 0) j--;

		// 3. Swap the values at array[i] and array[j]
		Swap(ref span[i], ref span[j]);

		// 4. Reverse the sequence from array[i + 1] up to the last element
		span.Slice(i + 1).Reverse();

		return true;
	}

	/// <inheritdoc cref="NextLexicographic{T}(Span{T})"/>
	public static bool NextLexicographic(this Span<int> span)
	{
		// 1. Find the largest index i such that span[i] < span[i + 1]
		int i = span.Length - 2;
		while (i >= 0 && span[i] >= span[i + 1]) i--;

		// If no such index exists, the permutation is the last permutation
		if (i < 0)
			return false;

		// 2. Find the largest index j greater than i such that span[i] < span[j]
		int j = span.Length - 1;
		while (span[i] >= span[j]) j--;

		// 3. Swap the values at span[i] and span[j]
		Swap(ref span[i], ref span[j]);

		// 4. Reverse the sequence from span[i + 1] up to the last element
		span.Slice(i + 1).Reverse();

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap<T>(ref T a, ref T b)
		=> (b, a) = (a, b);

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">The buffer that is filled with the values and returned as the yielded value instead of a copy.</param>
	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> Permutations<T>(this IReadOnlyCollection<T> elements, Memory<T> buffer)
	{
		if (elements is null) throw new ArgumentNullException(nameof(elements));

		int count = elements.Count;
		if (count == 0) return Enumerable.Empty<ReadOnlyMemory<T>>();
		Contract.EndContractBlock();

		var b = buffer.Slice(0, elements.Count);
		elements.CopyToSpan(b.Span);

		return b.Permutations();
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">The buffer that is filled with the values and returned as the yielded value instead of a copy.</param>
	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> Permutations<T>(this ReadOnlyMemory<T> elements, Memory<T> buffer)
	{
		int count = elements.Length;
		if (count == 0) return Enumerable.Empty<ReadOnlyMemory<T>>();
		Contract.EndContractBlock();

		var b = buffer.Slice(0, elements.Length);
		elements.CopyTo(b);

		return b.Permutations();
	}

	/// <inheritdoc cref="Permutations{T}(IReadOnlyCollection{T}, Memory{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> Permutations<T>(this IEnumerable<T> elements, Memory<T> buffer)
	{
		if (elements is null) throw new ArgumentNullException(nameof(elements));
		Contract.EndContractBlock();

		return elements is IReadOnlyCollection<T> c
			? Permutations(c, buffer)
			: Permutations(elements.ToArray().AsMemory(), buffer);
	}

	/// <inheritdoc cref="PermutationsBuffered{T}(IEnumerable{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> PermutationsBuffered<T>(this IReadOnlyCollection<T> elements)
	{
		if (elements is null)
			throw new ArgumentNullException(nameof(elements));
		Contract.EndContractBlock();

		return PermutationsBufferedCore(elements);

		static IEnumerable<ReadOnlyMemory<T>> PermutationsBufferedCore(IReadOnlyCollection<T> elements)
		{
			int count = elements.Count;
			if (count == 0) yield break;

			// We're using an ArrayPool here instead of a MemoryPool because benchmarking shows it can be slightly faster for smaller sizes.
			ArrayPool<T>? pool = count > 128 ? ArrayPool<T>.Shared : null;
			T[]? buffer = pool?.Rent(count) ?? new T[count];
			try
			{
				var m = buffer.AsMemory().Slice(0, count);
				elements.CopyToSpan(m.Span);
				foreach (var b in m.Permutations())
					yield return b;
			}
			finally
			{
				pool?.Return(buffer, true);
			}
		}
	}

	/// <inheritdoc cref="PermutationsBuffered{T}(IEnumerable{T})"/>
	public static IEnumerable<ReadOnlyMemory<T>> PermutationsBuffered<T>(this ReadOnlyMemory<T> elements)
	{
		int count = elements.Length;
		if (count == 0) yield break;

		// We're using an ArrayPool here instead of a MemoryPool because benchmarking shows it can be slightly faster for smaller sizes.
		ArrayPool<T>? pool = count > 128 ? ArrayPool<T>.Shared : null;
		T[]? buffer = pool?.Rent(count) ?? new T[count];
		try
		{
			var m = buffer.AsMemory().Slice(0, count);
			elements.CopyTo(m);
			foreach (var b in Permutations(elements, buffer))
				yield return b;
		}
		finally
		{
			pool?.Return(buffer, true);
		}
	}

	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
	public static IEnumerable<ReadOnlyMemory<T>> PermutationsBuffered<T>(this IEnumerable<T> elements)
	{
		if (elements is null) throw new ArgumentNullException(nameof(elements));
		Contract.EndContractBlock();

		return PermutationsBuffered(elements is IReadOnlyCollection<T> c ? c : elements.ToArray());
	}

	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> Permutations<T>(this IReadOnlyCollection<T> elements)
	{
		foreach (ReadOnlyMemory<T> p in PermutationsBuffered(elements))
			yield return p.ToArray();
	}

	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> Permutations<T>(this ReadOnlyMemory<T> elements)
	{
		foreach (ReadOnlyMemory<T> p in PermutationsBuffered(elements))
			yield return p.ToArray();
	}

	/// <summary>
	/// Using Heap's algorithm, enumerates all possible unique permutations of a given set.
	/// </summary>
	/// <example>[A, B, C] results in [A, B, C], [B, A, C], [C, A, B], [A, C, B], [B, C, A], [C, B, A]</example>
	/// <param name="elements">The elements to derive from.</param>
	public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> elements)
	{
		foreach (ReadOnlyMemory<T> p in PermutationsBuffered(elements))
			yield return p.ToArray();
	}
}
