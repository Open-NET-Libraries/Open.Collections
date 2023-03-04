using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Open.Collections;

/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class ConcurrentTrie<TKey, TValue> : TrieBase<TKey, TValue>
{
    /// <summary>
    /// Constructs a <see cref="ConcurrentTrie{TKey, TValue}"/>.
    /// </summary>
    public ConcurrentTrie()
        : base(() => new Node(), new ConcurrentHashSetInternal<int>())
    { }

    private sealed class Node : NodeBase
    {
        private ConcurrentDictionary<TKey, NodeBase>? _children;

        protected override IDictionary<TKey, NodeBase>? Children => _children;

        public override NodeBase GetOrAddChild(TKey key)
        {
            var children = LazyInitializer.EnsureInitialized(ref _children, () => new())!;
            return children.GetOrAdd(key, _ => new Node());
        }
    }
}