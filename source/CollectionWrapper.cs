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
	/// </summary>
	/// <remarks>
	/// This base implementation enumerates <paramref name="items"/> directly.
	/// The synchronized overrides materialize it to an array first, for two reasons:
	/// the lock must not be held while enumerating a caller-supplied sequence, which may
	/// be slow or yielding to a process; and the copy acts as a snapshot against a source
	/// whose <em>size</em> could change while the items are being added.
	/// The two exempted shapes are the ones the runtime guarantees are both cheap to enumerate
	/// -- no user callback, no blocking, no reentrant locking -- and fixed in extent for the
	/// duration of the call: <see cref="System.Array"/> and
	/// <see cref="System.Collections.Immutable.IImmutableList{T}"/>. An array&apos;s elements may change,
	/// but that cannot invalidate an enumeration in progress, and an immutable list cannot change at
	/// all. Every other <see cref="IEnumerable{T}"/> is copied because the type system gives no such
	/// guarantee -- not because its size is necessarily unstable.
	/// </remarks>
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
