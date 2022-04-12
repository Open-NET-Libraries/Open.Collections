using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;
public class ListWrapper<T, TList>
    : CollectionWrapper<T, TList>, IList<T>
    where TList : class, IList<T>
{
    [ExcludeFromCodeCoverage]
    public ListWrapper(TList source, bool owner = false)
        : base(source, owner)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
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

[ExcludeFromCodeCoverage]
public class ListWrapper<T>
    : ListWrapper<T, IList<T>>
{
    public ListWrapper(IList<T> source, bool owner = false)
        : base(source, owner)
    {
    }

    public ListWrapper(int capacity = 0)
        : base(new List<T>(capacity))
    {
    }
}