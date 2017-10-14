using Open.Threading;
using System.Collections.Generic;

namespace Open.Collections
{
	// Why?  Because it can be lightning fast.  Even faster than ConcurrentBag.

	public sealed class ConcurrentLinkedList<T> : ConcurrentCollectionBase<T, LinkedList<T>>
	{
		public ConcurrentLinkedList() : base(new LinkedList<T>()) { }
		public ConcurrentLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection)) { }

		public LinkedListNode<T> First => Sync.ReadValue(() => InternalSource.First);
		public LinkedListNode<T> Last => Sync.ReadValue(() => InternalSource.Last);

		public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
		{
			return Sync.WriteValue(() => InternalSource.AddAfter(node, value));
		}

		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddAfter(node, newNode));
		}

		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
		{
			return Sync.WriteValue(() => InternalSource.AddBefore(node, value));
		}

		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddBefore(node, newNode));
		}

		public LinkedListNode<T> AddFirst(T value)
		{
			return Sync.WriteValue(() => InternalSource.AddFirst(value));
		}

		public void AddFirst(LinkedListNode<T> newNode)
		{
			Sync.Write(() => InternalSource.AddFirst(newNode));
		}

		public LinkedListNode<T> AddLast(T value)
		{
			return Sync.WriteValue(() => InternalSource.AddLast(value));
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

		public bool TryTakeFirst(out T value)
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
			value = result;
			return success;
		}

		public bool TryTakeLast(out T value)
		{
			bool success = false;
			LinkedListNode<T> node = null;
			T result = default(T);
			Sync.ReadWriteConditionalOptimized(
				lockType => (node = InternalSource.Last) !=null,
				() =>
				{
					result = node.Value;
					InternalSource.RemoveLast();
					success = true;
				});
			value = result;
			return success;
		}


	}
}
