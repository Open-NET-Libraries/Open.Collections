using System.Collections.Concurrent;

namespace Open.Collections;

/// <summary>
/// A generic Trie collection.
/// </summary>
public sealed class ConcurrentTrie<TKey, TValue>
	: TrieBase<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Constructs a <see cref="ConcurrentTrie{TKey, TValue}"/>.
	/// </summary>
	public ConcurrentTrie()
		: base(() => new Node())
	{ }

	private sealed class Node : NodeBase
	{
		private readonly object _valueSync = new();

		protected override void SetValue(TValue value)
		{
			lock (_valueSync) base.SetValue(value);
		}

		private readonly object _childSync = new();

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
						Children = _children = children = new();
				}
			}

			return children.GetOrAdd(key, _ => new Node());
		}
	}
}