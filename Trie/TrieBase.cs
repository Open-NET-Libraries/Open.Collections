﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;
/// <summary>
/// A generic Trie collection.
/// </summary>
public abstract class TrieBase<TKey, TValue>
	: ITrie<TKey, TValue>, ITrieNode<TKey, TValue>
	where TKey : notnull
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
	public bool TryGetValue(ReadOnlySpan<TKey> key, [MaybeNullWhen(false)] out TValue value)
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
	public bool TryGetValueFromPath(IEnumerable<TKey> key, [MaybeNullWhen(false)] out TValue value)
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
	public bool TryGetValueFromPath(ICollection<TKey> key, [MaybeNullWhen(false)] out TValue value)
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
	bool ITrieNode<TKey, TValue>.TryGetChild(TKey key, [MaybeNullWhen(false)] out ITrieNode<TKey, TValue> child)
		=> _root.TryGetChild(key, out child);

	/// <inheritdoc />
	public ITrieNode<TKey, TValue> GetChild(TKey key)
		=> _root.GetChild(key);

	/// <inheritdoc />
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
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

		private readonly struct ValueContainer(bool isSet, TValue value)
		{
			public ValueContainer(TValue value)
				: this(true, value) { }

			public bool IsSet { get; } = isSet;
			public TValue Value { get; } = value;
		}

		private ValueContainer _value;

		private readonly struct Recent(
			bool exists, TKey key, ITrieNode<TKey, TValue> child)
		{
			public bool Exists { get; } = exists;
			public TKey Key { get; } = key;
			public ITrieNode<TKey, TValue> Child { get; } = child;
		}

		// It's not uncommon to have a 'hot path' that will be requested frequently.
		// This facilitates that by caching the last child that was requested.
		private Recent _recentChild;

		public bool IsSet => _value.IsSet;

		internal TValue GetValueOrThrow()
			=> TryGetValue(out var value)
				? value
				: throw new Exception("Unexpected concurrency condition. Value was set, but then not available.");

		public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
		{
			var v = _value;
			if (v.IsSet)
			{
				value = v.Value;
				return true;
			}

			value = default!;
			return false;
		}

		public TValue GetOrAdd(in TValue value)
			=> TryGetValue(out var v)
				? v
				: TrySetValue(in value)
				? value
				: GetValueOrThrow();

		public bool TrySetValue(in TValue value)
		{
			var v = _value;
			if (v.IsSet) return false;
			SetValue(value);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void SetValue(TValue value)
			=> _value = new(value);

		public abstract ITrieNode<TKey, TValue> GetOrAddChild(TKey key);

		protected bool TryGetChildFrom(
			IDictionary<TKey, ITrieNode<TKey, TValue>> children,
			TKey key,
			[MaybeNullWhen(false)] out ITrieNode<TKey, TValue> child)
		{
			var recent = _recentChild;
			if (recent.Exists && recent.Key!.Equals(key))
			{
				child = recent.Child;
				return true;
			}

			bool found = children.TryGetValue(key, out child!);
			if (found) UpdateRecent(key, child!);
			return found;
		}

		public bool TryGetChild(
			TKey key,
			[MaybeNullWhen(false)] out ITrieNode<TKey, TValue> child)
		{
			if (Children is null)
			{
				child = default!;
				return false;
			}

			return TryGetChildFrom(Children, key, out child);
		}

		// Facilitate subclasses synchronizing writes to avoid torn reads.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void UpdateRecent(TKey key, ITrieNode<TKey, TValue> child)
			=> _recentChild = new(true, key, child);

		public ITrieNode<TKey, TValue> GetChild(TKey key)
			=> TryGetChild(key, out var child)
			? child
			: throw new KeyNotFoundException();

		public TValue? Value => _value.Value;
	}
}
