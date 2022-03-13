using System;
using System.Buffers;

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

	public static implicit operator ArraySegment<T> (ArrayPoolSegment<T> segment) => segment.Segment;
	public static implicit operator Memory<T> (ArrayPoolSegment<T> segment) => segment.Segment;
	public static implicit operator ReadOnlyMemory<T>(ArrayPoolSegment<T> segment) => segment.Segment;
}

public static class ArrayPoolExtensions
{
	public static ArrayPoolSegment<T> RentSegment<T>(
		this ArrayPool<T> pool,
		int length,
		bool clearArrayOnDispose = false)
		=> new(length, pool, clearArrayOnDispose);
}
