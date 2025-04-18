using Open.Threading;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class TrackedIndexedDictionaryWrapper<TKey, TValue, TDictionary>
	: TrackedDictionaryWrapper<TKey, TValue, TDictionary>, IIndexedDictionary<TKey, TValue>
	where TKey : notnull
	where TDictionary : class, IIndexedDictionary<TKey, TValue>
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionaryWrapper(TDictionary dictionary, ModificationSynchronizer? sync = null)
	: base(dictionary, sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionaryWrapper(TDictionary dictionary, out ModificationSynchronizer sync)
		: base(dictionary, out sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual TKey GetKeyAt(int index)
		=> Sync!.Reading(() => InternalUnsafeSource!.GetKeyAt(index));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual TValue GetValueAt(int index)
		=> Sync!.Reading(() => InternalUnsafeSource!.GetValueAt(index));

	/// <inheritdoc />
	public void Insert(int index, TKey key, TValue value)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				InternalUnsafeSource!.Insert(index, key, value);
				return true;
			},
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Inserted, index, KeyValuePair.Create(key, value), version);
			});

	/// <inheritdoc cref="IList{T}.RemoveAt(int)" />
	public void RemoveAt(int index)
	{
		TKey key = default!;
		TValue value = default!;
		Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				var source = InternalUnsafeSource!;
				key = source.GetKeyAt(index);
				value = source[key];
				source.RemoveAt(index);
				return true;
			},
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Removed, index, KeyValuePair.Create(key, value), version);
			});
	}

	/// <inheritdoc />
	public new int SetValue(TKey key, TValue value)
	{
		SetValue(key, value, out int index);
		return index;
	}

	/// <inheritdoc />
	public bool SetValue(TKey key, TValue value, out int index)
	{
		int i = -1;
		bool result = Sync!.Modifying(
			AssertIsAliveDelegate,
			() => InternalUnsafeSource!.SetValue(key, value, out i),
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Modified, i, KeyValuePair.Create(key, value), version);
			});
		index = i;
		return result;
	}

	/// <inheritdoc />
	public bool SetValueAt(int index, TValue value, out TKey key)
	{
		TKey k = default!;
		bool result = Sync!.Modifying(
			AssertIsAliveDelegate,
			() => InternalUnsafeSource!.SetValueAt(index, value, out k),
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Modified, index, KeyValuePair.Create(k, value), version);
			});
		key = k;
		return result;
	}

	/// <summary>
	/// Synchronizes adding an item to the internal collection.
	/// </summary>
	protected override int AddSynchronized(TKey key, TValue value)
	{
		int index = -1;
		Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				index = InternalUnsafeSource!.Add(key, value);
				return true;
			},
			version =>
			{
				if (HasChangedListeners) // Avoid creating KVP unnecessarily.
					OnChanged(ItemChange.Added, index, KeyValuePair.Create(key, value), version);
			});
		return index;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public new int Add(TKey key, TValue value)
		=> AddSynchronized(key, value);
}

/// <summary>
/// A synchronized wrapper for a dictionary that uses a <see cref="ModificationSynchronizer"/> for synchronization.
/// </summary>
public class TrackedIndexedDictionaryWrapper<TKey, TValue>
	: TrackedIndexedDictionaryWrapper<TKey, TValue, IIndexedDictionary<TKey, TValue>>
	where TKey : notnull
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionaryWrapper(IIndexedDictionary<TKey, TValue> dictionary, ModificationSynchronizer? sync = null)
		: base(dictionary, sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionaryWrapper(IIndexedDictionary<TKey, TValue> dictionary, out ModificationSynchronizer sync)
		: base(dictionary, out sync)
	{
	}
}

/// <summary>
/// A synchronized dictionary that uses a <see cref="ModificationSynchronizer"/> for synchronization.
/// </summary>
public sealed class TrackedIndexedDictionary<TKey, TValue>
	: TrackedIndexedDictionaryWrapper<TKey, TValue>
	where TKey : notnull
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionary(int capacity, ModificationSynchronizer? sync = null)
		: base(new IndexedDictionary<TKey, TValue>(capacity), sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionary(int capacity, out ModificationSynchronizer sync)
		: base(new IndexedDictionary<TKey, TValue>(capacity), out sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionary(ModificationSynchronizer? sync)
		: base(new IndexedDictionary<TKey, TValue>(), sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionary(out ModificationSynchronizer sync)
		: base(new IndexedDictionary<TKey, TValue>(), out sync)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TrackedIndexedDictionary() : this(null)
	{
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override TKey GetKeyAt(int index)
		=> InternalSource.GetKeyAt(index);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override TValue GetValueAt(int index)
		=> InternalSource.GetValueAt(index);
}
