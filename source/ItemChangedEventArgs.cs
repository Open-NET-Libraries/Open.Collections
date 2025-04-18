namespace Open.Collections;

/// <summary>
/// The possible changes to an item.
/// </summary>
public enum ItemChange
{
	/// <summary>
	/// Default value indicating no change.
	/// </summary>
	None,

	/// <summary>
	/// The item was added to the collection.
	/// </summary>
	Added,

	/// <summary>
	/// The item was removed from the collection.
	/// </summary>
	Removed,

	/// <summary>
	/// The item was inserted into the collection.
	/// </summary>
	Inserted,

	/// <summary>
	/// The item was replaced in the collection.
	/// </summary>
	Modified
}

/// <summary>
/// Event arguments for item changes.
/// </summary>
public class ItemChangedEventArgs<T>(
	ItemChange action, T value, int version)
	: EventArgs
{
	/// <summary>
	/// The action that caused the event to be raised.
	/// </summary>
	public readonly ItemChange Change = action;

	/// <summary>
	/// The value that was changed to.
	/// </summary>
	public readonly T Value = value;

	/// <summary>
	/// The version of the collection at the time of the change.
	/// </summary>
	public readonly int Version = version;
}

/// <summary>
/// Event arguments for item changes with an index.
/// </summary>
public class ItemChangedEventArgs<TIndex, TValue>(
	ItemChange action, TIndex index, TValue value, int version)
	: ItemChangedEventArgs<TValue>(action, value, version)
{
	/// <summary>
	/// The index of the item that was changed.
	/// </summary>
	public readonly TIndex Index = index;
}

/// <summary>
/// A static helper class for creating <see cref="ItemChangedEventArgs{T}"/> instances.
/// </summary>
public static class ItemChangeEventArgs
{
	/// <summary>
	/// Creates a new <see cref="ItemChangedEventArgs{T}"/> instance.
	/// </summary>
	public static ItemChangedEventArgs<T> CreateArgs<T>(
		this ItemChange change, T value, int version)
		=> new(change, value, version);

	/// <summary>
	/// Creates a new <see cref="ItemChangedEventArgs{TIndex, TValue}"/> instance.
	/// </summary>
	public static ItemChangedEventArgs<TIndex, TValue> CreateArgs<TIndex, TValue>(
		this ItemChange change, TIndex index, TValue value, int version)
		=> new(change, index, value, version);
}