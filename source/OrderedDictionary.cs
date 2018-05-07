/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using Open.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace Open.Collections
{
	/// <summary>
	/// Represents a generic items of key/source pairs that are ordered independently of the key and source.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Adds an entry with the specified key and source into the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> items with the lowest available index.
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="source">The source of the entry to add.</param>
		/// <returns>The index of the newly added entry</returns>
		/// <remarks>
		/// <para>You can also use the <see cref="P:System.Collections.Generic.IDictionary{TKey,TValue}.Item(TKey)"/> property to add new elements by setting the source of a key that does not exist in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> items; however, if the specified key already exists in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see>, setting the <see cref="P:Item(TKey)"/> property overwrites the old source. In contrast, the <see cref="M:AddSynchronized"/> method does not modify existing elements.</para></remarks>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see></exception>
		/// <exception cref="NotSupportedException">The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> is read-only.<br/>
		/// -or-<br/>
		/// The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> has a fized size.</exception>
		new int Add(TKey key, TValue value);

		/// <summary>
		/// Inserts a new entry into the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> items with the specified key and source at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="source">The source of the entry to add. The source can be <null/> if the type of the values in the dictionary is a reference type.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
		/// <exception cref="NotSupportedException">The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> is read-only.<br/>
		/// -or-<br/>
		/// The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> has a fized size.</exception>
		void Insert(int index, TKey key, TValue value);

		/// <summary>
		/// Gets or sets the source at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the source to get or set.</param>
		/// <source>The source of the item at the specified index.</source>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is equal to or greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
		TValue this[int index]
		{
			get;
			set;
		}
	}


	/// <summary>
	/// Represents a generic items of key/source pairs that are ordered independently of the key and source.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	public class OrderedDictionary<TKey, TValue> : DisposableBase, IOrderedDictionary<TKey, TValue>
	{

		public class ItemChangedEventArgs<TIKey, TIValue> : KeyValueChangedEventArgs<TKey, TValue>
		{
			public readonly int Index;

			public ItemChangedEventArgs(ItemChange action, int index, TKey key, TValue value) : base(action, key, value)
			{
				Index = index;
			}
			public ItemChangedEventArgs(ItemChange action, int index, TKey key, TValue previous, TValue value) : base(action, key, previous, value)
			{
				Index = index;
			}
		}

		public delegate void ItemChangedEventHandler<TIKey, TIValue>(Object source, ItemChangedEventArgs<TIKey, TIValue> e);

		public event ItemChangedEventHandler<TKey, TValue> ItemChanged;

		protected void OnItemChanged(ItemChange action, int index, TKey key, TValue value)
		{
			OnItemChanged(new ItemChangedEventArgs<TKey, TValue>(action, index, key, value));
		}

		protected void OnItemChanged(ItemChange action, int index, TKey key, TValue previous, TValue value)
		{
			OnItemChanged(new ItemChangedEventArgs<TKey, TValue>(action, index, key, previous, value));
		}

		protected virtual void OnItemChanged(ItemChangedEventArgs<TKey, TValue> e)
		{
			ItemChanged?.Invoke(this, e);
		}

		private const int DefaultInitialCapacity = 0;

		private static readonly string _keyTypeName = typeof(TKey).FullName;
		private static readonly string _valueTypeName = typeof(TValue).FullName;

		private Dictionary<TKey, TValue> _dictionary;
		private List<TKey> _list;
		private IEqualityComparer<TKey> _comparer;
		private readonly object _syncRoot = new Object();
		private readonly int _initialCapacity;


		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class.
		/// </summary>
		public OrderedDictionary()
			: this(DefaultInitialCapacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
		public OrderedDictionary(int capacity)
			: this(capacity, null)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Cannot be less than zero.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified comparer.
		/// </summary>
		/// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
		public OrderedDictionary(IEqualityComparer<TKey> comparer)
			: this(DefaultInitialCapacity, comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity and comparer.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items can contain.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
		public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Cannot be less than zero.");

			_initialCapacity = capacity;
			_comparer = comparer;
		}

		public OrderedDictionary(TKey[] keys, TValue[] values = null)
		{

			if (keys != null)
			{
				var hasValues = values != null && values.Length != 0;
				if (hasValues && values.Length > keys.Length)
					throw new Exception("Invalid initialization values.  Value array is longer than key array.");

				for (var i = 0; i < keys.Length; i++)
					AddInternal(keys[i], (hasValues && i < values.Length) ? values[i] : default);
			}
			else if (values != null && values.Length != 0)
			{
				throw new Exception("Invalid initialization values.  Values but no keys.");
			}

		}

		/// <summary>
		/// Converts the object passed as a key to the key type of the dictionary
		/// </summary>
		/// <param name="keyObject">The key object to check</param>
		/// <returns>The key object, cast as the key type of the dictionary</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyObject"/> is <null/>.</exception>
		/// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="keyObject"/>.</exception>
		private static TKey ConvertToKeyType(object keyObject)
		{
			return (TKey)keyObject;
		}

		/// <summary>
		/// Converts the object passed as a source to the source type of the dictionary
		/// </summary>
		/// <param name="source">The object to convert to the source type of the dictionary</param>
		/// <returns>The source object, converted to the source type of the dictionary</returns>
		/// <exception cref="ArgumentNullException"><paramref name="valueObject"/> is <null/>, and the source type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a source type.</exception>
		/// <exception cref="ArgumentException">The source type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
		private static TValue ConvertToValueType(object value)
		{
			if (value == null)
				return default;

			return (TValue)value;
		}

		/// <summary>
		/// Gets the dictionary object that stores the keys and values
		/// </summary>
		/// <source>The dictionary object that stores the keys and values for the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></source>
		/// <remarks>Accessing this property will create the dictionary object if necessary</remarks>
		private Dictionary<TKey, TValue> Dictionary
		{
			get
			{

				return LazyInitializer.EnsureInitialized(ref _dictionary, () => new Dictionary<TKey, TValue>(_initialCapacity, _comparer))
					?? new Dictionary<TKey, TValue>(_initialCapacity, _comparer); // Satisifes code contracts...
			}
		}

		/// <summary>
		/// Gets the list object that stores the key/source pairs.
		/// </summary>
		/// <source>The list object that stores the key/source pairs for the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></source>
		/// <remarks>Accessing this property will create the list object if necessary.</remarks>
		private List<TKey> List
		{
			get
			{

				return LazyInitializer.EnsureInitialized(ref _list, () => new List<TKey>(_initialCapacity))
					?? new List<TKey>(_initialCapacity); // Satisifes code contracts...
			}
		}

		private IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable()
		{

			foreach (var key in List)
				yield return KeyValuePair.Create(key, Dictionary[key]);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return AsEnumerable().GetEnumerator();
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return AsEnumerable().GetEnumerator();
		}

		/// <summary>
		/// Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items with the specified key and source at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="source">The source of the entry to add. The source can be <null/> if the type of the values in the dictionary is a reference type.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
		public void Insert(int index, TKey key, TValue value)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), index, "Cannot be negative.");

			lock (SyncRoot)
			{
				if (index > Count)
					throw new ArgumentOutOfRangeException(nameof(index), index, "Cannot be greater than the count.");

				Dictionary.Add(key, value);
				List.Insert(index, key);
			}
			OnItemChanged(ItemChange.Inserted, index, key, value);
		}


		/// <summary>
		/// Removes the entry at the specified index from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.
		/// </summary>
		/// <param name="index">The zero-based index of the entry to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// index is equal to or greater than <see cref="Count"/>.</exception>
		public void RemoveAt(int index)
		{
			if (index < 0) throw new ArgumentOutOfRangeException();

			TKey key;
			TValue value;

			lock (SyncRoot)
			{
				if (index > Count)
					throw new ArgumentOutOfRangeException(nameof(index), index, "Cannot be greater than the count.");

				key = List[index];
				value = Dictionary[key];

				List.RemoveAt(index);
				Dictionary.Remove(key);
			}
			OnItemChanged(ItemChange.Removed, index, key, value, default);
		}


		/// <summary>
		/// Gets or sets the source at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the source to get or set.</param>
		/// <source>The source of the item at the specified index.</source>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// index is equal to or greater than <see cref="Count"/>.</exception>
		public TValue this[int index]
		{
			get
			{
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();

				// Could still explode here when not synchronized, but just allow for it.
				return Dictionary[List[index]];
			}

			set
			{
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();

				TKey key;
				bool changed = false;
				TValue previous = default;
				lock (SyncRoot)
				{
					if (index > Count)
						throw new ArgumentOutOfRangeException(nameof(index), index, "Cannot be greater than the count.");

					key = List[index];

					changed = !Dictionary.TryGetValue(key, out previous) || !previous.Equals(value);
					if (changed)
						Dictionary[key] = value;
				}
				if (changed)
					OnItemChanged(ItemChange.Modified, index, key, previous, value);
			}
		}


		/// <summary>
		/// Adds an entry with the specified key and source into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items with the lowest available index.
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="source">The source of the entry to add. This source can be <null/>.</param>
		/// <remarks>A key cannot be <null/>, but a source can be.
		/// <para>You can also use the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property to add new elements by setting the source of a key that does not exist in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items; however, if the specified key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>, setting the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property overwrites the old source. In contrast, the <see cref="M:AddSynchronized"/> method does not modify existing elements.</para></remarks>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></exception>
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		/// <summary>
		/// Adds an entry with the specified key and source into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items with the lowest available index.
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="source">The source of the entry to add. This source can be <null/>.</param>
		/// <returns>The index of the newly added entry</returns>
		/// <remarks>A key cannot be <null/>, but a source can be.
		/// <para>You can also use the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property to add new elements by setting the source of a key that does not exist in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items; however, if the specified key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>, setting the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property overwrites the old source. In contrast, the <see cref="M:AddSynchronized"/> method does not modify existing elements.</para></remarks>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></exception>
		public int Add(TKey key, TValue value)
		{
			int count;
			lock (SyncRoot)
				count = AddInternal(key, value);
			return count;
		}

		private int AddInternal(TKey key, TValue value)
		{
			Dictionary.Add(key, value);
			List.Add(key);
			return Count;
		}


		/// <summary>
		/// Removes all elements from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.
		/// </summary>
		/// <remarks>The capacity is not changed as a response of calling this method.</remarks>
		public void Clear()
		{
			lock (SyncRoot)
			{
				OnBeforeCleared(EventArgs.Empty);
				Dictionary.Clear();
				List.Clear();
				OnAfterCleared(EventArgs.Empty);
			}

		}

		public event EventHandler BeforeCleared;
		protected virtual void OnBeforeCleared(EventArgs e)
		{
			BeforeCleared?.Invoke(this, e);
		}

		public event EventHandler AfterCleared;
		protected virtual void OnAfterCleared(EventArgs e)
		{
			AfterCleared?.Invoke(this, e);
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.</param>
		/// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items contains an element with the specified key otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		public bool ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items contains a specific value.
		/// </summary>
		/// <param name="key">The value to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.</param>
		/// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items contains an element with the specified value otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		public bool ContainsValue(TValue value)
		{
			// Following contract is risky due to syncronization.
			// Contract.Ensures(!Contract.Result<bool>() || this.Count > 0);

			return Dictionary.ContainsValue(value);
		}


		/// <summary>
		/// Gets a source indicating whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items is read-only.
		/// </summary>
		/// <source><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is read-only; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</source>
		/// <remarks>
		/// A items that is read-only does not allow the source, removal, or modification of elements after the items is created.
		/// <para>A items that is read-only is simply a items with a wrapper that prevents modification of the items; therefore, if changes are made to the underlying items, the read-only items reflects those changes.</para>
		/// </remarks>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
		/// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
		/// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
		public int IndexOfKey(TKey key)
		{
			if (key != null)
				throw new ArgumentNullException(nameof(key));

			return List.IndexOf(key);
		}

		/// <summary>
		/// Removes the entry with the specified key from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.
		/// </summary>
		/// <param name="key">The key of the entry to remove</param>
		/// <returns><see langword="true"/> if the key was exists and the corresponding element was removed; otherwise, <see langword="false"/></returns>
		public bool Remove(TKey key)
		{
			if (key != null)
				throw new ArgumentNullException(nameof(key));
			int index;
			TValue value;
			lock (SyncRoot)
			{
				index = IndexOfKey(key);
				if (index >= 0)
				{
					if (index > Count)
						throw new ArgumentOutOfRangeException(nameof(index), index, "Cannot be greater than the count.");
					List.RemoveAt(index);
					value = Dictionary[key];
					Dictionary.Remove(key);
				}
				else
					return false;
			}
			OnItemChanged(ItemChange.Removed, index, key, value, default);
			return true;
		}


		/// <summary>
		/// Gets or sets the source with the specified key.
		/// </summary>
		/// <param name="key">The key of the source to get or set.</param>
		/// <source>The source associated with the specified key. If the specified key is not exists, attempting to get it returns <null/>, and attempting to set it creates a new element using the specified key.</source>
		public TValue this[TKey key]
		{
			get
			{
				return Dictionary[key];
			}
			set
			{
				int index = -1;
				var change = ItemChange.None;
				TValue previous = default;
				lock (SyncRoot)
				{
					if (Dictionary.TryGetValue(key, out previous))
					{
						if (!previous.Equals(value))
						{
							Dictionary[key] = value;
							change = ItemChange.Modified;
						}

						index = List.IndexOf(key);
						if (index == -1)
						{
							List.Add(key);
							index = List.Count - 1;
						}

					}
					else
					{
						index = AddInternal(key, value) - 1;
						change = ItemChange.Added;
					}
				}
				if (change != ItemChange.None)
					OnItemChanged(change, index, key, previous, value);

			}
		}


		/// <summary>
		/// Gets the number of key/values pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.
		/// </summary>
		/// <source>The number of key/source pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> items.</source>
		public int Count
		{
			get
			{
				return List.Count;
			}
		}


		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> object.
		/// </summary>
		/// <source>An object that can be used to synchronize access to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> object.</source>
		public object SyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}


		/// <summary>
		/// Gets an <see cref="TLock:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
		/// </summary>
		/// <source>An <see cref="TLock:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</source>
		/// <remarks>The returned <see cref="TLock:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the items refers back to the keys in the source <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the key items.</remarks>
		public ICollection<TKey> Keys
		{
			get
			{
				return List.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets the source associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the source to get.</param>
		/// <param name="source">When this method returns, contains the source associated with the specified key, if the key is exists; otherwise, the default source for the type of <paramref name="source"/>. This parameter can be passed uninitialized.</param>
		/// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			var result = Dictionary.TryGetValue(key, out value);
			return result;
		}

		/// <summary>
		/// Gets an <see cref="TLock:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
		/// </summary>
		/// <source>An <see cref="TLock:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</source>
		/// <remarks>The returned <see cref="TLock:ICollection{TValue}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the items refers back to the values in the source <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the source items.</remarks>
		public ICollection<TValue> Values
		{
			get
			{
				return Dictionary.Values;
			}
		}

		/// <summary>
		/// Adds the specified source to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> with the specified key.
		/// </summary>
		/// <param name="item">The <see cref="TLock:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and source to add to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains a specific key and source.
		/// </summary>
		/// <param name="item">The <see cref="TLock:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		/// <returns><see langword="true"/> if <paramref name="keyValuePair"/> is exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, <see langword="false"/>.</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> to an array of type <see cref="TLock:KeyValuePair`2>"/>, starting at the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional array of type <see cref="TLock:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> that is the destination of the <see cref="TLock:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> elements copied from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. The array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes a key and source from the dictionary.
		/// </summary>
		/// <param name="item">The <see cref="TLock:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and source to remove from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		/// <returns><see langword="true"/> if the key and source represented by <paramref name="keyValuePair"/> is successfully exists and removed; otherwise, <see langword="false"/>. This method returns <see langword="false"/> if <paramref name="keyValuePair"/> is not exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		// Making OrderDictionary Disposable ensures that events get cleaned up.
		protected override void OnDispose(bool calledExplicitly)
		{
			// Release items and clear events.
			Clear();

			BeforeCleared = null;
			AfterCleared = null;

			ItemChanged = null;
		}
	}
}
