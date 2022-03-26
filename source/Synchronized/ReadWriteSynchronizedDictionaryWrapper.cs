using Open.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
    : ReadWriteSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public ReadWriteSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

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
        using var write = Sync.WriteLock();
        InternalSource.Add(key, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public virtual bool Remove(TKey key)
    {
        using var write = Sync.WriteLock();
        return InternalSource.Remove(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}
