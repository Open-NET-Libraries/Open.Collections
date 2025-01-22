using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

[method: ExcludeFromCodeCoverage]
public class DictionaryToHashSetWrapper<T>(
	IDictionary<T, bool> source)
	: ISet<T>
{
	protected readonly IDictionary<T, bool> InternalSource = source;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public int Count
		=> InternalSource.Count;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsReadOnly
		=> InternalSource.IsReadOnly;

	/// <inheritdoc />
	public virtual bool Add(T item)
	{
		var source = InternalSource;
		if (source.ContainsKey(item))
			return false;

		try
		{
			source.Add(item, true);
		}
		catch
		{
			return false;
		}

		return true;
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool Remove(T item) => InternalSource.Remove(item);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void Clear() => InternalSource.Clear();

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool Contains(T item) => InternalSource.ContainsKey(item);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void CopyTo(T[] array, int arrayIndex) => InternalSource.Keys.CopyTo(array, arrayIndex);

	/// <inheritdoc cref="ReadOnlyCollectionWrapper{T, TCollection}.CopyTo(Span{T})"/>
	[ExcludeFromCodeCoverage]
	public virtual Span<T> CopyTo(Span<T> span) => InternalSource.Keys.CopyToSpan(span);

	/// <summary>
	/// Returns a copy of the underlying keys.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public HashSet<T> ToHashSet() => new(InternalSource.Keys);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public IEnumerator<T> GetEnumerator()
		=> InternalSource.Keys.GetEnumerator();

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<T> other)
	{
		foreach (T? e in other) Remove(e);
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<T> other)
	{
		var source = InternalSource;
		foreach (T? e in other)
		{
			if (!source.ContainsKey(e))
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
		var source = InternalSource;
		foreach (T? e in other)
		{
			if (source.ContainsKey(e))
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

	[ExcludeFromCodeCoverage]
	void ICollection<T>.Add(T item) => Add(item);

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
