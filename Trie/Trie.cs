using System.Collections.Generic;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class Trie<TKey, TValue>
    : TrieBase<TKey, TValue>
{
    /// <summary>
    /// Constructs a <see cref="Trie{TKey, TValue}"/>.
    /// </summary>
    public Trie()
        : base(() => new Node())
    { }

    private sealed class Node : NodeBase
    {
        private Dictionary<TKey, ITrieNode<TKey, TValue>>? _children;

        public override ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
        {
            var children = _children;
            if (children is null)
                Children = _children = children = new();
            else if(children.TryGetValue(key, out var c))
                return c;

            var child = new Node();
            children[key] = child;
            return child;
        }
    }
}
