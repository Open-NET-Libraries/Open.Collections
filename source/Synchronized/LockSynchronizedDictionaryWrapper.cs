namespace Open.Collections.Synchronized;

/// <summary>
/// A Monitor synchronized wrapper for a dictionary.
/// </summary>
[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionaryWrapper<TKey, TValue, TDictionary>(TDictionary dictionary)
	: LockSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>(dictionary), IDictionary<TKey, TValue>
	where TDictionary : class, IDictionary<TKey, TValue>
{
	/// <inheritdoc />
	public virtual TValue this[TKey key]
	{
		get => InternalSource[key];
		set => SetValueInternal(key, value);
	}

	/// <summary>
	/// Stores the value for the key.
	/// </summary>
	/// <remarks>
	/// Override to provide a faster path when the concrete dictionary type is known.
	/// </remarks>
	protected virtual void SetValueInternal(TKey key, TValue value)
	{
		lock (Sync) InternalSource[key] = value;
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => InternalSource.Keys;

	/// <inheritdoc cref="IDictionary{TKey, TValue}.Keys"/>
	public IReadOnlyCollection<TKey> Keys
		=> LazyInitializer.EnsureInitialized(ref field, () =>
		{
			var keys = InternalSource.Keys;
			return keys is IReadOnlyCollection<TKey> k ? k : new ReadOnlyCollectionAdapter<TKey>(keys);
		})!;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => InternalSource.Values;
	/// <inheritdoc cref="IDictionary{TKey, TValue}.Values"/>
	public IReadOnlyCollection<TValue> Values
		=> LazyInitializer.EnsureInitialized(ref field, () =>
		{
			var values = InternalSource.Values;
			return values is IReadOnlyCollection<TValue> v ? v : new ReadOnlyCollectionAdapter<TValue>(values);
		})!;

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Add(TKey key, TValue value)
	{
		lock (Sync) InternalSource.Add(key, value);
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool ContainsKey(TKey key)
		=> InternalSource.ContainsKey(key);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Remove(TKey key)
	{
		lock (Sync) return InternalSource.Remove(key);
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool TryGetValue(TKey key,
#if NET10_0_OR_GREATER
		[MaybeNullWhen(false)]
#endif
		out TValue value)
		=> InternalSource.TryGetValue(key, out value);
}

/// <summary>
/// A Monitor synchronized wrapper for a dictionary.
/// </summary>
[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionaryWrapper<TKey, TValue>(
	IDictionary<TKey, TValue> dictionary)
	: LockSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>(dictionary)
{
}

/// <summary>
/// A Monitor synchronized <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionary<TKey, TValue>
	: LockSynchronizedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Constructs a new instance of <see cref="LockSynchronizedDictionary{TKey, TValue}"/> with the specified capacity.
	/// </summary>
	public LockSynchronizedDictionary(int capacity) : base(new Dictionary<TKey, TValue>(capacity)) { }

	/// <summary>
	/// Constructs a new instance of <see cref="LockSynchronizedDictionary{TKey, TValue}"/> with the default capacity.
	/// </summary>
	public LockSynchronizedDictionary() : base(new Dictionary<TKey, TValue>()) { }
}
