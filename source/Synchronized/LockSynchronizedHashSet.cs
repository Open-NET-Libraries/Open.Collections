using System;
using System.Collections.Generic;

namespace Open.Collections.Synchronized;

public sealed class LockSynchronizedHashSet<T> : LockSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
{
	public LockSynchronizedHashSet() : base(new HashSet<T>()) { }
	public LockSynchronizedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
	public LockSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

	// Asumes that .Contains is a thread-safe read-only operation.
	// But any potentially iterative operation will be locked.

	/// <inheritdoc />
	public override bool Contains(T item) => InternalSource.Contains(item);

	/// <inheritdoc />
	public new bool Add(T item) => IfNotContains(item, c => c.Add(item));

	/// <inheritdoc />
	public override bool Remove(T item) => Contains(item) && base.Remove(item);

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.ExceptWith(other);
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.IntersectWith(other);
	}

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	public void UnionWith(IEnumerable<T> other)
	{
		lock (Sync) InternalSource.UnionWith(other);
	}

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.IsSupersetOf(other);
	}

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.Overlaps(other);
	}

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<T> other)
	{
		lock (Sync) return InternalSource.SetEquals(other);
	}

	/// <inheritdoc />
	public override bool IfContains(T item, Action<HashSet<T>> action) => InternalSource.Contains(item) && base.IfContains(item, action);

	/// <inheritdoc />
	public override bool IfNotContains(T item, Action<HashSet<T>> action) => !InternalSource.Contains(item) && base.IfNotContains(item, action);
}
