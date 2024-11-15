using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized wrapper for a list that uses a <see cref="System.Threading.ReaderWriterLockSlim"/> for synchronization.
/// </summary>
public class ReadWriteSynchronizedListWrapper<T, TList>(
	TList list, bool owner = false)
	: ReadWriteSynchronizedCollectionWrapper<T, TList>(list, owner), IList<T>
	where TList : class, IList<T>
{
	// This is a simplified version.
	// It could be possible to allow indexed values to change independently of one another.
	// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public T this[int index]
	{
		get => InternalSource[index];
		set => InternalSource[index] = value;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual int IndexOf(T item)
	{
		using var read = RWLock.ReadLock();
		return InternalUnsafeSource!.IndexOf(item);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual void Insert(int index, T item)
	{
		using var write = RWLock.WriteLock();
		InternalUnsafeSource!.Insert(index, item);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual void RemoveAt(int index)
	{
		using var write = RWLock.WriteLock();
		InternalUnsafeSource!.RemoveAt(index);
	}

	/// <inheritdoc />
	public override bool Remove(T item)
	{
		using var upgradable = RWLock.UpgradableReadLock();
		int i = InternalUnsafeSource!.IndexOf(item);
		if (i == -1) return false;
		RemoveAt(i);
		return true;
	}
}

/// <summary>
/// A synchronized wrapper for a list that uses a <see cref="System.Threading.ReaderWriterLockSlim"/> for synchronization.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedListWrapper<T>(
	IList<T> list, bool owner = false)
	: ReadWriteSynchronizedListWrapper<T, IList<T>>(list, owner)
{
}