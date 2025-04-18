namespace Open.Collections.Synchronized;

/// <summary>
/// A Monitor synchronized <see cref="HashSet{T}"/>.
/// </summary>
public sealed class LockSynchronizedHashSet<T> : LockSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
{
	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public LockSynchronizedHashSet() : base([]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public LockSynchronizedHashSet(IEnumerable<T> collection) : base([.. collection]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity and comparer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public LockSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

	// Asumes that .Contains is a thread-safe read-only operation.
	// But any potentially iterative operation will be locked.

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override bool Contains(T item)
		=> InternalSource.Contains(item);

	/// <inheritdoc />
	public new bool Add(T item)
		=> IfNotContains(item, c => c.Add(item));

	/// <inheritdoc />
	public override bool Remove(T item)
		=> IfContains(item, c => c.Remove(item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void ExceptWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.ExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void IntersectWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.IntersectWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void UnionWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.UnionWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool Overlaps(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.Overlaps(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool SetEquals(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.SetEquals(other);
	}

	/// <inheritdoc />
	public override bool IfContains(T item, Action<HashSet<T>> action)
		=> Contains(item) && base.IfContains(item, action);

	/// <inheritdoc />
	public override bool IfNotContains(T item, Action<HashSet<T>> action)
		=> !Contains(item) && base.IfNotContains(item, action);
}
