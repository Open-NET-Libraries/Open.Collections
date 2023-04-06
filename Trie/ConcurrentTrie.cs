using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Open.Collections;

/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class ConcurrentTrie<TKey, TValue>
    : TrieBase<TKey, TValue>
{
    /// <summary>
    /// Constructs a <see cref="ConcurrentTrie{TKey, TValue}"/>.
    /// </summary>
    public ConcurrentTrie()
        : base(() => new Node())
    { }

    private sealed class Node : NodeBase
    {
        private ConcurrentDictionary<TKey, ITrieNode<TKey, TValue>>? _children;

        public override ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
        {
            var children = LazyInitializer.EnsureInitialized(ref _children, ()=>
            {
                var c = new ConcurrentDictionary<TKey, ITrieNode<TKey, TValue>>();
                Children = c;
                return c;
            })!;

            return children.GetOrAdd(key, _ => new Node());
        }
    }
}