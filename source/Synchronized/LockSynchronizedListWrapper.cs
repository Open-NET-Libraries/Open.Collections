using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

[ExcludeFromCodeCoverage]
public class LockSynchronizedListWrapper<T, TList>(
	TList list, bool owner = false)
	: LockSynchronizedCollectionWrapper<T, TList>(list, owner), IList<T>
	where TList : class, IList<T>
{
	// This is a simplified version.
	// It could be possible to allow indexed values to change independently of one another.
	// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.

	/// <inheritdoc />
	public T this[int index]
	{
		get => InternalSource[index];
		set => InternalSource[index] = value;
	}

	/// <inheritdoc />
	public int IndexOf(T item)
	{
		lock (Sync) return InternalSource.IndexOf(item);
	}

	/// <inheritdoc />
	public void Insert(int index, T item)
	{
		lock (Sync) InternalSource.Insert(index, item);
	}

	/// <inheritdoc />
	public void RemoveAt(int index)
	{
		lock (Sync) InternalSource.RemoveAt(index);
	}
}

[ExcludeFromCodeCoverage]
public class LockSynchronizedListWrapper<T>(
	IList<T> list, bool owner = false)
	: LockSynchronizedListWrapper<T, IList<T>>(list, owner)
{
}