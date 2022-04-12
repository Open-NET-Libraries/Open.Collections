using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Open.Collections;

/// <summary>
/// A minimal implementation of <see cref="IIndexedDictionary{TKey, TValue}"/> that is inherently not thread safe.
/// </summary>
public class IndexedDictionary<TKey, TValue>
    : DictionaryWrapper<TKey, TValue>, IIndexedDictionary<TKey, TValue>
{
    private const string OUTOFSYNC = "Collection is out of sync possibly due to unsynchronized access by multiple threads.";
    private readonly List<KeyValuePair<TKey, TValue>> _entries;
    private readonly Dictionary<TKey, int> _indexes;

    [ExcludeFromCodeCoverage]
    public IndexedDictionary(int capacity)
        : base(capacity)
    {
        _entries = new(capacity);
        _indexes = new(capacity);
    }

    public IndexedDictionary()
    : base()
    {
        _entries = new();
        _indexes = new();
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        _entries.Clear();
        _indexes.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetValueInternal(TKey key, TValue value) => SetValue(key, value);

    /// <inheritdoc />
    public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => _entries.GetEnumerator().Preflight(ThrowIfDisposed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override ICollection<TKey> GetKeys()
        => new ReadOnlyCollectionAdapter<TKey>(
            ThrowIfDisposed(_entries.Select(e => e.Key)),
            () => _entries.Count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override ICollection<TValue> GetValues()
        => new ReadOnlyCollectionAdapter<TValue>(
            ThrowIfDisposed(_entries.Select(e => e.Value)),
            () => _entries.Count);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override int Count
    {
        get
        {
            AssertIsAlive();
            return _entries.Count;
        }
    }

#if NETSTANDARD2_0
    [SuppressMessage("Roslynator", "RCS1242:Do not pass non-read-only struct by read-only reference.", Justification = "KeyValuePairs are not truly readonly until NET Standard 2.1.")]
#endif
    private int AddToLists(in KeyValuePair<TKey, TValue> kvp)
    {
        int i = _entries.Count;
        _entries.Add(kvp);
        _indexes[kvp.Key] = i;
        Debug.Assert(_entries.Count == i + 1);
        Debug.Assert(_entries.Count == InternalSource.Count);
        return i;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void AddInternal(in KeyValuePair<TKey, TValue> item)
    {
        InternalSource.Add(item);
        AddToLists(in item);
    }

    /// <inheritdoc />
    public new int Add(TKey key, TValue value)
    {
        var kvp = KeyValuePair.Create(key, value);
        InternalSource.Add(kvp);
        return AddToLists(in kvp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void AddInternal(TKey key, TValue value)
        => Add(key, value);

    /// <inheritdoc />
    public TKey GetKeyAt(int index) => _entries[index].Key;

    /// <inheritdoc />
    public TValue GetValueAt(int index) => _entries[index].Value;

    /// <inheritdoc />
    public void Insert(int index, TKey key, TValue value)
    {
        if (index == _entries.Count)
        {
            Add(key, value);
            return;
        }

        AssertIsAlive();
        var kvp = KeyValuePair.Create(key, value);
        base.AddInternal(in kvp);
        _indexes.Clear(); // clear the index cache and let it rebuild organically.
        _entries.Insert(index, kvp);
        Debug.Assert(_entries.Count == InternalSource.Count);
    }

    private int GetIndex(TKey key, bool addToIndex = true)
    {
        if (_indexes.TryGetValue(key, out int index))
            return index;

        // Partial index rebuild.
        int i = 0;
        foreach(var kvp in _entries)
        {
            var k = kvp.Key;
            if(key!.Equals(k))
            {
                if (addToIndex) _indexes[k] = i;
                return i;
            }

            _indexes[k] = i;
            i++;
        }

        Debugger.Break();
        throw new Exception(OUTOFSYNC);
    }

    private void RemoveIndex(int index, TKey key)
    {
        _entries.RemoveAt(index);
        if (index == _entries.Count) _indexes.Remove(key); // Removed from end?
        else _indexes.Clear(); // clear the index cache and let it rebuild organically.
        Debug.Assert(_entries.Count == InternalSource.Count);
    }

    public override void Clear()
    {
        base.Clear();
        _entries.Clear();
        _indexes.Clear();
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
        var key = _entries[index].Key;
        InternalSource.Remove(key);
        RemoveIndex(index, key);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SetValue(TKey key, TValue value)
    {
        SetValue(key, value, out int index);
        return index;
    }

    /// <inheritdoc />
    public bool SetValue(TKey key, TValue value, out int index)
    {
        if (!InternalSource.TryGetValue(key, out var prevValue))
        {
            index = Add(key, value);
            return true;
        }

        index = GetIndex(key);
        if (prevValue?.Equals(value) ?? value is null)
            return false;

        InternalSource[key] = value;
        _entries[index] = KeyValuePair.Create(key, value);
        return true;
    }

    /// <inheritdoc />
    public bool SetValueAt(int index, TValue value, out TKey key)
    {
        key = _entries[index].Key;

        if (!InternalSource.TryGetValue(key, out var previous))
            throw new Exception(OUTOFSYNC);

        if (previous?.Equals(value) ?? value is null)
           return false;

        InternalSource[key] = value;
        _entries[index] = KeyValuePair.Create(key, value);
        return true;
    }
}
