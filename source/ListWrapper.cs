using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections;
public class ListWrapper<T> : CollectionWrapper<T, IList<T>>, IList<T>
{
    public ListWrapper(int capacity = 0)
        : base(new List<T>(capacity), true)
    {
    }

    public ListWrapper(IList<T> source, bool owner = false)
        : base(source, owner)
    {
    }

    /// <inheritdoc />
    public virtual T this[int index]
    {
        get => InternalSource[index];
        set => InternalSource[index] = value;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual int IndexOf(T item)
        => InternalSource.IndexOf(item);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Insert(int index, T item)
        => InternalSource.Insert(index, item);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void RemoveAt(int index)
        => InternalSource.RemoveAt(index);
}
