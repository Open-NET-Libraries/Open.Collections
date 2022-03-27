using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
    : ReadWriteSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public ReadWriteSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual TValue this[TKey key]
    {
        get => InternalSource[key];
        set => InternalSource[key] = value;
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
