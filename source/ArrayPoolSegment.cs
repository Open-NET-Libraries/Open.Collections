using System.Buffers;
using System.Collections;

namespace Open.Collections;

/// <summary>
/// Represents a segment of an array rented from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
public readonly struct ArrayPoolSegment<T> : IDisposable, IEnumerable<T>
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
	/// <remarks>
	/// The returned segment is a non-owning view: it shares the underlying
	/// rented array with this segment but does <em>not</em> carry a reference
	/// to the <see cref="ArrayPool{T}"/>. Disposing the slice is therefore a
	/// no-op; only disposing the original rented <see cref="ArrayPoolSegment{T}"/>
	/// returns the array to the pool. This prevents a slice from returning a
	/// still-in-use buffer (or double-returning it) while the parent segment
	/// remains alive.
	/// </remarks>
	public ArrayPoolSegment<T> Slice(int index)
		=> new(Segment.Slice(index));

	/// <summary>
	/// Forms a slice out of the segment
	/// starting at the specified <paramref name="index"/>
	/// and extending for the <paramref name="count"/>.
	/// </summary>
	/// <remarks>
	/// The returned segment is a non-owning view: it shares the underlying
	/// rented array with this segment but does <em>not</em> carry a reference
	/// to the <see cref="ArrayPool{T}"/>. Disposing the slice is therefore a
	/// no-op; only disposing the original rented <see cref="ArrayPoolSegment{T}"/>
	/// returns the array to the pool. This prevents a slice from returning a
	/// still-in-use buffer (or double-returning it) while the parent segment
	/// remains alive.
	/// </remarks>
	public ArrayPoolSegment<T> Slice(int index, int count)
		=> new(Segment.Slice(index, count));

	/// <summary>
	/// Returns the array to the pool.
	/// </summary>
	/// <remarks>
	/// Because this is a <see langword="readonly struct"/>, copies made by
	/// assignment, passing by value, or boxing all share the same
	/// <see cref="Pool"/> reference and underlying array. Calling <see cref="Dispose"/> on more than one such
	/// copy of the <em>original</em> rented segment will return the same
	/// array to the pool more than once, which corrupts the pool. Dispose
	/// exactly once per rented segment (e.g. via a single <see langword="using"/>
	/// declaration), and prefer passing it by <see langword="in"/> reference
	/// or as an <see cref="ArraySegment{T}"/>/<see cref="Memory{T}"/> view
	/// (see the implicit conversions below) rather than copying the owning
	/// segment itself. Segments produced by <see cref="Slice(int)"/> or
	/// <see cref="Slice(int, int)"/> do not own the array, so disposing them
	/// is always safe and has no effect.
	/// </remarks>
	/// <inheritdoc />
	public void Dispose() => Pool?.Return(Segment.Array!, _clear);

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() => Segment.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
