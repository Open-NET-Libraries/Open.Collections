using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

public readonly struct ArrayPoolSegment<T> : IDisposable
{
	public readonly ArraySegment<T> Segment;
	public readonly ArrayPool<T>? Pool;
	private readonly bool _clear;

	public ArrayPoolSegment(
		int length,
		ArrayPool<T>? pool = null,
		bool clearArrayOnDispose = false)
	{
		_clear = clearArrayOnDispose;
		Pool = pool;
		T[]? array = pool?.Rent(length) ?? new T[length];
		Segment = new(array, 0, length);
	}

	public void Dispose() => Pool?.Return(Segment.Array, _clear);

	[ExcludeFromCodeCoverage]
	public static implicit operator ArraySegment<T>(ArrayPoolSegment<T> segment) => segment.Segment;
	[ExcludeFromCodeCoverage]
	public static implicit operator Memory<T>(ArrayPoolSegment<T> segment) => segment.Segment;
	[ExcludeFromCodeCoverage]
	public static implicit operator ReadOnlyMemory<T>(ArrayPoolSegment<T> segment) => segment.Segment;
	[ExcludeFromCodeCoverage]
	public static implicit operator ReadOnlySpan<T>(ArrayPoolSegment<T> segment) => segment.Segment;
	[ExcludeFromCodeCoverage]
	public static implicit operator Span<T>(ArrayPoolSegment<T> segment) => segment.Segment;
}

public static class ArrayPoolExtensions
{
	[ExcludeFromCodeCoverage]
	public static ArrayPoolSegment<T> RentSegment<T>(
		this ArrayPool<T> pool,
		int length,
		bool clearArrayOnDispose = false)
		=> new(length, pool, clearArrayOnDispose);
}
