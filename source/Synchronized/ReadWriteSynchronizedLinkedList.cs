using Open.Threading;
using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public sealed class ReadWriteSynchronizedLinkedList<T> : ReadWriteSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>
	{
		public ReadWriteSynchronizedLinkedList() : base(new LinkedList<T>()) { }
		public ReadWriteSynchronizedLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection)) { }

		/// <inheritdoc />
		public LinkedListNode<T> First
			=> InternalSource.First;

		/// <inheritdoc />
		public LinkedListNode<T> Last
			=> InternalSource.Last;

		/// <inheritdoc />
		public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
			=> Sync.WriteValue(() => InternalSource.AddAfter(node, item));

		/// <inheritdoc />
		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
			=> Sync.Write(() => InternalSource.AddAfter(node, newNode));

		/// <inheritdoc />
		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
			=> Sync.WriteValue(() => InternalSource.AddBefore(node, item));

		/// <inheritdoc />
		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
			=> Sync.Write(() => InternalSource.AddBefore(node, newNode));

		/// <inheritdoc />
		public LinkedListNode<T> AddFirst(T item)
			=> Sync.WriteValue(() => InternalSource.AddFirst(item));

		/// <inheritdoc />
		public void AddFirst(LinkedListNode<T> newNode)
			=> Sync.Write(() => InternalSource.AddFirst(newNode));

		/// <inheritdoc />
		public LinkedListNode<T> AddLast(T item)
			=> Sync.WriteValue(() => InternalSource.AddLast(item));

		/// <inheritdoc />
		public void AddLast(LinkedListNode<T> newNode)
			=> Sync.Write(() => InternalSource.AddLast(newNode));

		/// <inheritdoc />
		public void Remove(LinkedListNode<T> node)
			=> Sync.Write(() => InternalSource.Remove(node));

		/// <inheritdoc />
		public void RemoveFirst()
			=> Sync.Write(() => InternalSource.RemoveFirst());

		/// <inheritdoc />
		public void RemoveLast()
			=> Sync.Write(() => InternalSource.RemoveLast());

		/// <inheritdoc />
		public bool TryTakeFirst(out T item)
		{
			var success = false;
			LinkedListNode<T>? node = null;
			T result = default!;
			Sync.ReadWriteConditionalOptimized(
				lockType => (node = InternalSource.First) != null,
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
			Sync.ReadWriteConditionalOptimized(
				lockType => (node = InternalSource.Last) != null,
				() =>
				{
					result = node!.Value;
					InternalSource.RemoveLast();
					success = true;
				});
			item = result;
			return success;
		}

	}
}
