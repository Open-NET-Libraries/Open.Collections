using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

public sealed class ReadWriteSynchronizedLinkedList<T> : ReadWriteSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>
{
    [ExcludeFromCodeCoverage]
    public ReadWriteSynchronizedLinkedList()
        : base(new LinkedList<T>()) { }

    [ExcludeFromCodeCoverage]
    public ReadWriteSynchronizedLinkedList(IEnumerable<T> collection)
        : base(new LinkedList<T>(collection)) { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> First
        => InternalSource.First;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> Last
        => InternalSource.Last;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T item)
        => RWLock.Write(() => InternalSource.AddAfter(node, item));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        => RWLock.Write(() => InternalSource.AddAfter(node, newNode));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
        => RWLock.Write(() => InternalSource.AddBefore(node, item));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        => RWLock.Write(() => InternalSource.AddBefore(node, newNode));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddFirst(T item)
        => RWLock.Write(() => InternalSource.AddFirst(item));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddFirst(LinkedListNode<T> newNode)
        => RWLock.Write(() => InternalSource.AddFirst(newNode));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddLast(T item)
        => RWLock.Write(() => InternalSource.AddLast(item));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddLast(LinkedListNode<T> newNode)
        => RWLock.Write(() => InternalSource.AddLast(newNode));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Remove(LinkedListNode<T> node)
        => RWLock.Write(() => InternalSource.Remove(node));

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
				InternalSource.RemoveFirst();
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
				InternalSource.RemoveLast();
			});
		item = result;
		return success;
	}
}
