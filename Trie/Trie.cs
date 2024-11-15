using System.Collections.Generic;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class Trie<TKey, TValue>(
	IEqualityComparer<TKey>? equalityComparer = null)
	: TrieBase<TKey, TValue>(() => new Node(equalityComparer))
	where TKey : notnull
{
	private sealed class Node(IEqualityComparer<TKey>? equalityComparer)
		: NodeBase
	{
		private Dictionary<TKey, ITrieNode<TKey, TValue>>? _children;

		public override ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
		{
			var children = _children;
			if (children is null)
				Children = _children = children = equalityComparer is null ? new() : new(equalityComparer);
			else if (TryGetChildFrom(children, key, out var c))
				return c;

			var child = new Node(equalityComparer);
			children[key] = child;
			return child;
		}
	}
}
