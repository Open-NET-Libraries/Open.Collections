using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
    public interface ILinkedList<T> : ICollection<T>
    {
		LinkedListNode<T> First { get; }
		LinkedListNode<T> Last { get; }

		LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item);

		void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode);

		LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item);

		void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode);

		LinkedListNode<T> AddFirst(T item);

		void AddFirst(LinkedListNode<T> newNode);

		LinkedListNode<T> AddLast(T item);

		void AddLast(LinkedListNode<T> newNode);

		void Remove(LinkedListNode<T> node);

		void RemoveFirst();

		void RemoveLast();
	}
}
