using System.Collections.Generic;

namespace Open.Collections;

/// <summary>
/// An interface for a linked list.
/// </summary>
public interface ILinkedList<T> : ICollection<T>
{
	/// <inheritdoc cref="LinkedList{T}.First"/>
	LinkedListNode<T>? First { get; }

	/// <inheritdoc cref="LinkedList{T}.Last"/>
	LinkedListNode<T>? Last { get; }

	/// <inheritdoc cref="LinkedList{T}.AddAfter(LinkedListNode{T}, T)"/>
	LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item);

	/// <inheritdoc cref="LinkedList{T}.AddAfter(LinkedListNode{T}, LinkedListNode{T})"/>
	void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList{T}.AddBefore(LinkedListNode{T}, T)"/>
	LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item);

	/// <inheritdoc cref="LinkedList{T}.AddBefore(LinkedListNode{T}, LinkedListNode{T})"/>
	void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList{T}.AddFirst(T)"/>
	LinkedListNode<T> AddFirst(T item);

	/// <inheritdoc cref="LinkedList{T}.AddFirst(LinkedListNode{T})"/>
	void AddFirst(LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList{T}.AddLast(T)"/>
	LinkedListNode<T> AddLast(T item);

	/// <inheritdoc cref="LinkedList{T}.AddLast(LinkedListNode{T})"/>
	void AddLast(LinkedListNode<T> newNode);

	/// <inheritdoc cref="LinkedList{T}.Remove(LinkedListNode{T})"/>
	void Remove(LinkedListNode<T> node);

	/// <inheritdoc cref="LinkedList{T}.RemoveFirst()"/>
	void RemoveFirst();

	/// <inheritdoc cref="LinkedList{T}.RemoveLast()"/>
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
