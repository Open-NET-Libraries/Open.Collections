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

public class ItemChangedEventArgs<T> : EventArgs
{
	public readonly ItemChange Change;
	public readonly T Value;
	public readonly int Version;

	public ItemChangedEventArgs(ItemChange action, T value, int version)
	{
		Change = action;
		Value = value;
		Version = version;
	}
}

public class ItemChangedEventArgs<TIndex, TValue> : ItemChangedEventArgs<TValue>
{
	public readonly TIndex Index;
	public ItemChangedEventArgs(ItemChange action, TIndex index, TValue value, int version)
		: base(action, value, version) => Index = index;
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