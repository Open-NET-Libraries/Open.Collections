using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

/// <summary>
/// A wrapper for <see cref="IList{T}"/> that allows for easy extension.
/// </summary>
public class ListWrapper<T, TList>(
	TList source, bool owner = false)
	: CollectionWrapper<T, TList>(source, owner), IList<T>
	where TList : class, IList<T>
{
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual T this[int index]
	{
		get => InternalSource[index];
		set => InternalSource[index] = value;
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual int IndexOf(T item)
		=> InternalSource.IndexOf(item);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Insert(int index, T item)
		=> InternalSource.Insert(index, item);

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void RemoveAt(int index)
		=> InternalSource.RemoveAt(index);
}

/// <summary>
/// A wrapper for <see cref="IList{T}"/> that allows for easy extension.
/// </summary>
[ExcludeFromCodeCoverage]
public class ListWrapper<T>
	: ListWrapper<T, IList<T>>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ListWrapper{T}"/> class.
	/// </summary>
	public ListWrapper(IList<T> source, bool owner = false)
		: base(source, owner)
	{
	}

	/// <inheritdoc cref="ListWrapper{T}.ListWrapper(IList{T}, bool)"/>
	public ListWrapper(int capacity = 0)
		: base(new List<T>(capacity))
	{
	}
}