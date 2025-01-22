using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

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

[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>(
	IDictionary<TKey, TValue> dictionary, bool owner = false)
	: ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>(dictionary, owner)
{
}

[ExcludeFromCodeCoverage]
public class ReadWriteSynchronizedDictionary<TKey, TValue>
	: ReadWriteSynchronizedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	public ReadWriteSynchronizedDictionary() : base(new Dictionary<TKey, TValue>()) { }

	public ReadWriteSynchronizedDictionary(int capacity) : base(new Dictionary<TKey, TValue>(capacity)) { }
}