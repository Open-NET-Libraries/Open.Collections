using Open.Threading;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized wrapper for <see cref="LinkedList{T}"/> that uses a <see cref="System.Threading.ReaderWriterLockSlim"/> for thread safety.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ReadWriteSynchronizedLinkedList<T>
	: ReadWriteSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>
{
	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedLinkedList()
		: base([]) { }

	/// <summary>
	/// Constructs a new instance with the specified collection.
	/// </summary>
	/// <param name="collection"></param>
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedLinkedList(IEnumerable<T> collection)
		: base(new LinkedList<T>(collection)) { }

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T>? First
		=> InternalSource.First;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T>? Last
		=> InternalSource.Last;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
		=> RWLock.Write(() => InternalUnsafeSource!.AddAfter(node, item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		=> RWLock.Write(() => InternalUnsafeSource!.AddAfter(node, newNode));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
		=> RWLock.Write(() => InternalUnsafeSource!.AddBefore(node, item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		=> RWLock.Write(() => InternalUnsafeSource!.AddBefore(node, newNode));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T> AddFirst(T item)
		=> RWLock.Write(() => InternalUnsafeSource!.AddFirst(item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void AddFirst(LinkedListNode<T> newNode)
		=> RWLock.Write(() => InternalUnsafeSource!.AddFirst(newNode));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public LinkedListNode<T> AddLast(T item)
		=> RWLock.Write(() => InternalUnsafeSource!.AddLast(item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void AddLast(LinkedListNode<T> newNode)
		=> RWLock.Write(() => InternalUnsafeSource!.AddLast(newNode));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void Remove(LinkedListNode<T> node)
		=> RWLock.Write(() => InternalUnsafeSource!.Remove(node));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void RemoveFirst() => RWLock.Write(()
		=> InternalSource.RemoveFirst());

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void RemoveLast() => RWLock.Write(()
		=> InternalSource.RemoveLast());

	/// <inheritdoc />
	public bool TryTakeFirst(out T item)
	{
		LinkedListNode<T>? node = null;
		T result = default!;
		bool success = RWLock.ReadWriteConditional(
			_ => (node = InternalSource.First) is not null,
			() =>
			{
				result = node!.Value;
				InternalUnsafeSource!.RemoveFirst();
			});
		item = result;
		return success;
	}

	/// <inheritdoc />
	public bool TryTakeLast(out T item)
	{
		LinkedListNode<T>? node = null;
		T result = default!;
		bool success = RWLock.ReadWriteConditional(
			_ => (node = InternalSource.Last) is not null,
			() =>
			{
				result = node!.Value;
				InternalUnsafeSource!.RemoveLast();
			});
		item = result;
		return success;
	}
}
