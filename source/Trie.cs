using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public class Trie<TKey, TValue> : ITrie<TKey, TValue>
{
    // NOTE: Path suffixed methods are provided to avoid ambiguity.

    private Node _root = new();
    private readonly HashSet<int> _lookup = new();

    /// <inheritdoc />
    public void Add(ReadOnlySpan<TKey> key, TValue value)
    {
        int length = key.Length;
        var node = _root;

        for (int i = 0; i < length; i++)
            node = node.GetOrAddChild(key[i]);

        if (node.TrySetValue(value))
        {
            _lookup.Add(length);
            return;
        }

        throw new ArgumentException("An element with the same key already exists.");
    }

    /// <inheritdoc />
    public void AddPath(IEnumerable<TKey> key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        int length = 0;
        var node = _root;

        foreach (var k in key)
        {
            node = node.GetOrAddChild(k);
            length++;
        }

        if (node.TrySetValue(value))
        {
            _lookup.Add(length);
            return;
        }

        throw new ArgumentException("An element with the same key already exists.");
    }

    /// <inheritdoc />
    public bool TryGetValue(ReadOnlySpan<TKey> key, out TValue value)
    {
        int length = key.Length;
        if (!_lookup.Contains(length))
            goto NotFound;

        var node = _root;

        for (int i = 0; i < length; i++)
        {
            if (!node.TryGetChild(key[i], out node))
                goto NotFound;
        }

        if (node.TryGetValue(out value))
            return true;

NotFound:
        value = default!;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValueFromPath(IEnumerable<TKey> key, out TValue value)
    {
        if (key is null)
            goto NotFound;

        var node = _root;
        foreach (var k in key)
        {
            if (!node.TryGetChild(k, out node))
                goto NotFound;
        }

        if (node.TryGetValue(out value))
            return true;

NotFound:
        value = default!;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetValueFromPath(ICollection<TKey> key, out TValue value)
    {
        if (key is not null && !_lookup.Contains(key.Count))
        {
            value = default!;
            return false;
        }

        return TryGetValueFromPath((IEnumerable<TKey>)key!, out value);
    }

    /// <inheritdoc />
    public bool ContainsKey(ReadOnlySpan<TKey> key)
    {
        int length = key.Length;
        if (!_lookup.Contains(length))
            return false;

        var node = _root;

        for (int i = 0; i < length; i++)
        {
            if (!node.TryGetChild(key[i], out node))
                return false;
        }

        return node.IsSet;
    }

    /// <inheritdoc />
    public bool ContainsKeyFromPath(IEnumerable<TKey> key)
    {
        if (key is null) return false;

        var node = _root;
        foreach (var k in key)
        {
            if (!node.TryGetChild(k, out node))
                return false;
        }

        return node.IsSet;
    }

    /// <inheritdoc />
    public bool ContainsKeyFromPath(ICollection<TKey> key)
        => _lookup.Contains(key.Count)
        && ContainsKeyFromPath((IEnumerable<TKey>)key);

    /// <inheritdoc />
    public TValue GetOrAdd(ReadOnlySpan<TKey> key, TValue value)
    {
        if (TryGetValue(key, out var v)) return v;
        Add(key, value);
        return value;
    }

    /// <inheritdoc />
    public TValue GetOrAdd(ReadOnlySpan<TKey> key, Func<TValue> factory)
    {
        if (TryGetValue(key, out var v)) return v;
        var value = factory();
        Add(key, value);
        return value;
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(IEnumerable<TKey> key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (TryGetValueFromPath(key, out var v)) return v;
        AddPath(key, value);
        return value;
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(IEnumerable<TKey> key, Func<TValue> factory)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (TryGetValueFromPath(key, out var v)) return v;
        var value = factory();
        AddPath(key, value);
        return value;
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (TryGetValueFromPath(key, out var v)) return v;
        AddPath(key, value);
        return value;
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, Func<TValue> factory)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (TryGetValueFromPath(key, out var v)) return v;
        var value = factory();
        AddPath(key, value);
        return value;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _lookup.Clear();
        _root = new();
    }

    private sealed class Node
    {
        private Dictionary<TKey, Node>? _children;

        private TValue? _value;

        public bool IsSet { get; private set; }

        public bool TryGetValue(out TValue value)
        {
            bool isSet = IsSet;
            value = isSet ? _value! : default!;
            return isSet;
        }

        public bool TrySetValue(in TValue value)
        {
            if (IsSet) return false;
            _value = value;
            return IsSet = true;
        }

        public Node GetOrAddChild(TKey key)
        {
            var children = _children;
            if (children is not null && children.TryGetValue(key, out var child))
                return child;

            children = _children ??= new();
            child = new Node();
            children[key] = child;

            return child;
        }

        public bool TryGetChild(TKey key, out Node child)
        {
            var children = _children;
            if (children is null)
            {
                child = default!;
                return false;
            }

            return children.TryGetValue(key, out child);
        }
    }
}

/// <summary>
/// Extensions for special cases for a Trie.
/// </summary>
public static class TrieExtensions
{
    /// <inheritdoc cref="Trie{TKey, TValue}.Add(ReadOnlySpan{TKey}, TValue)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(this Trie<char, T> target, StringSegment key, T value)
        => target.Add(key.AsSpan(), value);

    /// <inheritdoc cref="Trie{TKey, TValue}.TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetValue<T>(this Trie<char, T> target, StringSegment key, out T value)
        => target.TryGetValue(key.AsSpan(), out value);

    /// <inheritdoc cref="Trie{TKey, TValue}.ContainsKey(ReadOnlySpan{TKey})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey<T>(this Trie<char, T> target, StringSegment key)
        => target.ContainsKey(key.AsSpan());
}