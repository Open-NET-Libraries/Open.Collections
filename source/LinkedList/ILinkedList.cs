using System.Collections.Generic;

namespace Open.Collections;

public interface ILinkedList<T> : ICollection<T>
{
	/// <inheritdoc cref="LinkedList&lt;T&gt;.First"/>
	LinkedListNode<T> First { get; }

	/// <inheritdoc cref="LinkedList&lt;T&gt;.Last"/>
	LinkedListNode<T> Last { get; }

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddAfter(LinkedListNode{T}, T)"/>
	LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddAfter(LinkedListNode{T}, LinkedListNode{T})"/>
	void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddBefore(LinkedListNode{T}, T)"/>
	LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddBefore(LinkedListNode{T}, LinkedListNode{T})"/>
	void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddFirst(T)"/>
	LinkedListNode<T> AddFirst(T item);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddFirst(LinkedListNode{T})"/>
	void AddFirst(LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddLast(T)"/>
	LinkedListNode<T> AddLast(T item);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.AddLast(LinkedListNode{T})"/>
	void AddLast(LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.Remove(LinkedListNode{T})"/>
	void Remove(LinkedListNode<T> node);

	/// <inheritdoc cref="LinkedList&lt;T&gt;.RemoveFirst()"/>
	void RemoveFirst();

	/// <inheritdoc cref="LinkedList&lt;T&gt;.RemoveLast()"/>
	void RemoveLast();

	/// <summary>
	/// Attempts to remove the first item.
	/// </summary>
	/// <param name="item">The item removed.</param>
	/// <returns>True if the first item was removed successfully.</returns>
	bool TryTakeFirst(out T item);

	/// <summary>
	/// Attempts to remove the last item.
	/// </summary>
	/// <param name="item">The item removed.</param>
	/// <returns>True if the last item was removed successfully.</returns>
	bool TryTakeLast(out T item);
}
