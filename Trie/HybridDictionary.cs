using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Trie;
internal class HybridDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly struct Entry
    {
        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }
        public TValue Value { get; }
    }

    private const int SEARCH_THRESHOLD = 12;

    private Entry[]? _list;
    private readonly Dictionary<TKey, TValue> _dictionary;

    public ICollection<TKey> Keys => _dictionary.Keys;

    public ICollection<TValue> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;

    public HybridDictionary()
    {
        _list = new Entry[SEARCH_THRESHOLD];
        _dictionary = new();
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        int count = _dictionary.Count;
        var list = _list;
        if (list is null) return;
        if (count == SEARCH_THRESHOLD)
            _list = null;
        else
            list[count - 1] = new(key, value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        var list = _list;
        if(list is not null)
        {
            foreach(var e in list)
            {
                if(key.Equals(e.Key))
                {
                    value = e.Value;
                    return true;
                }
            }

            value = default!;
            return false;
        }

        return _dictionary.TryGetValue(key, out value);
    }

    public bool Remove(TKey key) => throw new NotSupportedException();

    public void Clear()
    {
        _dictionary.Clear();
        _list = new Entry[SEARCH_THRESHOLD];
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
}