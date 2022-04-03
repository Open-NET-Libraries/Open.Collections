using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class LockSynchronizedDictionaryWrapper<TKey, TValue>
    : LockSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
	public LockSynchronizedDictionaryWrapper(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

    public LockSynchronizedDictionaryWrapper() : this(new Dictionary<TKey, TValue>()) { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
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
        lock (Sync) InternalSource.Add(key, value);
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
        lock (Sync) return InternalSource.Remove(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}
