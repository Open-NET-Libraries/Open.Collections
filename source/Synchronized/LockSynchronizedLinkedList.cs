using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized
{
	// LinkedLists are a bit different and don't have an default interface.
	// Overriding the .Value property of the nodes is beyond the scope of this.  All that's needed is to synchronize the collection.
	public sealed class LockSynchronizedLinkedList<T> : LockSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>
	{
		public LockSynchronizedLinkedList() : base(new LinkedList<T>()) { }
		public LockSynchronizedLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection)) { }

		/// <inheritdoc />
		public LinkedListNode<T> First
			=> InternalSource.First;

		/// <inheritdoc />
		public LinkedListNode<T> Last
			=> InternalSource.Last;

		/// <inheritdoc />
		public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
		{
			lock (Sync) return InternalSource.AddAfter(node, item);
		}

		/// <inheritdoc />
		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			lock (Sync) InternalSource.AddAfter(node, newNode);
		}

		/// <inheritdoc />
		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
		{
			lock (Sync) return InternalSource.AddBefore(node, item);
		}

		/// <inheritdoc />
		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			lock (Sync) InternalSource.AddBefore(node, newNode);
		}

		/// <inheritdoc />
		public LinkedListNode<T> AddFirst(T item)
		{
			lock (Sync) return InternalSource.AddFirst(item);
		}

		/// <inheritdoc />
		public void AddFirst(LinkedListNode<T> newNode)
		{
			lock (Sync) InternalSource.AddFirst(newNode);
		}

		/// <inheritdoc />
		public LinkedListNode<T> AddLast(T item)
		{
			lock (Sync) return InternalSource.AddLast(item);
		}

		/// <inheritdoc />
		public void AddLast(LinkedListNode<T> newNode)
		{
			lock (Sync) InternalSource.AddLast(newNode);
		}

		/// <inheritdoc />
		public void Remove(LinkedListNode<T> node)
		{
			lock (Sync) InternalSource.Remove(node);
		}

		/// <inheritdoc />
		public void RemoveFirst()
		{
			lock (Sync) InternalSource.RemoveFirst();
		}

		/// <inheritdoc />
		public void RemoveLast()
		{
			lock (Sync) InternalSource.RemoveLast();
		}

		/// <inheritdoc />
		public bool TryTakeFirst(out T item)
		{
			var success = false;
			LinkedListNode<T>? node = null;
			T result = default!;
			ThreadSafety.LockConditional(
				Sync,
				() => (node = InternalSource.First) != null,
				() =>
				{
					result = node!.Value;
					InternalSource.RemoveFirst();
					success = true;
				});
			item = result;
			return success;
		}

		/// <inheritdoc />
		public bool TryTakeLast(out T item)
		{
			var success = false;
			LinkedListNode<T>? node = null;
			T result = default!;
			ThreadSafety.LockConditional(
				Sync,
				() => (node = InternalSource.Last) != null,
				() =>
				{
					result = node!.Value;
					InternalSource.RemoveLast();
					success = true;
				});
			item = result!;
			return success;
		}

	}
}
