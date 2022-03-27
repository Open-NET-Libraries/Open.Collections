using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Open.Collections;

/// <summary>
/// A minimal implementation of <see cref="IOrderedDictionary{TKey, TValue}"/> that is inherently not thread safe.
/// </summary>
public class SimpleOrderedDictionary<TKey, TValue>
    : DictionaryWrapper<TKey, TValue>, IOrderedDictionary<TKey, TValue>
{
    private readonly List<TKey> _keys;
    private readonly List<TValue> _values;

    public SimpleOrderedDictionary(int capacity = 0)
        : base(capacity)
    {
        _keys = new(capacity);
        _values = new(capacity);
    }

    /// <inheritdoc />
    public override TValue this[TKey key]
    {
        get => InternalSource[key];
        set
        {
            int i = GetIndex(key);
            InternalSource[key] = value;
            _values[i] = value;
        }
    }

    /// <inheritdoc />
    public override ICollection<TKey> Keys => _keys.AsReadOnly();

    /// <inheritdoc />
    public override ICollection<TValue> Values => _values.AsReadOnly();

    /// <inheritdoc />
    public new int Add(TKey key, TValue value)
    {
        base.Add(key, value);
        _keys.Add(key);
        _values.Add(value);

        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);

        return _keys.Count;
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Add(key, value);

    /// <inheritdoc />
    public TKey GetKeyAt(int index) => _keys[index];

    /// <inheritdoc />
    public TValue GetValueAt(int index) => _values[index];

    /// <inheritdoc />
    public void Insert(int index, TKey key, TValue value)
    {
        base.Add(key, value);
        _keys.Insert(index, key);
        _values.Insert(index, value);
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
    }

    private int GetIndex(TKey key)
    {
        int i = _keys.IndexOf(key);
        return i == -1 ? throw new Exception("Collection is out of sync possibly due to unsynchronized access by multiple threads.") : i;
    }

    /// <inheritdoc />
    public override bool Remove(TKey key)
    {
        if (!base.Remove(key)) return false;
        int i = GetIndex(key);
        _keys.RemoveAt(i);
        _values.RemoveAt(i);
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
        return true;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        var key = _keys[index];
        InternalSource.Remove(key);
        _keys.RemoveAt(index);
        _values.RemoveAt(index);
        Debug.Assert(_keys.Count == InternalSource.Count);
        Debug.Assert(_keys.Count == _values.Count);
    }

    /// <inheritdoc />
    public int SetValue(TKey key, TValue value)
    {
        if (!InternalSource.TryGetValue(key, out _))
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
