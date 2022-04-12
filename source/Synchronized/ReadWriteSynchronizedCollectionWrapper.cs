using Open.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized;

public class ReadWriteSynchronizedCollectionWrapper<T, TCollection>
    : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
    where TCollection : class, ICollection<T>
{
    protected ReaderWriterLockSlim RWLock = new(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.

    protected ReadWriteSynchronizedCollectionWrapper(TCollection source, bool owner = false)
        : base(source, owner)
    {
    }

    #region Implementation of ICollection<T>

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override void Add(T item)

    {
        using var write = RWLock.WriteLock();
        AddInternal(item);
    }

    /// <inheritdoc />
    public override void AddThese(T item1, T item2, params T[] items)
    {
        using var write = RWLock.WriteLock();
        AddInternal(item1);
        AddInternal(item2);
        foreach (var i in items)
            AddInternal(i);
    }

    /// <inheritdoc />
    public override void AddRange(IEnumerable<T> items)
    {
        if (items is null) return;
        IReadOnlyList<T> enumerable = items switch
        {
            IImmutableList<T> i => i,
            T[] a => a,
            _ => items.ToArray(),
        };

        using var write = RWLock.WriteLock();
        base.AddRange(enumerable);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        using var write = RWLock.WriteLock();
        base.Clear();
    }

    /// <inheritdoc />
    public override bool Contains(T item)
    {
        using var read = RWLock.ReadLock();
        return base.Contains(item);
    }

    /// <inheritdoc />
    public override bool Remove(T item)
    {
        using var write = RWLock.WriteLock();
        return base.Remove(item);
    }

    /// <inheritdoc />
    public override int Count
    {
        get
        {
            using var read = RWLock.ReadLock();
            return base.Count;
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public T[] Snapshot()
    {
        using var read = RWLock.ReadLock();
        return InternalUnsafeSource!.ToArray();
    }

    /// <inheritdoc />
    public override void Export(ICollection<T> to)
    {
        using var read = RWLock.ReadLock();
        to.AddRange(InternalUnsafeSource!);
    }

    /// <inheritdoc />
    public override void CopyTo(T[] array, int arrayIndex)
    {
        using var read = RWLock.ReadLock();
        base.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public override Span<T> CopyTo(Span<T> span)
    {
        using var read = RWLock.ReadLock();
        return base.CopyTo(span);
    }

    #endregion

    #region Dispose
    protected override void OnBeforeDispose() =>
        // Give everything else a chance to finish up.
        RWLock.TryWrite(1000, () => { });

    protected override void OnDispose()
    {
        RWLock.Dispose();
        base.OnDispose();
    }
    #endregion

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Read(Action action)
    {
        using var read = RWLock.ReadLock();
        action();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TResult Read<TResult>(Func<TResult> action)
    {
        using var read = RWLock.ReadLock();
        return action();
    }

    /// <inheritdoc />
    public void Modify(Func<bool> condition, Action<TCollection> action)
        => RWLock.ReadWriteConditional(_ => condition(), () => action(InternalSource));

    /// <summary>
    /// Allows for multiple modifications at once.
    /// </summary>
    /// <param name="condition">Only executes the action if the condition is true.  The condition may be invoked more than once.</param>
    /// <param name="action">The action to execute safely on the underlying collection safely.</param>
    public void Modify(Func<bool, bool> condition, Action<TCollection> action)
        => RWLock.ReadWriteConditional(condition, () => action(InternalSource));

    /// <inheritdoc />
    public void Modify(Action<TCollection> action)
    {
        using var write = RWLock.WriteLock();
        action(InternalSource);
    }

    /// <inheritdoc />
    public TResult Modify<TResult>(Func<TCollection, TResult> action)
    {
        using var write = RWLock.WriteLock();
        return action(InternalSource);
    }

    /// <inheritdoc />
    public virtual bool IfContains(T item, Action<TCollection> action)
    {
        using var uLock = RWLock.UpgradableReadLock();
        if (!InternalSource.Contains(item)) return false;
        using var wLock = RWLock.WriteLock();
        action(InternalSource);
        return true;
    }

    /// <inheritdoc />
    public virtual bool IfNotContains(T item, Action<TCollection> action)
    {
        using var uLock = RWLock.UpgradableReadLock();
        if (InternalSource.Contains(item)) return false;
        using var wLock = RWLock.WriteLock();
        action(InternalSource);
        return true;
    }
}
