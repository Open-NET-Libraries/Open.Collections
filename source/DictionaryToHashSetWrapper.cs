using System;
using System.Collections;
using System.Collections.Generic;

namespace Open.Collections;

public class DictionaryToHashSetWrapper<T> : ISet<T>
{
	protected readonly IDictionary<T, bool> InternalSource;

	// ReSharper disable once MemberCanBeProtected.Global
	public DictionaryToHashSetWrapper(IDictionary<T, bool> source) => InternalSource = source;

	/// <inheritdoc />
	public int Count
		=> InternalSource.Count;

	/// <inheritdoc />
	public bool IsReadOnly
		=> InternalSource.IsReadOnly;

	/// <inheritdoc />
	public virtual bool Add(T item)
	{
		if (InternalSource.ContainsKey(item))
			return false;

		try
		{
			InternalSource.Add(item, true);
		}
		catch
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public bool Remove(T item) => InternalSource.Remove(item);

	/// <inheritdoc />
	public void Clear() => InternalSource.Clear();

	/// <inheritdoc />
	public bool Contains(T item) => InternalSource.ContainsKey(item);

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex) => InternalSource.Keys.CopyTo(array, arrayIndex);

	/// <inheritdoc cref="ReadOnlyCollectionWrapper{T, TCollection}.CopyTo(Span{T})"/>
	public virtual Span<T> CopyTo(Span<T> span) => InternalSource.Keys.CopyToSpan(span);

	/// <summary>
	/// Returns a copy of the underlying keys.
	/// </summary>
	public HashSet<T> ToHashSet() => new(InternalSource.Keys);

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() => InternalSource.Keys.GetEnumerator();

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<T> other)
	{
		foreach (T? e in other) Remove(e);
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<T> other)
	{
		foreach (T? e in other)
		{
			if (!InternalSource.ContainsKey(e))
				Remove(e);
		}
	}

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<T> other) => ToHashSet().IsProperSubsetOf(other);

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<T> other) => ToHashSet().IsProperSupersetOf(other);

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<T> other) => ToHashSet().IsSubsetOf(other);

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<T> other) => ToHashSet().IsSupersetOf(other);

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<T> other) => ToHashSet().Overlaps(other);

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<T> other) => ToHashSet().SetEquals(other);

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		foreach (T? e in other)
		{
			if (InternalSource.ContainsKey(e))
				Add(e);
			else
				Remove(e);
		}
	}

	/// <inheritdoc />
	public void UnionWith(IEnumerable<T> other)
	{
		foreach (T? e in other) Add(e);
	}

	void ICollection<T>.Add(T item) => Add(item);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
