using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class LockSynchronizedListWrapper<T> : LockSynchronizedCollectionWrapper<T, IList<T>>, IList<T>
{
	public LockSynchronizedListWrapper(IList<T> list) : base(list) { }

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
    public int IndexOf(T item)
	{
		lock (Sync) return InternalSource.IndexOf(item);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Insert(int index, T item)
	{
		lock (Sync) InternalSource.Insert(index, item);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void RemoveAt(int index)
	{
		lock (Sync) InternalSource.RemoveAt(index);
	}
}
