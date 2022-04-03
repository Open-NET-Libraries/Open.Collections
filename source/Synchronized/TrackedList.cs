using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class TrackedList<T> : TrackedCollectionWrapper<T, IList<T>>, IList<T>
{
    [ExcludeFromCodeCoverage]
    public TrackedList(ModificationSynchronizer? sync = null) : base(new List<T>(), sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedList(out ModificationSynchronizer sync) : base(new List<T>(), out sync)
    {
    }

    /// <inheritdoc />
    public T this[int index]
    {
        get => Sync!.Reading(() =>
        {
            AssertIsAlive();
            return InternalSource[index];
        });

        set => SetValue(index, value);
    }

    public bool SetValue(int index, T value)
        => Sync!.Modifying(
            () => AssertIsAlive(),
            () => SetValueInternal(index, value));

    private bool SetValueInternal(int index, T value)
    {
        bool changing
            = index >= InternalSource.Count
            || !(InternalSource[index]?.Equals(value) ?? value is null);
        if (changing)
            InternalSource[index] = value;
        return changing;
    }

    /// <inheritdoc />
    public int IndexOf(T item)
        => Sync!.Reading(
            () => AssertIsAlive()
            ? InternalSource.IndexOf(item)
            : -1);

    /// <inheritdoc />
    public void Insert(int index, T item)
        => Sync!.Modifying(
            () => AssertIsAlive(),
            () =>
            {
                InternalSource.Insert(index, item);
                return true;
            });

    /// <inheritdoc />
    public override bool Remove(T item)
    {
        int i = -1;
        return Sync!.Modifying(
            () => AssertIsAlive()
                && (i = InternalSource.IndexOf(item)) != -1,
            () =>
            {
                InternalSource.RemoveAt(i);
                return true;
            });
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
        => Sync!.Modifying(
            () => AssertIsAlive(),
            () =>
            {
                InternalSource.RemoveAt(index);
                return true;
            });

    /// <summary>
    /// Synchonizes finding an item (<paramref name="target"/>), and if found, replaces it with the <paramref name="replacement"/>.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="throwIfNotFound"/> is true and the <paramref name="target"/> is not found.</exception>
    public bool Replace(T target, T replacement, bool throwIfNotFound = false)
    {
        AssertIsAlive();
        int index = -1;
        return !(target?.Equals(replacement) ?? replacement is null)
            && Sync!.Modifying(
            () =>
            {
                AssertIsAlive();
                index = InternalSource.IndexOf(target);
                return index != -1 || (throwIfNotFound ? throw new ArgumentException("Not found.", nameof(target)) : false);
            },
            () =>
                SetValueInternal(index, replacement)
        );
    }
}
