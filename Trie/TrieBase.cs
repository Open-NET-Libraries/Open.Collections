using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public abstract class TrieBase<TKey, TValue>
    : ITrie<TKey, TValue>, ITrieNode<TKey, TValue>
{
    /// <summary>
    /// Initializes this.
    /// </summary>
    internal TrieBase(Func<ITrieNode<TKey, TValue>> rootFactory)
    {
        _root = rootFactory();
        _rootFactory = rootFactory;
    }

    // NOTE: Path suffixed methods are provided to avoid ambiguity.

    private ITrieNode<TKey, TValue> _root;
    private readonly Func<ITrieNode<TKey, TValue>> _rootFactory;

    internal ITrieNode<TKey, TValue> EnsureNode(ReadOnlySpan<TKey> key)
    {
        int length = key.Length;
        var node = _root;

        for (int i = 0; i < length; i++)
            node = node.GetOrAddChild(key[i]);

        return node;
    }

    internal ITrieNode<TKey, TValue> EnsureNode(IEnumerable<TKey> key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        var node = _root;
        foreach (var k in key)
            node = node.GetOrAddChild(k);

        return node;
    }

    ITrieNode<TKey, TValue> ITrie<TKey, TValue>.EnsureNodes(ReadOnlySpan<TKey> key)
        => EnsureNode(key);

    /// <inheritdoc />
    public bool Add(ReadOnlySpan<TKey> key, in TValue value)
        => EnsureNode(key).TrySetValue(in value);

    /// <inheritdoc />
    public bool AddPath(IEnumerable<TKey> key, in TValue value)
        => EnsureNode(key).TrySetValue(in value);

    /// <inheritdoc />
    public bool TryGetValue(ReadOnlySpan<TKey> key, out TValue value)
    {
        int length = key.Length;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValueFromPath(ICollection<TKey> key, out TValue value)
        => TryGetValueFromPath((IEnumerable<TKey>)key, out value);

    /// <inheritdoc />
    public bool ContainsKey(ReadOnlySpan<TKey> key)
    {
        int length = key.Length;
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
        => key is not null && ContainsKeyFromPath((IEnumerable<TKey>)key);

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
        => EnsureNode(key).GetOrAdd(in value);

    /// <inheritdoc />
    public TValue GetOrAddFromPath(IEnumerable<TKey> key, Func<TValue> factory)
    {
        var node = EnsureNode(key);
        if (node.TryGetValue(out var v))
            return v;

        var value = factory();
        return node.GetOrAdd(in value);
    }

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, in TValue value)
        => EnsureNode(key).GetOrAdd(in value);

    /// <inheritdoc />
    public TValue GetOrAddFromPath(ICollection<TKey> key, Func<TValue> factory)
    {
        var node = EnsureNode(key);
        if (node.TryGetValue(out var v))
            return v;

        var value = factory();
        return node.GetOrAdd(in value);
    }

    /// <inheritdoc />
    public void Clear()
        => _root = _rootFactory();

    /// <inheritdoc />
    public bool IsSet => _root.IsSet;

    /// <inheritdoc />
    public TValue? Value => _root.Value;

    /// <inheritdoc />
    public ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
        => _root.GetOrAddChild(key);

    /// <inheritdoc />
    bool ITrieNode<TKey, TValue>.TryGetChild(TKey key, out ITrieNode<TKey, TValue> child)
        => _root.TryGetChild(key, out child);

    /// <inheritdoc />
    public ITrieNode<TKey, TValue> GetChild(TKey key)
        => _root.GetChild(key);

    /// <inheritdoc />
    public bool TryGetValue(out TValue value)
        => _root.TryGetValue(out value);

    /// <inheritdoc />
    public bool TrySetValue(in TValue value)
        => _root.TrySetValue(value);

    /// <inheritdoc />
    public TValue GetOrAdd(in TValue value)
        => _root.GetOrAdd(value);

    internal abstract class NodeBase : ITrieNode<TKey, TValue>
    {
        protected IDictionary<TKey, ITrieNode<TKey, TValue>>? Children;

        private (bool isSet, TValue? value) _value;

        // It's not uncommon to have a 'hot path' that will be requested frequently.
        // This facilitates that by caching the last child that was requested.
        private (bool exists, TKey key, ITrieNode<TKey, TValue> child) _recentChild;

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

        public abstract ITrieNode<TKey, TValue> GetOrAddChild(TKey key);

        public bool TryGetChild(TKey key, out ITrieNode<TKey, TValue> child)
        {
            if(Children is null)
            {
                child = default!;
                return false;
            }

            var recent = _recentChild;
            if(recent.exists && recent.key!.Equals(key))
            {
                child = recent.child;
                return true;
            }

            bool found = Children!.TryGetValue(key, out child);
            if(found) _recentChild = (true, key, child);
            return found;
        }

        public ITrieNode<TKey, TValue> GetChild(TKey key)
        {
            var children = Children ?? throw new KeyNotFoundException();
            return children[key];
        }

        public TValue? Value => _value.value;
    }
}
