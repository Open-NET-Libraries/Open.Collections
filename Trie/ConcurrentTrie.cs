using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Open.Collections;

/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class ConcurrentTrie<TKey, TValue>(
	IEqualityComparer<TKey>? equalityComparer = null)
	: TrieBase<TKey, TValue>(() => new Node(equalityComparer))
	where TKey : notnull
{
	private sealed class Node(IEqualityComparer<TKey>? equalityComparer) : NodeBase
	{
		private readonly object _valueSync = new();

		protected override void SetValue(TValue value)
		{
			lock (_valueSync) base.SetValue(value);
		}

		private readonly object _childSync = new();

		private readonly IEqualityComparer<TKey>? _equalityComparer = equalityComparer;
		private ConcurrentDictionary<TKey, ITrieNode<TKey, TValue>>? _children;

		protected override void UpdateRecent(TKey key, ITrieNode<TKey, TValue> child)
		{
			lock (_childSync) base.UpdateRecent(key, child);
		}

		public override ITrieNode<TKey, TValue> GetOrAddChild(TKey key)
		{
			var children = _children;
			if (children is null)
			{
				lock (_childSync)
				{
					if (children is null)
						Children = _children = children = _equalityComparer is null ? new() : new(_equalityComparer);
				}
			}

			return children.GetOrAdd(key, _ => new Node(_equalityComparer));
		}
	}
}