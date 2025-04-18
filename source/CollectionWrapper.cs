namespace Open.Collections;

/// <summary>
/// A disposable wrapper for a collection.
/// </summary>
[ExcludeFromCodeCoverage]
public class CollectionWrapper<T, TCollection>(
	TCollection source, bool owner = false)
	: ReadOnlyCollectionWrapper<T, TCollection>(source, owner), ICollection<T>, IAddMultiple<T>
	where TCollection : class, ICollection<T>
{
	/// <summary>
	/// The underlying object used for synchronization.
	/// </summary>
#if NET9_0_OR_GREATER
	protected readonly Lock Sync = new();
#else
	protected readonly object Sync = new();
#endif

	/// <summary>
	/// The object used for synchronization.
	/// This is exposed to allow for more complex synchronization operations.
	/// </summary>
#if NET9_0_OR_GREATER
	public Lock SyncRoot => Sync;
#else
	public object SyncRoot => Sync;
#endif

	#region Implementation of ICollection<T>

	/// <summary>
	/// Manages adding an item to the collection.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void AddInternal(in T item)
		=> InternalUnsafeSource!.Add(item);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Add(T item)
	{
		AssertIsAlive();
		AddInternal(in item);
	}

#if NET9_0_OR_GREATER
	/// <inheritdoc cref="IAddMultiple{T}.AddThese(T, T, ReadOnlySpan{T})"/>
	public virtual void AddThese(T item1, T item2, params ReadOnlySpan<T> items)
#else
	/// <inheritdoc cref="IAddMultiple{T}.AddThese(T, T, T[])"/>
	public virtual void AddThese(T item1, T item2, params T[] items)
#endif
	{
		AssertIsAlive();
		AddInternal(in item1);
		AddInternal(in item2);
		foreach (T? i in items)
			AddInternal(in i);
	}

	/// <summary>
	/// Adds multiple items to the collection.
	/// It's important to avoid locking for too long so an array is used to add multiple items.
	/// An enumerable is potentially slow as it may be yielding to a process.
	/// </summary>
	/// <param name="items">The items to add.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void AddRange(IEnumerable<T> items)
	{
		AssertIsAlive();
		if (items is null) return;
		foreach (var i in items)
			AddInternal(in i);
	}

	/// <inheritdoc cref="AddRange(IEnumerable{T})"/>
#if NET9_0_OR_GREATER
	[OverloadResolutionPriority(1)]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void AddRange(ReadOnlySpan<T> items)
	{
		AssertIsAlive();
		foreach (var i in items)
			AddInternal(in i);
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Clear()
		=> InternalSource.Clear();

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Remove(T item)
		=> InternalSource.Remove(item);

	/// <inheritdoc />
	public override bool IsReadOnly
		=> InternalSource.IsReadOnly;
	#endregion
}
