namespace Open.Collections;

/// <summary>
/// Represents a generic items of key/value pairs that are ordered independently of the key and value.
/// </summary>
/// <inheritdoc />
public interface IIndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
	/// <summary>
	/// Adds an entry with the specified key and value into the <see cref="IIndexedDictionary{TKey,TValue}"/>.
	/// </summary>
	/// <returns>The index of the newly added entry</returns>
	/// <inheritdoc cref="Insert(int, TKey, TValue)"/>
	new int Add(TKey key, TValue value);

	/// <summary>
	/// Inserts a new entry into the <see cref="IIndexedDictionary{TKey,TValue}"/> items with the specified key and value at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index at which the element should be inserted.</param>
	/// <param name="key">The key of the entry to add.</param>
	/// <param name="value">The value of the entry to add. The value can be <see langword="null"/> if the type of the values in the dictionary is a reference type.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
	/// -or-<br/>
	/// <paramref name="index"/> is greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
	/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IIndexedDictionary{TKey,TValue}"/>.</exception>
	/// <exception cref="NotSupportedException">The <see cref="IIndexedDictionary{TKey,TValue}"/> is read-only.<br/>
	/// -or-<br/>
	/// The <see cref="IIndexedDictionary{TKey,TValue}"/> has a fized size.</exception>
	void Insert(int index, TKey key, TValue value);

	/// <summary>
	/// Updates or creates and item.
	/// </summary>
	/// <returns><see langword="true"/> if the value was updated; otherwise <see langword="false"/>.</returns>
	bool SetValue(TKey key, TValue value, out int index);

	/// <summary>
	/// Updates or creates and item.
	/// </summary>
	/// <returns>The index that was updated or added.</returns>
	int SetValue(TKey key, TValue value);

	/// <summary>
	/// Returns the key at the requested <paramref name="index"/>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The index is less than zero or greater than the length of the collection.</exception>
	TKey GetKeyAt(int index);

	/// <summary>
	/// Returns the value at the requested <paramref name="index"/>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The index is less than zero or greater than the length of the collection.</exception>
	TValue GetValueAt(int index);

	/// <summary>
	/// Sets the <paramref name="value"/> of an item at the specified <paramref name="index"/>.
	/// </summary>
	/// <remarks><see langword="true"/> if the value changed; otherwise <see langword="false"/>.</remarks>
	/// <exception cref="ArgumentOutOfRangeException">The index is less than zero or greater than the length of the collection.</exception>
	bool SetValueAt(int index, TValue value, out TKey key);

	/// <summary>
	/// Removes the entry at the specified index from the <see cref="IIndexedDictionary{TKey,TValue}"/> items.
	/// </summary>
	/// <param name="index">The zero-based index of the entry to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
	/// -or-<br/>
	/// index is equal to or greater than <see cref="ICollection{T}.Count"/>.</exception>
	void RemoveAt(int index);
}