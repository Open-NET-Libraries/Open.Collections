using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

[ExcludeFromCodeCoverage]
public abstract class DictionaryWrapperBase<TKey, TValue, TCollection>
    : CollectionWrapper<KeyValuePair<TKey, TValue>, TCollection>, IDictionary<TKey, TValue>
    where TCollection : class, ICollection<KeyValuePair<TKey, TValue>>
{
    protected DictionaryWrapperBase(TCollection source, bool owner = false)
        : base(source, owner)
    {
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => GetValueInternal(key);
        set => SetValueInternal(key, value);
    }

    protected abstract TValue GetValueInternal(TKey key);

    protected abstract void SetValueInternal(TKey key, TValue value);

    ICollection<TKey>? _keys;
    /// <inheritdoc />
    public ICollection<TKey> Keys => _keys ??= GetKeys();
    protected abstract ICollection<TKey> GetKeys();

    ICollection<TValue>? _values;

    /// <inheritdoc />
    public ICollection<TValue> Values => _values ??= GetValues();

    protected abstract ICollection<TValue> GetValues();

    protected abstract void AddInternal(TKey key, TValue value);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value)
        => AddInternal(key, value);

    /// <inheritdoc />
    public abstract bool ContainsKey(TKey key);

    /// <inheritdoc />
    public abstract bool Remove(TKey key);

    /// <inheritdoc />
    public abstract bool TryGetValue(TKey key, out TValue value);
}
