using System.Collections.Generic;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class Trie<TKey, TValue>
	: TrieBase<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Constructs a <see cref="Trie{TKey, TValue}"/>.
	/// </summary>
	public Trie(IEqualityComparer<TKey>? equalityComparer = null)
		: base(() => new Node(equalityComparer))
	{ }

	private sealed class Node : NodeBase
	{
		private readonly IEqualityComparer<TKey>? _equalityComparer;
		private Dictionary<TKey, ITrieNode<TKey, TValue>>? _children;

		public Node(IEqualityComparer<TKey>? equalityComparer)
			=> _equalityComparer = equalityComparer;

		public override ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
		{
			var children = _children;
			if (children is null)
				Children = _children = children = _equalityComparer is null ? new() : new(_equalityComparer);
			else if (TryGetChildFrom(children, key, out var c))
				return c;

			var child = new Node(_equalityComparer);
			children[key] = child;
			return child;
		}
	}
}
