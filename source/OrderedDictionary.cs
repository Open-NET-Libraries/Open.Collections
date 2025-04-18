namespace Open.Collections;

/// <summary>
/// A dictionary that maintains the order of items as they are added
/// and can be accessed by index.
/// </summary>
public class OrderedDictionary<TKey, TValue>
	: DictionaryWrapperBase<TKey, TValue, LinkedList<KeyValuePair<TKey, TValue>>>, IDictionary<TKey, TValue>
	where TKey : notnull
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public OrderedDictionary()
		: base(new LinkedList<KeyValuePair<TKey, TValue>>(), true)
		=> _lookup = [];

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public OrderedDictionary(int capacity)
		: base(new LinkedList<KeyValuePair<TKey, TValue>>(), true)
		=> _lookup = new(capacity);

	private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _lookup;

	/// <summary>
	/// The dictionary that can look up the node for a key.
	/// </summary>
	protected Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> Lookup
		=> _lookup ?? throw new ObjectDisposedException(GetType().ToString());

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	protected override void OnDispose()
	{
		InternalSource.Clear();
		Nullify(ref _lookup)?.Clear();
		base.OnDispose();
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override TValue GetValueInternal(TKey key) => Lookup[key].Value.Value;

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void SetValueInternal(TKey key, TValue value) => SetValue(key, value);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void AddInternal(in KeyValuePair<TKey, TValue> kvp)
		=> AddNode(in kvp);

	/// <summary>
	/// Updates a value by key and returns true if the value changed.
	/// </summary>
	public virtual bool SetValue(TKey key, TValue value)
	{
		AssertIsAlive();
		if (!Lookup.TryGetValue(key, out var node))
		{
			Add(key, value);
			return true;
		}

		if (node.Value.Value?.Equals(value) ?? value is null)
			return false;

		node.Value = new KeyValuePair<TKey, TValue>(key, value);
		return true;
	}

	/// <inheritdoc />
	protected override ICollection<TKey> GetKeys()
		=> new ReadOnlyCollectionAdapter<TKey>(
			ThrowIfDisposed(InternalSource.Select(e => e.Key)),
			() => InternalSource.Count);

	/// <inheritdoc />
	protected override ICollection<TValue> GetValues()
		=> new ReadOnlyCollectionAdapter<TValue>(
			ThrowIfDisposed(InternalSource.Select(e => e.Value)),
			() => InternalSource.Count);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void AddInternal(TKey key, TValue value)
		=> AddInternal(KeyValuePair.Create(key, value));

	/// <summary>
	/// Adds a node to the lookup dictionary.
	/// </summary>
#if NETSTANDARD2_0
#pragma warning disable IDE0079 // Remove unnecessary suppression
	[SuppressMessage("Roslynator", "RCS1242:Do not pass non-read-only struct by read-only reference.", Justification = "KeyValuePairs are not truly readonly until NET Standard 2.1.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
#endif
	protected virtual LinkedListNode<KeyValuePair<TKey, TValue>> AddNode(
		in KeyValuePair<TKey, TValue> kvp)
	{
		AssertIsAlive();
		var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(kvp);

		Lookup.Add(kvp.Key, node);
		InternalSource.AddLast(node);
		return node;
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool ContainsKey(TKey key)
		=> Lookup.ContainsKey(key);

	/// <inheritdoc />
	public override bool Remove(TKey key)
	{
		if (Lookup.TryGetValue(key, out var node))
		{
			bool removed = Lookup.Remove(key);
			Debug.Assert(removed);
			InternalSource.Remove(node);
			return true;
		}

		return false;
	}

	/// <inheritdoc />
	public override bool TryGetValue(TKey key, out TValue value)
	{
		if (Lookup.TryGetValue(key, out var node))
		{
			value = node.Value.Value;
			return true;
		}

		value = default!;
		return false;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		Lookup.Clear();
		base.Clear();
	}

	/// <inheritdoc />
	public override bool Remove(KeyValuePair<TKey, TValue> item)
	{
		var key = item.Key;
		if (Lookup.TryGetValue(key, out var node)
			&& (node.Value.Value?.Equals(item.Value) ?? item.Value is null))
		{
			Debug.Assert(key.Equals(node.Value.Key));
			bool removed = Lookup.Remove(key);
			Debug.Assert(removed);
			InternalSource.Remove(node);
			return true;
		}

		return false;
	}
}
