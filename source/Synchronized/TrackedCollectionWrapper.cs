using Open.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized;

public class TrackedCollectionWrapper<T, TCollection>
    : ModificationSynchronizedBase, ICollection<T>, IAddMultiple<T>
    where TCollection : class, ICollection<T>
{
    protected TCollection InternalSource;

    [ExcludeFromCodeCoverage]
    public TrackedCollectionWrapper(TCollection collection, ModificationSynchronizer? sync = null)
        : base(sync) => InternalSource = collection ?? throw new ArgumentNullException(nameof(collection));

    [ExcludeFromCodeCoverage]
    public TrackedCollectionWrapper(TCollection collection, out ModificationSynchronizer sync)
        : base(out sync) => InternalSource = collection ?? throw new ArgumentNullException(nameof(collection));

    [ExcludeFromCodeCoverage]
    protected override ModificationSynchronizer InitSync(object? sync = null)
    {
        _syncOwned = true;
        return new ReadWriteModificationSynchronizer(sync as ReaderWriterLockSlim);
    }

    [ExcludeFromCodeCoverage]
    protected override void OnDispose()
    {
        base.OnDispose();
        Nullify(ref InternalSource); // Eliminate risk from wrapper.
    }

    /// <inheritdoc />
    public int Count
        => Sync!.Reading(() =>
        {
            AssertIsAlive();
            return InternalSource.Count;
        });

    [ExcludeFromCodeCoverage]
    protected virtual void AddInternal(T item)
       => InternalSource.Add(item);

    /// <inheritdoc />
    public void Add(T item)
        => Sync!.Modifying(() => AssertIsAlive(), () =>
        {
            AddInternal(item);
            return true;
        });

    /// <inheritdoc cref="IAddMultiple{T}.AddThese(T, T, T[])" />
    public void AddThese(T item1, T item2, params T[] items)
        => Sync!.Modifying(() => AssertIsAlive(), () =>
        {
            AddInternal(item1);
            AddInternal(item2);
            foreach (T? i in items)
                AddInternal(i);
            return true;
        });

    /// <inheritdoc />
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null) return;
        IReadOnlyList<T> enumerable = items switch
        {
            IImmutableList<T> i => i,
            T[] a => a,
            _ => items.ToArray(),

        };

        if (enumerable.Count == 0)
            return;

        Sync!.Modifying(() => AssertIsAlive(), () =>
        {
            foreach (var item in enumerable)
                AddInternal(item);
            return true;
        });
    }

    [ExcludeFromCodeCoverage]
    protected virtual void ClearInternal()
        => InternalSource.Clear();

    /// <inheritdoc />
    public void Clear()
        => Sync!.Modifying(
        () => AssertIsAlive() && InternalSource.Count != 0,
        () =>
        {
            int count = Count;
            bool hasItems = count != 0;
            if (hasItems) ClearInternal();
            return hasItems;
        });

    /// <inheritdoc />
    public bool Contains(T item)
        => Sync!.Reading(() => AssertIsAlive() && InternalSource.Contains(item));

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
        => Sync!.Reading(() =>
        {
            AssertIsAlive();
            InternalSource.CopyTo(array, arrayIndex);
        });

    /// <inheritdoc />
    public virtual bool Remove(T item)
        => Sync!.Modifying(
            () => AssertIsAlive(),
            () => InternalSource.Remove(item));

    [ExcludeFromCodeCoverage]
    protected virtual IEnumerator<T> GetEnumeratorInternal()
        => InternalSource.GetEnumerator();

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
        => Sync!.Reading(() =>
        {
            AssertIsAlive();
            return GetEnumeratorInternal();
        });

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
