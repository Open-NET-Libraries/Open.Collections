using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class ReadWriteSynchronizedListWrapper<T> : ReadWriteSynchronizedCollectionWrapper<T, IList<T>>, IList<T>
{
    [ExcludeFromCodeCoverage]
    public ReadWriteSynchronizedListWrapper(IList<T> list, bool owner = false) : base(list, owner) { }

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
        return InternalSource.IndexOf(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual void Insert(int index, T item)
    {
        using var write = RWLock.WriteLock();
        InternalSource.Insert(index, item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual void RemoveAt(int index)
    {
        using var write = RWLock.WriteLock();
        InternalSource.RemoveAt(index);
    }

    /// <inheritdoc />
    public override bool Remove(T item)
    {
        using var upgradable = RWLock.UpgradableReadLock();
        int i = InternalSource.IndexOf(item);
        if (i == -1) return false;
        RemoveAt(i);
        return true;
    }
}
