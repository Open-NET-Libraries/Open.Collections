using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class DictionaryWrapper<TKey, TValue>
    : CollectionWrapper<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>
{
    /// <inheritdoc />
    public DictionaryWrapper(int capacity = 0)
        : base(new Dictionary<TKey, TValue>(capacity), true)
    {
    }

    public DictionaryWrapper(IDictionary<TKey, TValue> source, bool owned = false)
        : base(source, owned)
    {
    }

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
        => InternalSource.Add(key, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public virtual bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public virtual bool Remove(TKey key)
        => InternalSource.Remove(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}
