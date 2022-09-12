using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class DictionaryWrapper<TKey, TValue>
    : DictionaryWrapperBase<TKey, TValue, IDictionary<TKey, TValue>>
{
    /// <inheritdoc />
    public DictionaryWrapper()
        : base(new Dictionary<TKey, TValue>(), true) { }

    /// <inheritdoc />
    public DictionaryWrapper(int capacity)
        : base(new Dictionary<TKey, TValue>(capacity), true) { }

    /// <inheritdoc />
    public DictionaryWrapper(IDictionary<TKey, TValue> source, bool owned = false)
        : base(source, owned)
    {
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override TValue GetValueInternal(TKey key)
        => InternalSource[key];

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetValueInternal(TKey key, TValue value)
        => InternalSource[key] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override ICollection<TKey> GetKeys()
        => new ReadOnlyCollectionAdapter<TKey>(
            ThrowIfDisposed(InternalSource.Keys),
            () => InternalSource.Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override ICollection<TValue> GetValues()
        => new ReadOnlyCollectionAdapter<TValue>(
            ThrowIfDisposed(InternalSource.Values),
            () => InternalSource.Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void AddInternal(TKey key, TValue value)
        => InternalSource.Add(key, value);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool ContainsKey(TKey key)
        => InternalSource.ContainsKey(key);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Remove(TKey key)
        => InternalSource.Remove(key);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool TryGetValue(TKey key, out TValue value)
        => InternalSource.TryGetValue(key, out value);
}
