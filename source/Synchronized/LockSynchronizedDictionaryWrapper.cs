﻿namespace Open.Collections.Synchronized;

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
		set
		{
			// With a dictionary, setting can be like adding.
			// Collection size might change.  Gotta be careful.
			lock (Sync) InternalSource[key] = value;
		}
	}

	ICollection<TKey>? _keys;
	/// <inheritdoc />
	public ICollection<TKey> Keys => _keys ??= GetKeys();
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual ICollection<TKey> GetKeys()
	=> new ReadOnlyCollectionAdapter<TKey>(
		ThrowIfDisposed(InternalSource.Keys),
		() => InternalSource.Count);

	ICollection<TValue>? _values;
	/// <inheritdoc />
	public ICollection<TValue> Values => _values ??= GetValues();
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual ICollection<TValue> GetValues()
		=> new ReadOnlyCollectionAdapter<TValue>(
			ThrowIfDisposed(InternalSource.Values),
			() => InternalSource.Count);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Add(TKey key, TValue value)
	{
		lock (Sync) InternalSource.Add(key, value);
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsKey(TKey key)
		=> InternalSource.ContainsKey(key);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Remove(TKey key)
	{
		lock (Sync) return InternalSource.Remove(key);
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue(TKey key,
#if NET9_0_OR_GREATER
		[MaybeNullWhen(false)]
#endif
		out TValue value)
		=> InternalSource.TryGetValue(key, out value);
}

[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionaryWrapper<TKey, TValue>(
	IDictionary<TKey, TValue> dictionary)
	: LockSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>(dictionary)
{
}

[ExcludeFromCodeCoverage]
public class LockSynchronizedDictionary<TKey, TValue>
	: LockSynchronizedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	public LockSynchronizedDictionary(int capacity) : base(new Dictionary<TKey, TValue>(capacity)) { }
	public LockSynchronizedDictionary() : base(new Dictionary<TKey, TValue>()) { }
}