using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

/// <summary>
/// Represents a segment of an array rented from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
public readonly struct ArrayPoolSegment<T> : IDisposable
{
	/// <summary>
	/// The segment of the array.
	/// </summary>
	public readonly ArraySegment<T> Segment;

	/// <summary>
	/// The <see cref="ArrayPool{T}"/> used to rent the array.
	/// </summary>
	public readonly ArrayPool<T>? Pool;

	private readonly bool _clear;

	/// <summary>
	/// Constructs a new <see cref="ArrayPoolSegment{T}"/>.
	/// </summary>
	public ArrayPoolSegment(
		ArraySegment<T> segment,
		ArrayPool<T>? pool = null,
		bool clearArrayOnDispose = false)
	{
		Segment = segment;
		Pool = pool;
		_clear = clearArrayOnDispose;
	}

	/// <summary>
	/// Constructs a new <see cref="ArrayPoolSegment{T}"/> from the <see cref="ArrayPool{T}"/>.
	/// </summary>
	public ArrayPoolSegment(
		int length,
		ArrayPool<T>? pool = null,
		bool clearArrayOnDispose = false)
	{
		Pool = pool;
		T[]? array = pool?.Rent(length) ?? new T[length];
		Segment = new(array, 0, length);
		_clear = clearArrayOnDispose;
	}

	/// <summary>
	/// Forms a slice out of the segment
	/// starting at the specified <paramref name="index"/>.
	/// </summary>
	public ArrayPoolSegment<T> Slice(int index)
		=> new(Segment.Slice(index), Pool, _clear);

	/// <summary>
	/// Forms a slice out of the segment
	/// starting at the specified <paramref name="index"/>
	/// and extending for the <paramref name="count"/>.
	/// </summary>
	public ArrayPoolSegment<T> Slice(int index, int count)
		=> new(Segment.Slice(index, count), Pool, _clear);

	/// <summary>
	/// Returns the array to the pool.
	/// </summary>
	/// <inheritdoc />
	public void Dispose() => Pool?.Return(Segment.Array!, _clear);

	/// <summary>
	/// Implicitly converts the <see cref="ArrayPoolSegment{T}"/> to an <see cref="ArraySegment{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static implicit operator ArraySegment<T>(ArrayPoolSegment<T> segment) => segment.Segment;

	/// <summary>
	/// Implicitly converts the <see cref="ArrayPoolSegment{T}"/> to a <see cref="Memory{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static implicit operator Memory<T>(ArrayPoolSegment<T> segment) => segment.Segment;

	/// <summary>
	/// Implicitly converts the <see cref="ArrayPoolSegment{T}"/> to a <see cref="ReadOnlyMemory{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static implicit operator ReadOnlyMemory<T>(ArrayPoolSegment<T> segment) => segment.Segment;

	/// <summary>
	/// Implicitly converts the <see cref="ReadOnlySpan{T}"/> to a <see cref="ReadOnlyMemory{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static implicit operator ReadOnlySpan<T>(ArrayPoolSegment<T> segment) => segment.Segment;

	/// <summary>
	/// Implicitly converts the <see cref="Span{T}"/> to a <see cref="ReadOnlyMemory{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static implicit operator Span<T>(ArrayPoolSegment<T> segment) => segment.Segment;
}

/// <summary>
/// Extension methods for <see cref="ArrayPool{T}"/>.
/// </summary>
public static class ArrayPoolExtensions
{
	/// <summary>
	/// Creates a new <see cref="ArrayPoolSegment{T}"/> from the <see cref="ArrayPool{T}"/>.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static ArrayPoolSegment<T> RentSegment<T>(
		this ArrayPool<T> pool,
		int length,
		bool clearArrayOnDispose = false)
		=> new(length, pool, clearArrayOnDispose);
}
