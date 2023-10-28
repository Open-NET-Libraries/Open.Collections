using Open.Disposable;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public static partial class Extensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static IEnumerable<T[]> PermutationsCore<T>(Action<LinkedList<T>> appendElements, T[] buffer, int count)
	{
		if (count == 0) yield break;
		if (count > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length is less than the number of elements.");

		// based on: https://stackoverflow.com/questions/1145703/permutation-of-string-without-recursion

		int max = 1;
		for (int i = 2; i <= count; i++) max *= i;

		int[]? a = new int[count];
		var pos = new LinkedList<T>();

		for (int j = 0; j < max; ++j)
		{
			appendElements(pos);

			int i;
			int n = j;
			int c = 0;

			for (i = count; i > 0; --i)
			{
				int m = n; n /= i;
				a[c++] = m % i;
			}

			// Avoid copy if not needed.
			for (i = 0; i < count; i++)
			{
				int index = a[i];
				var node = pos.First;
				for (int ni = 0; ni < index; ni++)
					node = node.Next;
				buffer[i] = node.Value;
				pos.Remove(node);
			}

			yield return buffer;
		}
	}

	static IEnumerable<T[]> PermutationsCore<T>(IReadOnlyCollection<T> elements, T[] buffer)
	{
		int count = elements.Count;
		if (count == 0) return Enumerable.Empty<T[]>();
		if (count > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length is less than the number of elements.");
		Contract.EndContractBlock();

		return PermutationsCore(pos => pos.AddRange(elements), buffer, count);
	}

	static IEnumerable<T[]> PermutationsCore<T>(ReadOnlyMemory<T> elements, T[] buffer)
	{
		int count = elements.Length;
		if (count == 0) return Enumerable.Empty<T[]>();
		if (count > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length is less than the number of elements.");
		Contract.EndContractBlock();

		return PermutationsCore(pos =>
		{
			foreach (var e in elements.Span)
				pos.AddLast(e);
		}, buffer, count);
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">The buffer array that is filled with the values and returned as the yielded value instead of a new array</param>
	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> Permutations<T>(this IReadOnlyCollection<T> elements, T[] buffer)
	{
		if (elements is null) throw new ArgumentNullException(nameof(elements));
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		Contract.EndContractBlock();

		return PermutationsCore(elements, buffer);
	}

	/// <param name="elements">The elements to draw from.</param>
	/// <param name="buffer">The buffer array that is filled with the values and returned as the yielded value instead of a new array</param>
	/// <inheritdoc cref="Permutations{T}(IEnumerable{T})"/>
	public static IEnumerable<T[]> Permutations<T>(this ReadOnlyMemory<T> elements, T[] buffer)
	{
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		Contract.EndContractBlock();

		return PermutationsCore(elements, buffer);
	}

	/// <inheritdoc cref="Permutations{T}(IReadOnlyCollection{T}, T[])"/>
	public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> elements, T[] buffer)
	{
		if (elements is null) throw new ArgumentNullException(nameof(elements));
		if (buffer is null) throw new ArgumentNullException(nameof(buffer));
		Contract.EndContractBlock();

		return PermutationsCore(elements is IReadOnlyCollection<T> c ? c : elements.ToArray(), buffer);
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

			ArrayPool<T>? pool = count > 128 ? ArrayPool<T>.Shared : null;
			T[]? buffer = pool?.Rent(count) ?? new T[count];
			var readBuffer = new ReadOnlyMemory<T>(buffer, 0, count);
			try
			{
				foreach (T[]? _ in PermutationsCore(elements, buffer))
					yield return readBuffer;
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

		ArrayPool<T>? pool = count > 128 ? ArrayPool<T>.Shared : null;
		T[]? buffer = pool?.Rent(count) ?? new T[count];
		var readBuffer = new ReadOnlyMemory<T>(buffer, 0, count);
		try
		{
			foreach (T[]? _ in PermutationsCore(elements, buffer))
				yield return readBuffer;
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
	/// Enumerates all possible unique permutations of a given set.
	/// </summary>
	/// <example>[A, B, C] results in [A, B, C], [A, C, B], [B, A, C], [C, A, B], [B, C, A], [C, B, A]</example>
	/// <param name="elements">The elements to derive from.</param>
	public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> elements)
	{
		foreach (ReadOnlyMemory<T> p in PermutationsBuffered(elements))
			yield return p.ToArray();
	}
}
