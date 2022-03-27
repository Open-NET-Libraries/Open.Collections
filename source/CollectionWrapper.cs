using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public class CollectionWrapper<T, TCollection> : ReadOnlyCollectionWrapper<T, TCollection>, ICollection<T>
	where TCollection : class, ICollection<T>
{
	public CollectionWrapper(TCollection source, bool owner = false)
        : base(source, owner)
	{
	}

    protected readonly object Sync = new(); // Could possibly override..

    /// <summary>
    /// The underlying object used for synchronization.
    /// This is exposed to allow for more complex synchronization operations.
    /// </summary>
    public object SyncRoot => Sync;

    #region Implementation of ICollection<T>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void AddInternal(T item)
        => InternalSource.Add(item);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(T item) => AddInternal(item);

    /// <summary>Adds more than one item.</summary>
    /// <param name="item1">First item to add.</param>
    /// <param name="item2">Additional item to add.</param>
    /// <param name="items">Extended param items to add.</param>
    public virtual void AddThese(T item1, T item2, params T[] items)
    {
        AddInternal(item1);
        AddInternal(item2);
        foreach (T? i in items)
            AddInternal(i);
    }

    /// <summary>
    /// Adds mutliple items to the collection.
    /// It's important to avoid locking for too long so an array is used to add multiple items.
    /// An enumerable is potentially slow as it may be yielding to a process.
    /// </summary>
    /// <param name="items">The items to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void AddRange(IEnumerable<T> items)
	{
        foreach (var i in items)
			AddInternal(i);
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
