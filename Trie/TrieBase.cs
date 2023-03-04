using System;
using System.Collections.Generic;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public abstract class TrieBase<TKey, TValue> : ITrie<TKey, TValue>
{
    /// <summary>
    /// Initializes this.
    /// </summary>
    internal TrieBase(Func<NodeBase> rootFactory, ISet<int> lookup)
    {
        _root = rootFactory();
        _rootFactory = rootFactory;
        _lookup = lookup;
    }

    // NOTE: Path suffixed methods are provided to avoid ambiguity.

    private NodeBase _root;
    private readonly Func<TrieBase<TKey, TValue>.NodeBase> _rootFactory;
    private readonly ISet<int> _lookup;

    internal NodeBase EnsureNode(ReadOnlySpan<TKey> key)
    {
        int length = key.Length;
        var node = _root;

        for (int i = 0; i < length; i++)
            node = node.GetOrAddChild(key[i]);

        return node;
    }

    internal NodeBase EnsureNode(IEnumerable<TKey> key, out int length)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        length = 0;
        var node = _root;

        foreach (var k in key)
        {
            node = node.GetOrAddChild(k);
            length++;
        }

        return node;
    }

    TrieBase<TKey, TValue>.NodeBase ITrie<TKey, TValue>.EnsureNodes(ReadOnlySpan<TKey> key)
        => EnsureNode(key);


    /// <inheritdoc />
    public bool Add(ReadOnlySpan<TKey> key, in TValue value)
    {
        if (!EnsureNode(key).TrySetValue(in value))
            return false;

        _lookup.Add(key.Length);
        return true;
    }

    /// <inheritdoc />
    public bool AddPath(IEnumerable<TKey> key, in TValue value)
    {
        if (!EnsureNode(key, out int length).TrySetValue(in value))
            return false;

        _lookup.Add(length);
        return true;
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
        if (key is null || !_lookup.Contains(key.Count))
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
        => key is not null && _lookup.Contains(key.Count)
        && ContainsKeyFromPath((IEnumerable<TKey>)key);

    /// <inheritdoc />
    public TValue GetOrAdd(ReadOnlySpan<TKey> key, in TValue value)
        => EnsureNode(key).GetOrAdd(in value);

    /// <inheritdoc />
    public TValue GetOrAdd(ReadOnlySpan<TKey> key, Func<TValue> factory)
    {
        var node = EnsureNode(key);
        if (node.TryGetValue(out var v))
            return v;

        var value = factory();
        return node.GetOrAdd(in value);
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(IEnumerable<TKey> key, in TValue value)
        => EnsureNode(key, out _).GetOrAdd(in value);

    /// <inheritdoc />
    public TValue GetOrAddFromPath(IEnumerable<TKey> key, Func<TValue> factory)
    {
        var node = EnsureNode(key, out _);
        if (node.TryGetValue(out var v))
            return v;

        var value = factory();
        return node.GetOrAdd(in value);
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, in TValue value)
        => EnsureNode(key, out _).GetOrAdd(in value);

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, Func<TValue> factory)
    {
        var node = EnsureNode(key, out _);
        if (node.TryGetValue(out var v))
            return v;

        var value = factory();
        return node.GetOrAdd(in value);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _lookup.Clear();
        _root = _rootFactory();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    internal abstract class NodeBase
    {
        protected abstract IDictionary<TKey, NodeBase>? Children { get; }

        private (bool isSet, TValue? value) _value;

        public bool IsSet => _value.isSet;

        internal TValue GetValueOrThrow()
            => TryGetValue(out var value)
                ? value
                : throw new Exception("Unexpected concurrency condition. Value was set, but then not available.");

        public bool TryGetValue(out TValue value)
        {
            var (isSet, v) = _value;
            value = isSet ? v! : default!;
            return isSet;
        }

        public TValue GetOrAdd(in TValue value)
            => TryGetValue(out var v)
                ? v
                : TrySetValue(in value)
                ? value
                : GetValueOrThrow();

        // Optimistic concurrency wins here. No locking.
        public bool TrySetValue(in TValue value)
        {
            var (isSet, _) = _value;
            if (isSet) return false;
            _value = (true, value);
            return true;
        }

        public abstract NodeBase GetOrAddChild(TKey key);

        public bool TryGetChild(TKey key, out NodeBase child)
        {
            var children = Children;
            if (children is null)
            {
                child = default!;
                return false;
            }

            return children.TryGetValue(key, out child);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
