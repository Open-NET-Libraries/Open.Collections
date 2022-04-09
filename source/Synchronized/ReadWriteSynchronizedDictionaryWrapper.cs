using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, TDictionary>
    : ReadWriteSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>, IDictionary<TKey, TValue>
    where TDictionary : class, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public ReadWriteSynchronizedDictionaryWrapper(TDictionary dictionary, bool owner = false) : base(dictionary, owner) { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual TValue this[TKey key]
    {
        get => InternalSource[key];
        set
        {
            // With a dictionary, setting can be like adding.
            // Collection size might change.  Gotta be careful.
            using var upgradable = RWLock.UpgradableReadLock();
            if(InternalSource.ContainsKey(key))
            {
                InternalSource[key] = value;
                return;
            }
            using var write = RWLock.WriteLock();
            InternalSource[key] = value;
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual ICollection<TKey> Keys
        => InternalSource.Keys;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual ICollection<TValue> Values
        => InternalSource.Values;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(TKey key, TValue value)
    {
        using var write = RWLock.WriteLock();
        InternalSource.Add(key, value);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Remove(TKey key)
    {
        using var write = RWLock.WriteLock();
        return InternalSource.Remove(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);

    /// <inheritdoc />
    public virtual bool IfContainsKey(TKey key, Action<IDictionary<TKey, TValue>> action)
    {
        using var uLock = RWLock.UpgradableReadLock();
        if (!InternalSource.ContainsKey(key)) return false;
        using var wLock = RWLock.WriteLock();
        action(InternalSource);
        return true;
    }

    /// <inheritdoc />
    public virtual bool IfNotContainsKey(TKey key, Action<IDictionary<TKey, TValue>> action)
    {
        using var uLock = RWLock.UpgradableReadLock();
        if (InternalSource.ContainsKey(key)) return false;
        using var wLock = RWLock.WriteLock();
        action(InternalSource);
        return true;
    }
}

[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
    : ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>
{
    public ReadWriteSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary, bool owner = false) : base(dictionary, owner)
    {
    }
}

[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionary<TKey, TValue>
    : ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
{
    public ReadWriteSynchronizedDictionary(int capacity = 0) : base(new Dictionary<TKey, TValue>(capacity)) { }
}