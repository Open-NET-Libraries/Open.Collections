using System;

namespace Open.Collections
{

	public enum ItemChange
	{
		None,
		Added,
		Removed,
		Inserted,
		Modified
	}

	public class ItemChangedEventArgs<TIValue> : EventArgs
	{
		public readonly ItemChange Change;
		public readonly TIValue Value;
		public readonly TIValue PreviousValue;

		public ItemChangedEventArgs(ItemChange action, TIValue value)
		{
			Change = action;
			Value = value;
		}

		public ItemChangedEventArgs(ItemChange action, TIValue previous, TIValue value) : this(action, value)
		{
			PreviousValue = previous;
		}
	}

	public delegate void ItemChangedEventHandler<TIValue>(Object source, ItemChangedEventArgs<TIValue> e);

	public class KeyValueChangedEventArgs<TIKey, TIValue> : ItemChangedEventArgs<TIValue>
	{
		public readonly TIKey Key;

		public KeyValueChangedEventArgs(ItemChange action, TIKey key, TIValue value) : base(action, value)
		{
			Key = key;
		}

		public KeyValueChangedEventArgs(ItemChange action, TIKey key, TIValue previous, TIValue value) : base(action, previous, value)
		{
			Key = key;
		}
	}

	public delegate void KeyValueChangedEventHandler<TIKey, TIValue>(Object source, KeyValueChangedEventArgs<TIKey, TIValue> e);

}