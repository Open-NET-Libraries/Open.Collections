using System.Collections.Generic;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class Trie<TKey, TValue> : TrieBase<TKey, TValue>
{
    /// <summary>
    /// Constructs a <see cref="Trie{TKey, TValue}"/>.
    /// </summary>
    public Trie()
        : base(() => new Node(), new HashSet<int>())
    { }

    private sealed class Node : NodeBase
    {
        private Dictionary<TKey, NodeBase>? _children;

        protected override IDictionary<TKey, NodeBase>? Children => _children;

        public override NodeBase GetOrAddChild(TKey key)
        {
            var children = _children;
            if (children is not null && children.TryGetValue(key, out var child))
                return child;

            children = _children ??= new();
            child = new Node();
            children[key] = child;

            return child;
        }
    }
}
