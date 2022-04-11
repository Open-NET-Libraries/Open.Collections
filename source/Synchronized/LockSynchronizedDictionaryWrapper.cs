using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionaryWrapper<TKey, TValue, TDictionary>
    : LockSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>, IDictionary<TKey, TValue>
    where TDictionary : class, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public LockSynchronizedDictionaryWrapper(TDictionary dictionary) : base(dictionary) { }

    /// <inheritdoc />
    public virtual TValue this[TKey key]
    {
        get => InternalSource[key];
        set
        {
            // With a dictionary, setting can be like adding.
            // Collection size might change.  Gotta be careful.
            lock (Sync) InternalSource[key] = value;
        }
    }

    /// <inheritdoc />
    public virtual ICollection<TKey> Keys
        => InternalSource.Keys;

    /// <inheritdoc />
    public virtual ICollection<TValue> Values
        => InternalSource.Values;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(TKey key, TValue value)
    {
        lock (Sync) InternalSource.Add(key, value);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Remove(TKey key)
    {
        lock (Sync) return InternalSource.Remove(key);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}

[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionaryWrapper<TKey, TValue>
    : LockSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>
{
    public LockSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary) : base(dictionary)
    {
    }
}

[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionary<TKey, TValue>
    : LockSynchronizedDictionaryWrapper<TKey, TValue>
{
    public LockSynchronizedDictionary(int capacity) : base(new Dictionary<TKey, TValue>(capacity)) { }
    public LockSynchronizedDictionary() : base(new Dictionary<TKey, TValue>()) { }
}