using System.Collections.Generic;

namespace Open.Collections
{
	public interface ILinkedList<T> : ICollection<T>
	{
		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> First { get; }

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> Last { get; }

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> AddFirst(T item);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void AddFirst(LinkedListNode<T> newNode);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		LinkedListNode<T> AddLast(T item);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void AddLast(LinkedListNode<T> newNode);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void Remove(LinkedListNode<T> node);

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
		void RemoveFirst();

		/// <inheritdoc cref="LinkedList&lt;T&gt;"/>
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
}
