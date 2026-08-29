using Open.Threading;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized <see cref="HashSet{T}"/> that uses a <see cref="System.Threading.ReaderWriterLockSlim"/> for thread safety.
/// </summary>
public sealed class ReadWriteSynchronizedHashSet<T>
	: ReadWriteSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
{
	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet() : base([]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity.
	/// </summary>
	/// <param name="collection"></param>
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection) : base([.. collection]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity and comparer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

	// Assumes that .Contains is a thread-safe read-only operation.
	// But any potentially iterative operation will be locked.

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Contains(T item)
		=> InternalSource.Contains(item);

	/// <inheritdoc />
	/// <remarks>
	/// The unsynchronized pre-check keeps a no-op add off the lock entirely.
	/// Beyond that, <see cref="HashSet{T}.Add(T)"/> already reports whether it
	/// added, so delegating to it avoids re-probing under the lock and avoids
	/// allocating a closure to carry the item.
	/// </remarks>
	public new bool Add(T item)
	{
		if (InternalSource.Contains(item)) return false;
		using var write = RWLock.WriteLock();
		return InternalSource.Add(item);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void ExceptWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.ExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void IntersectWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.IntersectWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool Overlaps(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.Overlaps(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool SetEquals(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.SetEquals(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void UnionWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.UnionWith(other);
	}

	/// <inheritdoc />
	public override bool IfContains(T item, Action<HashSet<T>> action)
		=> InternalSource.Contains(item) && base.IfContains(item, action);

	/// <inheritdoc />
	public override bool IfNotContains(T item, Action<HashSet<T>> action)
		=> !InternalSource.Contains(item) && base.IfNotContains(item, action);
}
