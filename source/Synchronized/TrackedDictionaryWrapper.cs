using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized dictionary that can be tracked for changes.
/// </summary>
public class TrackedDictionaryWrapper<TKey, TValue, TDictionary>
	: TrackedCollectionWrapper<KeyValuePair<TKey, TValue>, TDictionary>, IDictionary<TKey, TValue>
	where TKey : notnull
	where TDictionary : class, IDictionary<TKey, TValue>
{
	/// <summary>
	/// Construct a new instance with the provide dictionary and optional synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionaryWrapper(TDictionary dictionary, ModificationSynchronizer? sync = null)
	: base(dictionary, sync)
	{
	}

	/// <summary>
	/// Construct a new instance with the provide dictionary and a new synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionaryWrapper(TDictionary dictionary, out ModificationSynchronizer sync)
		: base(dictionary, out sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public ICollection<TKey> Keys => InternalSource.Keys;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public ICollection<TValue> Values => InternalSource.Values;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool TryGetValue(TKey key,
#if NET9_0_OR_GREATER
		[MaybeNullWhen(false)]
#else
#endif
		out TValue value)
	{
		TValue? v = default;
		bool result = Sync!.Reading(() => InternalSource.TryGetValue(key, out v));
		value = v!;
		return result;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TValue this[TKey key]
	{
		get => InternalSource[key];
		set => SetValue(key, value);
	}

	/// <inheritdoc />
	public bool SetValue(TKey key, TValue value)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() => SetValueInternal(key, value),
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Modified, KeyValuePair.Create(key, value), version);
			});

	private bool SetValueInternal(TKey key, TValue value)
	{
		bool changing
			= !InternalSource.TryGetValue(key, out var current)
			|| !(current?.Equals(value) ?? value is null);
		if (changing)
			InternalSource[key] = value;
		return changing;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool ContainsKey(TKey key)
		=> Sync!.Reading(
			() => AssertIsAlive() && InternalSource.ContainsKey(key));

	/// <inheritdoc />
	protected virtual int AddSynchronized(TKey key, TValue value)
	{
		Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				InternalSource.Add(key, value);
				return true;
			},
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Added, KeyValuePair.Create(key, value), version);
			});
		return -1;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void Add(TKey key, TValue value)
		=> AddSynchronized(key, value);

	/// <inheritdoc />
	public bool Remove(TKey key)
	{
		TValue? value = default;
		return Sync!.Modifying(
			() => AssertIsAlive()
				&& InternalSource.TryGetValue(key, out value),
			() =>
			{
				bool result = InternalSource.Remove(key);
				Debug.Assert(result); // Should always be true unless out of sync.
				return result;
			},
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Removed, KeyValuePair.Create(key, value!), version);
			});
	}
}

/// <inheritdoc cref="TrackedDictionaryWrapper{TKey, TValue, TDictionary}"/>
public class TrackedDictionaryWrapper<TKey, TValue>
	: TrackedDictionaryWrapper<TKey, TValue, IDictionary<TKey, TValue>>
	where TKey : notnull
{
	/// <inheritdoc cref="TrackedDictionaryWrapper{TKey, TValue, TDictionary}.TrackedDictionaryWrapper(TDictionary, ModificationSynchronizer?)"/>
	[ExcludeFromCodeCoverage]
	public TrackedDictionaryWrapper(IDictionary<TKey, TValue> dictionary, ModificationSynchronizer? sync = null)
		: base(dictionary, sync)
	{
	}

	/// <inheritdoc cref="TrackedDictionaryWrapper{TKey, TValue, TDictionary}.TrackedDictionaryWrapper(TDictionary, out ModificationSynchronizer)"/>
	[ExcludeFromCodeCoverage]
	public TrackedDictionaryWrapper(IDictionary<TKey, TValue> dictionary, out ModificationSynchronizer sync)
		: base(dictionary, out sync)
	{
	}
}

/// <inheritdoc cref="TrackedDictionaryWrapper{TKey, TValue, TDictionary}"/>
public class TrackedDictionary<TKey, TValue>
	: TrackedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	/// <summary>
	/// Constructs a new instance with the specified capacity and optional synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionary(int capacity, ModificationSynchronizer? sync = null)
		: base(new Dictionary<TKey, TValue>(capacity), sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with the specified capacity and a new synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionary(int capacity, out ModificationSynchronizer sync)
		: base(new Dictionary<TKey, TValue>(capacity), out sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with an optional synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionary(ModificationSynchronizer? sync = null)
		: base(new Dictionary<TKey, TValue>(), sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with a new synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionary(out ModificationSynchronizer sync)
		: base(new Dictionary<TKey, TValue>(), out sync)
	{
	}

	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedDictionary() : this(null) { }
}
