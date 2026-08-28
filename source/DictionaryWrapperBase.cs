namespace Open.Collections;

///<summary>
/// A base class for wrapping a collection as a dictionary.
///</summary>
[ExcludeFromCodeCoverage]
public abstract class DictionaryWrapperBase<TKey, TValue, TCollection>(
	TCollection source, bool owner = false)
	: CollectionWrapper<KeyValuePair<TKey, TValue>, TCollection>(source, owner), IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
	where TKey : notnull
	where TCollection : class, ICollection<KeyValuePair<TKey, TValue>>
{
	/// <inheritdoc />
	public TValue this[TKey key]
	{
		get => GetValueInternal(key);
		set => SetValueInternal(key, value);
	}

	/// <summary>
	/// Get the value for the key.
	/// </summary>
	protected abstract TValue GetValueInternal(TKey key);

	/// <summary>
	/// Set the value for the key.
	/// </summary>
	protected abstract void SetValueInternal(TKey key, TValue value);

	/// <inheritdoc />
	public abstract IReadOnlyCollection<TKey> Keys { get; }

	/// <summary>
	/// Get the key collection as an <see cref="ICollection{T}"/>.
	/// </summary>
	protected virtual ICollection<TKey> KeyCollection
		=> LazyInitializer.EnsureInitialized(ref field, () =>
		{
			var keys = ThrowIfDisposed(Keys);
			return keys is ICollection<TKey> k ? k : new ReadOnlyCollectionAdapter<TKey>(keys);
		})!;

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => KeyCollection;

	/// <inheritdoc />
	public abstract IReadOnlyCollection<TValue> Values { get; }

	/// <summary>
	/// Get the value collection as an <see cref="ICollection{T}"/>.
	/// </summary>
	protected virtual ICollection<TValue> ValueCollection
		=> LazyInitializer.EnsureInitialized(ref field, () =>
		{
			var values = ThrowIfDisposed(Values);
			return values is ICollection<TValue> v ? v : new ReadOnlyCollectionAdapter<TValue>(values);
		})!;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => ValueCollection;

	/// <summary>
	/// Add a key and value to the dictionary.
	/// </summary>
	protected abstract void AddInternal(TKey key, TValue value);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(TKey key, TValue value)
		=> AddInternal(key, value);

	/// <inheritdoc />
	public abstract bool ContainsKey(TKey key);

	/// <inheritdoc />
	public abstract bool Remove(TKey key);

	/// <inheritdoc />
	public abstract bool TryGetValue(TKey key,
#if NET9_0_OR_GREATER
		[MaybeNullWhen(false)]
#else
#endif
		out TValue value);
}
