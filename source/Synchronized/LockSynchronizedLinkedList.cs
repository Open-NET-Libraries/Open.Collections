using Open.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

// LinkedLists are a bit different and don't have an default interface.
// Overriding the .Value property of the nodes is beyond the scope of this.  All that's needed is to synchronize the collection.
public sealed class LockSynchronizedLinkedList<T> : LockSynchronizedCollectionWrapper<T, LinkedList<T>>, ILinkedList<T>
{
    public LockSynchronizedLinkedList() : base(new LinkedList<T>()) { }

    [ExcludeFromCodeCoverage]
    public LockSynchronizedLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection)) { }

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
    {
        lock (Sync) return InternalSource.AddAfter(node, item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        lock (Sync) InternalSource.AddAfter(node, newNode);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T item)
    {
        lock (Sync) return InternalSource.AddBefore(node, item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        lock (Sync) InternalSource.AddBefore(node, newNode);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddFirst(T item)
    {
        lock (Sync) return InternalSource.AddFirst(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddFirst(LinkedListNode<T> newNode)
    {
        lock (Sync) InternalSource.AddFirst(newNode);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public LinkedListNode<T> AddLast(T item)
    {
        lock (Sync) return InternalSource.AddLast(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void AddLast(LinkedListNode<T> newNode)
    {
        lock (Sync) InternalSource.AddLast(newNode);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Remove(LinkedListNode<T> node)
    {
        lock (Sync) InternalSource.Remove(node);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void RemoveFirst()
    {
        lock (Sync) InternalSource.RemoveFirst();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void RemoveLast()
    {
        lock (Sync) InternalSource.RemoveLast();
    }

    /// <inheritdoc />
    public bool TryTakeFirst(out T item)
    {
        LinkedListNode<T>? node = null;
        T result = default!;
        bool success = ThreadSafety.LockConditional(
            Sync,
            () => (node = InternalSource.First) is not null,
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
        bool success = ThreadSafety.LockConditional(
            Sync,
            () => (node = InternalSource.Last) is not null,
            () =>
            {
                result = node!.Value;
                InternalSource.RemoveLast();
            });
        item = result!;
        return success;
    }
}
