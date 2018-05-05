using Open.Threading;
using System;
using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public sealed class ReadWriteSynchronizedLinkedList<T> : ReadWriteSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>, ISynchronizedCollection<T>, IDisposable
	{
		public ReadWriteSynchronizedLinkedList() : base(new LinkedList<T>()) { }
		public ReadWriteSynchronizedLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection)) { }

		public LinkedListNode<T> First => InternalSource.First;

		public LinkedListNode<T> Last => InternalSource.Last;

		public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
		{
			return Sync.WriteValue(() => InternalSource.AddAfter(node, item));
		}

		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddAfter(node, newNode));
		}

		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
		{
			return Sync.WriteValue(() => InternalSource.AddBefore(node, item));
		}

		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddBefore(node, newNode));
		}

		public LinkedListNode<T> AddFirst(T item)
		{
			return Sync.WriteValue(() => InternalSource.AddFirst(item));
		}

		public void AddFirst(LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddFirst(newNode));
		}

		public LinkedListNode<T> AddLast(T item)
		{
			return Sync.WriteValue(() => InternalSource.AddLast(item));
		}

		public void AddLast(LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddLast(newNode));
		}

		public void Remove(LinkedListNode<T> node)
		{
			Sync.Write(() => InternalSource.Remove(node));
		}

		public void RemoveFirst()
		{
			Sync.Write(() => InternalSource.RemoveFirst());
		}

		public void RemoveLast()
		{
			Sync.Write(() => InternalSource.RemoveLast());
		}

		public bool TryTakeFirst(out T item)
		{
			bool success = false;
			LinkedListNode<T> node = null;
			T result = default(T);
			Sync.ReadWriteConditionalOptimized(
				lockType => (node = InternalSource.First) != null,
				() =>
				{
					result = node.Value;
					InternalSource.RemoveFirst();
					success = true;
				});
			item = result;
			return success;
		}

		public bool TryTakeLast(out T item)
		{
			bool success = false;
			LinkedListNode<T> node = null;
			T result = default(T);
			Sync.ReadWriteConditionalOptimized(
				lockType => (node = InternalSource.Last) != null,
				() =>
				{
					result = node.Value;
					InternalSource.RemoveLast();
					success = true;
				});
			item = result;
			return success;
		}

	}
}
