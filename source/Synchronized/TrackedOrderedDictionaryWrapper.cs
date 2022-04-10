using Open.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class TrackedOrderedDictionaryWrapper<TKey, TValue, TDictionary>
    : TrackedDictionaryWrapper<TKey, TValue, TDictionary>, IOrderedDictionary<TKey, TValue>
    where TDictionary : class, IOrderedDictionary<TKey, TValue>
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionaryWrapper(TDictionary dictionary, ModificationSynchronizer? sync = null)
    : base(dictionary, sync)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionaryWrapper(TDictionary dictionary, out ModificationSynchronizer sync)
        : base(dictionary, out sync)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual TKey GetKeyAt(int index)
        => Sync!.Reading(() => InternalSource.GetKeyAt(index));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual TValue GetValueAt(int index)
        => Sync!.Reading(() => InternalSource.GetValueAt(index));

    /// <inheritdoc />
    public void Insert(int index, TKey key, TValue value)
        => Sync!.Modifying(
            AssertIsAlive,
            () =>
            {
                InternalSource.Insert(index, key, value);
                return true;
            },
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Inserted, index, KeyValuePair.Create(key, value), version);
            });

    /// <inheritdoc cref="IList{T}.RemoveAt(int)" />
    public void RemoveAt(int index)
    {
        TKey key = default!;
        TValue value = default!;
        Sync!.Modifying(
            AssertIsAlive,
            () =>
            {
                key = InternalSource.GetKeyAt(index);
                value = InternalSource[key];
                InternalSource.RemoveAt(index);
                return true;
            },
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Removed, index, KeyValuePair.Create(key, value), version);
            });
    }

    /// <inheritdoc />
    public new int SetValue(TKey key, TValue value)
    {
        SetValue(key, value, out int index);
        return index;
    }

    /// <inheritdoc />
    public bool SetValue(TKey key, TValue value, out int index)
    {
        int i = -1;
        bool result = Sync!.Modifying(
            AssertIsAlive,
            () => InternalSource.SetValue(key, value, out i),
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Modified, i, KeyValuePair.Create(key, value), version);
            });
        index = i;
        return result;
    }

    /// <inheritdoc />
    public bool SetValueAt(int index, TValue value, out TKey key)
    {
        TKey k = default!;
        bool result = Sync!.Modifying(
            AssertIsAlive,
            () => InternalSource.SetValueAt(index, value, out k),
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Modified, index, KeyValuePair.Create(k, value), version);
            });
        key = k;
        return result;
    }

    protected override int AddSynchronized(TKey key, TValue value)
    {
        int index = -1;
        Sync!.Modifying(
            AssertIsAlive,
            () =>
            {
                index = InternalSource.Add(key, value);
                return true;
            },
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Added, index, KeyValuePair.Create(key, value), version);
            });
        return index;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public new int Add(TKey key, TValue value)
        => AddSynchronized(key, value);
}

public class TrackedOrderedDictionaryWrapper<TKey, TValue>
    : TrackedOrderedDictionaryWrapper<TKey, TValue, IOrderedDictionary<TKey, TValue>>
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionaryWrapper(IOrderedDictionary<TKey, TValue> dictionary, ModificationSynchronizer? sync = null)
        : base(dictionary, sync)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionaryWrapper(IOrderedDictionary<TKey, TValue> dictionary, out ModificationSynchronizer sync)
        : base(dictionary, out sync)
    {
    }
}

public sealed class TrackedOrderedDictionary<TKey, TValue>
    : TrackedOrderedDictionaryWrapper<TKey, TValue>
{
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionary(int capacity, ModificationSynchronizer? sync = null)
        : base(new OrderedDictionary<TKey, TValue>(capacity), sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionary(int capacity, out ModificationSynchronizer sync)
        : base(new OrderedDictionary<TKey, TValue>(capacity), out sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionary(ModificationSynchronizer? sync = null)
        : base(new OrderedDictionary<TKey, TValue>(), sync)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TrackedOrderedDictionary(out ModificationSynchronizer sync)
        : base(new OrderedDictionary<TKey, TValue>(), out sync)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override TKey GetKeyAt(int index) => InternalSource.GetKeyAt(index);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override TValue GetValueAt(int index) => InternalSource.GetValueAt(index);
}
