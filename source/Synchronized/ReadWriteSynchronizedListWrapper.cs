using Open.Threading;
using System.Collections.Generic;

namespace Open.Collections.Synchronized;

public class ReadWriteSynchronizedListWrapper<T> : ReadWriteSynchronizedCollectionWrapper<T, IList<T>>, IList<T>
{
	public ReadWriteSynchronizedListWrapper(IList<T> list, bool owner = false) : base(list, owner) { }

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
	public virtual int IndexOf(T item)
        => RWLock.Read(() => InternalSource.IndexOf(item));

	/// <inheritdoc />
	public virtual void Insert(int index, T item)
        => RWLock.Write(() => InternalSource.Insert(index, item));

	/// <inheritdoc />
	public virtual void RemoveAt(int index)
        => RWLock.Write(() => InternalSource.RemoveAt(index));

    /// <inheritdoc />
    public override bool Remove(T item)
        => RWLock.ReadUpgradeable(() =>
        {
            int i = InternalSource.IndexOf(item);
            if (i == -1) return false;
            RemoveAt(i);
            return true;
        });
}
