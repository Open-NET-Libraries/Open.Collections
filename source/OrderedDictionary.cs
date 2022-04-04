using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

/// <summary>
/// A minimal implementation of <see cref="IOrderedDictionary{TKey, TValue}"/> that is inherently not thread safe.
/// </summary>
public class OrderedDictionary<TKey, TValue>
    : DictionaryWrapper<TKey, TValue>, IOrderedDictionary<TKey, TValue>
{
    private readonly List<TKey> _keys;
    private readonly List<TValue> _values;
    private readonly Dictionary<TKey, int> _indexes;

    public OrderedDictionary(int capacity = 0)
        : base(capacity)
    {
        _keys = new(capacity);
        _values = new(capacity);
        _indexes = new(capacity);
    }

    /// <inheritdoc />
    public override TValue this[TKey key]
    {
        get => InternalSource[key];
        set => SetValue(key, value);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override ICollection<TKey> Keys => _keys.AsReadOnly();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override ICollection<TValue> Values => _values.AsReadOnly();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override int Count
        => _keys.Count;

    private int AddToLists(TKey key, TValue value)
    {
        int i = _keys.Count;
        _keys.Add(key);
        _values.Add(value);
        _indexes[key] = i;
        Debug.Assert(_keys.Count == i + 1);
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
        return i;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void AddInternal(KeyValuePair<TKey, TValue> item)
    {
        InternalSource.Add(item);
        AddToLists(item.Key, item.Value);
    }

    /// <inheritdoc />
    public new int Add(TKey key, TValue value)
    {
        base.Add(key, value);
        return AddToLists(key, value);
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        => Add(key, value);

    /// <inheritdoc />
    public TKey GetKeyAt(int index) => _keys[index];

    /// <inheritdoc />
    public TValue GetValueAt(int index) => _values[index];

    /// <inheritdoc />
    public void Insert(int index, TKey key, TValue value)
    {
        if (index == _keys.Count)
        {
            Add(key, value);
            return;
        }

        base.Add(key, value);
        _indexes.Clear(); // clear the index cache and let it rebuild organically.
        _keys.Insert(index, key);
        _values.Insert(index, value);
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
    }

    private int GetIndex(TKey key, bool addToIndex = true)
    {
        if (_indexes.TryGetValue(key, out int i))
            return i;

        i = _keys.IndexOf(key);
        if (i == -1)
        {
            Debugger.Break();
            throw new Exception("Collection is out of sync possibly due to unsynchronized access by multiple threads.");
        }

        if (addToIndex) _indexes[key] = i;
        return i;
    }

    private void RemoveIndex(int index, TKey key)
    {
        _keys.RemoveAt(index);
        _values.RemoveAt(index);
        if (index == _keys.Count) _indexes.Remove(key); // Removed from end?
        else _indexes.Clear(); // clear the index cache and let it rebuild organically.
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
    }

    /// <inheritdoc />
    public override bool Remove(TKey key)
    {
        if (!base.Remove(key)) return false;
        int i = GetIndex(key, false);
        RemoveIndex(i, key);
        return true;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        var key = _keys[index];
        InternalSource.Remove(key);
        RemoveIndex(index, key);
    }

    /// <inheritdoc />
    public int SetValue(TKey key, TValue value)
    {
        if (!InternalSource.ContainsKey(key))
            return Add(key, value);

        int i = GetIndex(key);
        InternalSource[key] = value;
        _values[i] = value;
        return i;
    }

    /// <inheritdoc />
    public bool SetValueAt(int index, TValue value)
    {
        var key = _keys[index];
        if (InternalSource.TryGetValue(key, out var previous))
        {
            if (previous is null)
            {
                if (value is null) return false;
            }
            else if (previous.Equals(value))
            {
                return false;
            }
        }
        InternalSource[key] = value;
        _values[index] = value;
        return true;
    }
}
