using Open.Threading;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized list that tracks changes.
/// </summary>
public class TrackedListWrapper<T> : TrackedCollectionWrapper<T, IList<T>>, IList<T>
{
	/// <summary>
	/// Constructs a new instance with the specified list and optional modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedListWrapper(IList<T> list, ModificationSynchronizer? sync = null) : base(list, sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with the specified list and a new modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedListWrapper(IList<T> list, out ModificationSynchronizer sync) : base(list, out sync)
	{
	}

	/// <inheritdoc />
	public T this[int index]
	{
		get => InternalSource[index];
		set => SetValue(index, value);
	}

	/// <inheritdoc />
	public bool SetValue(int index, T value)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() => SetValueInternal(index, value),
			version => OnChanged(ItemChange.Modified, index, value, version));

	private bool SetValueInternal(int index, T value)
	{
		bool changing
			= index >= InternalSource.Count
			|| !(InternalSource[index]?.Equals(value) ?? value is null);
		if (changing)
			InternalSource[index] = value;
		return changing;
	}

	/// <inheritdoc />
	public int IndexOf(T item)
		=> Sync!.Reading(
			() => AssertIsAlive()
			? InternalSource.IndexOf(item)
			: -1);

	/// <inheritdoc />
	public void Insert(int index, T item)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				InternalSource.Insert(index, item);
				return true;
			},
			version => OnChanged(ItemChange.Inserted, index, item, version));

	/// <inheritdoc />
	public override bool Remove(T item)
	{
		int i = -1;
		return Sync!.Modifying(
			() => AssertIsAlive()
				&& (i = InternalSource.IndexOf(item)) != -1,
			() =>
			{
				InternalSource.RemoveAt(i);
				return true;
			},
			version => OnChanged(ItemChange.Removed, i, item, version));
	}

	/// <returns>The item removed.</returns>
	/// <inheritdoc cref="IList{T}.RemoveAt(int)" />
	public T RemoveAt(int index)
	{
		T removed = default!;
		Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				removed = InternalSource[index];
				InternalSource.RemoveAt(index);
				return true;
			},
			version => OnChanged(ItemChange.Removed, index, removed, version));
		return removed;
	}

	void IList<T>.RemoveAt(int index) => RemoveAt(index);

	/// <summary>
	/// Synchronizes finding an item (<paramref name="target"/>), and if found, replaces it with the <paramref name="replacement"/>.
	/// </summary>
	/// <exception cref="ArgumentException">If <paramref name="throwIfNotFound"/> is true and the <paramref name="target"/> is not found.</exception>
	public bool Replace(T target, T replacement, bool throwIfNotFound = false)
	{
		AssertIsAlive();
		int index = -1;
		return !(target?.Equals(replacement) ?? replacement is null)
			&& Sync!.Modifying(
			() =>
			{
				var source = InternalUnsafeSource;
				AssertIsAlive();
				index = source!.IndexOf(target);
				return index != -1 || (throwIfNotFound ? throw new ArgumentException("Not found.", nameof(target)) : false);
			},
			() => SetValueInternal(index, replacement),
			version => OnChanged(ItemChange.Modified, index, replacement, version)
		);
	}
}

/// <summary>
/// A synchronized list that tracks changes.
/// </summary>
public sealed class TrackedList<T> : TrackedListWrapper<T>
{
	/// <summary>
	/// Constructs a new instance with the specified initial capacity and optional modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedList(int capacity, ModificationSynchronizer? sync = null)
		: base(new List<T>(capacity), sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with the specified initial capacity and a new modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedList(int capacity, out ModificationSynchronizer sync)
		: base(new List<T>(capacity), out sync)
	{
	}

	/// <summary>
	/// Constructs a new instance using the provided modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedList(ModificationSynchronizer? sync)
		: base([], sync)
	{
	}

	/// <summary>
	/// Constructs a new instance with a new modification synchronizer.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedList(out ModificationSynchronizer sync)
		: base([], out sync)
	{
	}

	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public TrackedList() : this(null) { }
}