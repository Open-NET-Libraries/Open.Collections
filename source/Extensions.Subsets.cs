using System.Buffers;

namespace Open.Collections;

public static partial class Extensions
{
	/// <param name="source">The source list to derive from.</param>
	/// <param name="count">The maximum number of items in the result sets.</param>
	/// <param name="buffer">
	/// A buffer to use instead of returning new arrays for each iteration.
	/// It must be at least the length of the count.
	/// </param>
	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
	public static IEnumerable<Memory<T>> Subsets<T>(this IReadOnlyList<T> source, int count, Memory<T> buffer)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));
		if (count < 1)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
		if (count > source.Count)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Must be less than or equal to the length of the source set.");
		if (buffer.Length < count)
			throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length must be greater than or equal to the provided count.");
		Contract.EndContractBlock();

		return SubsetsCore(source, count, buffer);

		static IEnumerable<Memory<T>> SubsetsCore(IReadOnlyList<T> source, int count, Memory<T> buffer)
		{
			if (count == 1)
			{
				foreach (T? e in source)
				{
					buffer.Span[0] = e;
					yield return buffer;
				}

				yield break;
			}

			// Using an ArrayPool in this manner instead of a MemoryPool does use a few more bytes but is also slightly faster.
			// The result is faster enough to justify using this method.
			int diff = source.Count - count;
			ArrayPool<int>? pool = count > 128 ? ArrayPool<int>.Shared : null;
			int[]? indices = pool?.Rent(count) ?? new int[count];
			try
			{
				int pos = 0;
				int index = 0;

loop:
				var span = buffer.Span;
				while (pos < count)
				{
					indices[pos] = index;
					span[pos] = source[index];
					++pos;
					++index;
				}

				yield return buffer;

				do
				{
					if (pos == 0) yield break;
					index = indices[--pos] + 1;
				}
				while (index > diff + pos);

				goto loop;
			}
			finally
			{
				pool?.Return(indices);
			}
		}
	}

	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
	/// <remarks>Values are yielded as read only memory buffer that should not be retained as its array is returned to pool afterwards.</remarks>
	/// <returns>An enumerable containing the resultant subsets as a memory buffer.</returns>
	public static IEnumerable<ReadOnlyMemory<T>> SubsetsBuffered<T>(this IReadOnlyList<T> source, int count)
	{
		using var lease = MemoryPool<T>.Shared.Rent(count);
		Memory<T> buffer = lease.Memory.Slice(0, count);
		ReadOnlyMemory<T> readBuffer = buffer;
		foreach (Memory<T> _ in Subsets(source, count, buffer))
			yield return readBuffer;
	}

	/// <summary>
	/// Enumerates the possible (ordered) subsets of the list, limited by the provided count.
	/// </summary>
	/// <param name="source">The source list to derive from.</param>
	/// <param name="count">The maximum number of items in the result sets.</param>
	/// <returns>An enumerable containing the resultant subsets.</returns>
	public static IEnumerable<T[]> Subsets<T>(this IReadOnlyList<T> source, int count)
	{
		using var lease = MemoryPool<T>.Shared.Rent(count);
		Memory<T> buffer = lease.Memory.Slice(0, count);
		foreach (Memory<T> _ in Subsets(source, count, buffer))
		{
			var a = new T[count];
			buffer.CopyTo(a);
			yield return a;
		}
	}

	/// <param name="source">The source list to derive from.</param>
	/// <param name="count">The maximum number of items in the result sets.</param>
	/// <param name="pool">The array pool to get result arrays from.</param>
	/// <param name="clearArray"><see langword="true"/>, clears the pooled array when finished; otherwise is left alone.</param>
	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)" />
	public static IEnumerable<ArrayPoolSegment<T>> Subsets<T>(this IReadOnlyList<T> source, int count, ArrayPool<T>? pool, bool clearArray = false)
	{
		foreach (ReadOnlyMemory<T> subset in SubsetsBuffered(source, count))
		{
			var a = new ArrayPoolSegment<T>(count, pool, clearArray);
			subset.CopyTo(a);
			yield return a;
		}
	}

	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int, Memory{T})" />
	public static IEnumerable<Memory<T>> Subsets<T>(this ReadOnlyMemory<T> source, int count, Memory<T> buffer)
	{
		if (count < 1)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Must greater than zero.");
		if (count > source.Length)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Must be less than or equal to the length of the source set.");
		if (buffer.Length < count)
			throw new ArgumentOutOfRangeException(nameof(buffer), buffer, "Length must be greater than or equal to the provided count.");
		Contract.EndContractBlock();

		return SubsetsCore(source, count, buffer);

		static IEnumerable<Memory<T>> SubsetsCore(ReadOnlyMemory<T> source, int count, Memory<T> buffer)
		{
			if (count == 1)
			{
				int len = source.Length;
				for (int i = 0; i < len; ++i)
				{
					buffer.Span[0] = source.Span[i];
					yield return buffer;
				}

				yield break;
			}

			// Using an ArrayPool in this manner instead of a MemoryPool does use a few more bytes but is also slightly faster.
			// The result is faster enough to justify using this method.
			int diff = source.Length - count;
			ArrayPool<int>? pool = count > 128 ? ArrayPool<int>.Shared : null;
			int[]? indices = pool?.Rent(count) ?? new int[count];
			try
			{
				int pos = 0;
				int index = 0;

loop:
				var span = buffer.Span;
				var sourceSpan = source.Span;
				while (pos < count)
				{
					indices[pos] = index;
					span[pos] = sourceSpan[index];
					++pos;
					++index;
				}

				yield return buffer;

				do
				{
					if (pos == 0) yield break;
					index = indices[--pos] + 1;
				}
				while (index > diff + pos);

				goto loop;
			}
			finally
			{
				pool?.Return(indices);
			}
		}
	}

	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
	public static IEnumerable<ReadOnlyMemory<T>> SubsetsBuffered<T>(this ReadOnlyMemory<T> source, int count)
	{
		using var lease = MemoryPool<T>.Shared.Rent(count);
		Memory<T> buffer = lease.Memory.Slice(0, count);
		ReadOnlyMemory<T> readBuffer = buffer;
		foreach (Memory<T> _ in Subsets(source, count, buffer))
			yield return readBuffer;
	}

	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int)"/>
	public static IEnumerable<T[]> Subsets<T>(this ReadOnlyMemory<T> source, int count)
	{
		using var lease = MemoryPool<T>.Shared.Rent(count);
		Memory<T> buffer = lease.Memory.Slice(0, count);
		foreach (Memory<T> _ in Subsets(source, count, buffer))
		{
			var a = new T[count];
			buffer.CopyTo(a);
			yield return a;
		}
	}

	/// <inheritdoc cref="Subsets{T}(IReadOnlyList{T}, int, ArrayPool{T}?, bool)"/>
	public static IEnumerable<ArrayPoolSegment<T>> Subsets<T>(this ReadOnlyMemory<T> source, int count, ArrayPool<T>? pool, bool clearArray = false)
	{
		foreach (ReadOnlyMemory<T> subset in SubsetsBuffered(source, count))
		{
			var a = new ArrayPoolSegment<T>(count, pool, clearArray);
			subset.CopyTo(a);
			yield return a;
		}
	}
}
