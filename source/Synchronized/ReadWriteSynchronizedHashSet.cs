using Open.Threading;
using System;
using System.Collections.Generic;

namespace Open.Collections.Synchronized;

public sealed class ReadWriteSynchronizedHashSet<T> : ReadWriteSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
{
	public ReadWriteSynchronizedHashSet() : base(new HashSet<T>()) { }
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

	// Asumes that .Contains is a thread-safe read-only operation.
	// But any potentially iterative operation will be locked.

	/// <inheritdoc />
	public override bool Contains(T item) => InternalSource.Contains(item);

	/// <inheritdoc />
	public new bool Add(T item) => IfNotContains(item, c => c.Add(item));

	/// <inheritdoc />
	public override bool Remove(T item) => Contains(item) && base.Remove(item);

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<T> other) => Sync.Write(() => InternalSource.ExceptWith(other));

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<T> other) => Sync.Write(() => InternalSource.IntersectWith(other));

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.IsProperSubsetOf(other));

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.IsProperSupersetOf(other));

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.IsSubsetOf(other));

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.IsSupersetOf(other));

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.Overlaps(other));

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<T> other) => Sync.ReadValue(() => InternalSource.SetEquals(other));

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<T> other) => Sync.Write(() => InternalSource.SymmetricExceptWith(other));

	/// <inheritdoc />
	public void UnionWith(IEnumerable<T> other) => Sync.Write(() => InternalSource.UnionWith(other));

	/// <inheritdoc />
	public override bool IfContains(T item, Action<HashSet<T>> action) => InternalSource.Contains(item) && base.IfContains(item, action);

	/// <inheritdoc />
	public override bool IfNotContains(T item, Action<HashSet<T>> action) => !InternalSource.Contains(item) && base.IfNotContains(item, action);
}
