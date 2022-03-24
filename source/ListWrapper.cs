using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections;
public abstract class ListWrapper<T> : CollectionWrapper<T, IList<T>>, IList<T>
{
    protected ListWrapper(IList<T> source) : base(source)
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
