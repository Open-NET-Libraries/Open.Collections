using Open.Threading;

namespace Open.Collections.Synchronized;

/// <summary>
/// A read/write synchronized wrapper for a dictionary.
/// </summary>
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, TDictionary>(
	TDictionary dictionary, bool owner = false)
	: ReadWriteSynchronizedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>(dictionary, owner), IDictionary<TKey, TValue>
	where TDictionary : class, IDictionary<TKey, TValue>
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual TValue this[TKey key]
	{
		get => InternalSource[key];
		set
		{
			// With a dictionary, setting can be like adding.
			// Collection size might change.  Gotta be careful.
			using var upgradable = RWLock.UpgradableReadLock();
			if (InternalSource.ContainsKey(key))
			{
				InternalSource[key] = value;
				return;
			}

			using var write = RWLock.WriteLock();
			InternalSource[key] = value;
		}
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
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Add(TKey key, TValue value)
	{
		using var write = RWLock.WriteLock();
		InternalSource.Add(key, value);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsKey(TKey key)
		=> InternalSource.ContainsKey(key);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Remove(TKey key)
	{
		using var write = RWLock.WriteLock();
		return InternalSource.Remove(key);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue(TKey key,
#if NET9_0_OR_GREATER
		[MaybeNullWhen(false)]
#endif
		out TValue value)
		=> InternalSource.TryGetValue(key, out value);

	/// <inheritdoc />
	public virtual bool IfContainsKey(TKey key, Action<IDictionary<TKey, TValue>> action)
	{
		using var uLock = RWLock.UpgradableReadLock();
		if (!InternalSource.ContainsKey(key)) return false;
		using var wLock = RWLock.WriteLock();
		action(InternalSource);
		return true;
	}

	/// <inheritdoc />
	public virtual bool IfNotContainsKey(TKey key, Action<IDictionary<TKey, TValue>> action)
	{
		using var uLock = RWLock.UpgradableReadLock();
		if (InternalSource.ContainsKey(key)) return false;
		using var wLock = RWLock.WriteLock();
		action(InternalSource);
		return true;
	}
}

/// <summary>
/// A read/write synchronized wrapper for a dictionary.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>(
	IDictionary<TKey, TValue> dictionary, bool owner = false)
	: ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>(dictionary, owner)
{
}

/// <summary>
/// A read/write synchronized dictionary.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionary<TKey, TValue>
	: ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Constructs a new instance of <see cref="ReadWriteSynchronizedDictionary{TKey, TValue}"/> with the default capacity.
	/// </summary>
	public ReadWriteSynchronizedDictionary() : base(new Dictionary<TKey, TValue>()) { }

	/// <summary>
	/// Constructs a new instance of <see cref="ReadWriteSynchronizedDictionary{TKey, TValue}"/> with the specified capacity.
	/// </summary>
	public ReadWriteSynchronizedDictionary(int capacity) : base(new Dictionary<TKey, TValue>(capacity)) { }
}