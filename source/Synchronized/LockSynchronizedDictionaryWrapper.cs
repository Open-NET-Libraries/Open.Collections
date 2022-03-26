using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class LockSynchronizedDictionaryWrapper<TKey, TValue>
    : LockSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public LockSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

    /// <inheritdoc />
    public virtual TValue this[TKey key]
    {
        get => InternalSource[key];
        set => InternalSource[key] = value;
    }

    /// <inheritdoc />
    public virtual ICollection<TKey> Keys
        => InternalSource.Keys;

    /// <inheritdoc />
    public virtual ICollection<TValue> Values
        => InternalSource.Values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public virtual void Add(TKey key, TValue value)
    {
        lock(Sync) InternalSource.Add(key, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public virtual bool Remove(TKey key)
    {
        lock (Sync) return InternalSource.Remove(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}
