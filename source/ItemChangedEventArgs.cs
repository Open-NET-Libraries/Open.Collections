using System;

// ReSharper disable NotAccessedField.Global
// ReSharper disable MemberCanBeProtected.Global

namespace Open.Collections;

public enum ItemChange
{
	None,
	Added,
	Removed,
	Inserted,
	Modified
}

public class ItemChangedEventArgs<T>(
	ItemChange action, T value, int version)
	: EventArgs
{
	public readonly ItemChange Change = action;
	public readonly T Value = value;
	public readonly int Version = version;
}

public class ItemChangedEventArgs<TIndex, TValue>(
	ItemChange action, TIndex index, TValue value, int version)
	: ItemChangedEventArgs<TValue>(action, value, version)
{
	public readonly TIndex Index = index;
}

public static class ItemChangeEventArgs
{
	public static ItemChangedEventArgs<T> CreateArgs<T>(
		this ItemChange change, T value, int version)
		=> new(change, value, version);

	public static ItemChangedEventArgs<TIndex, TValue> CreateArgs<TIndex, TValue>(
		this ItemChange change, TIndex index, TValue value, int version)
		=> new(change, index, value, version);
}