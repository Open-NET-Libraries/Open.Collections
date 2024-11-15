using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

///<summary>
/// A base class for wrapping a collection as a dictionary.
///</summary>
[ExcludeFromCodeCoverage]
public abstract class DictionaryWrapperBase<TKey, TValue, TCollection>(
	TCollection source, bool owner = false)
	: CollectionWrapper<KeyValuePair<TKey, TValue>, TCollection>(source, owner), IDictionary<TKey, TValue>
	where TKey : notnull
	where TCollection : class, ICollection<KeyValuePair<TKey, TValue>>
{
	/// <inheritdoc />
	public TValue this[TKey key]
	{
		get => GetValueInternal(key);
		set => SetValueInternal(key, value);
	}

	/// <summary>
	/// Get the value for the key.
	/// </summary>
	protected abstract TValue GetValueInternal(TKey key);

	/// <summary>
	/// Set the value for the key.
	/// </summary>
	protected abstract void SetValueInternal(TKey key, TValue value);

	ICollection<TKey>? _keys;
	/// <inheritdoc />
	public ICollection<TKey> Keys => _keys ??= GetKeys();

	/// <summary>
	/// Get the keys.
	/// </summary>
	protected abstract ICollection<TKey> GetKeys();

	ICollection<TValue>? _values;

	/// <inheritdoc />
	public ICollection<TValue> Values => _values ??= GetValues();

	/// <summary>
	/// Get the values.
	/// </summary>
	protected abstract ICollection<TValue> GetValues();

	/// <summary>
	/// Add a key and value to the dictionary.
	/// </summary>
	protected abstract void AddInternal(TKey key, TValue value);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(TKey key, TValue value)
		=> AddInternal(key, value);

	/// <inheritdoc />
	public abstract bool ContainsKey(TKey key);

	/// <inheritdoc />
	public abstract bool Remove(TKey key);

	/// <inheritdoc />
	public abstract bool TryGetValue(TKey key,
#if NET9_0_OR_GREATER
		[MaybeNullWhen(false)]
#else
#endif
		out TValue value);
}
