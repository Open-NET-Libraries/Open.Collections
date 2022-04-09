using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class TrackedDictionaryWrapper<TKey, TValue, TDictionary>
    : TrackedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>, IDictionary<TKey, TValue>
    where TDictionary : class, IDictionary<TKey, TValue>
{
    [ExcludeFromCodeCoverage]
    public TrackedDictionaryWrapper(TDictionary dictionary, ModificationSynchronizer? sync = null)
    : base(dictionary, sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedDictionaryWrapper(TDictionary dictionary, out ModificationSynchronizer sync)
        : base(dictionary, out sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public ICollection<TKey> Keys => InternalSource.Keys;

    [ExcludeFromCodeCoverage]
    public ICollection<TValue> Values => InternalSource.Values;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool TryGetValue(TKey key, out TValue value)
    {
        TValue v = default!;
        bool result = Sync!.Reading(() => InternalSource.TryGetValue(key, out v));
        value = v;
        return result;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TValue this[TKey key]
    {
        get => InternalSource[key];
        set => SetValue(key, value);
    }

    public bool SetValue(TKey key, TValue value)
        => Sync!.Modifying(
            AssertIsAlive,
            () => SetValueInternal(key, value),
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Modified, KeyValuePair.Create(key, value), version);
            });

    private bool SetValueInternal(TKey key, TValue value)
    {
        bool changing
            = InternalSource.TryGetValue(key, out var current)
            && !(current?.Equals(value) ?? value is null);
        if (changing)
            InternalSource[key] = value;
        return changing;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool ContainsKey(TKey key)
        => Sync!.Reading(
            () => AssertIsAlive() && InternalSource.ContainsKey(key));

    protected virtual int AddSynchronized(TKey key, TValue value)
    {
        Sync!.Modifying(
            AssertIsAlive,
            () =>
            {
                InternalSource.Add(key, value);
                return true;
            },
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Added, KeyValuePair.Create(key, value), version);
            });
        return -1;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(TKey key, TValue value)
        => AddSynchronized(key, value);

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        TValue value = default!;
        return Sync!.Modifying(
            () => AssertIsAlive()
                && InternalSource.TryGetValue(key, out value),
            () =>
            {
                bool result = InternalSource.Remove(key);
                Debug.Assert(result); // Should always be true unless out of sync.
                return result;
            },
            version =>
            {
                if (HasChangedListeners) // Avoid creating KVP unnecessarily.
                    OnChanged(ItemChange.Removed, KeyValuePair.Create(key, value), version);
            });
    }
}

public class TrackedDictionaryWrapper<TKey, TValue>
    : TrackedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>
{
    [ExcludeFromCodeCoverage]
    public TrackedDictionaryWrapper(IDictionary<TKey, TValue> dictionary, ModificationSynchronizer? sync = null)
        : base(dictionary, sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedDictionaryWrapper(IDictionary<TKey, TValue> dictionary, out ModificationSynchronizer sync)
        : base(dictionary, out sync)
    {
    }
}


public class TrackedDictionary<TKey, TValue>
    : TrackedDictionaryWrapper<TKey, TValue>
{
    [ExcludeFromCodeCoverage]
    public TrackedDictionary(int capacity, ModificationSynchronizer? sync = null)
        : base(new Dictionary<TKey, TValue>(capacity), sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedDictionary(int capacity, out ModificationSynchronizer sync)
        : base(new Dictionary<TKey, TValue>(capacity), out sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedDictionary(ModificationSynchronizer? sync = null)
        : base(new Dictionary<TKey, TValue>(), sync)
    {
    }

    [ExcludeFromCodeCoverage]
    public TrackedDictionary(out ModificationSynchronizer sync)
        : base(new Dictionary<TKey, TValue>(), out sync)
    {
    }
}
