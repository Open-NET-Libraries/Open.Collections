﻿using Open.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Open.Collections.Synchronized;

/// <summary>
/// Buffers additions to the list using a <see cref="ConcurrentQueue{T}"/>
/// and defers synchronization until needed.
/// </summary>
public class ConcurrentList<T> : ListWrapper<T>, ISynchronizedCollection<T>
{
    int _count;
    public override int Count => _count;
    private readonly ConcurrentQueue<T> _buffer = new();
    private readonly ReaderWriterLockSlim _sync = new();
    private ReaderWriterLockSlim RWLock
        => AssertIsAlive() ? _sync : null!;

    protected override void OnDispose()
    {
        Clear();
        RWLock.Dispose();
        base.OnDispose();
    }

    private void DumpBuffer()
    {
        if (_buffer.IsEmpty) return;
        using var write = RWLock.WriteLock();
        DumpBufferUnlocked();
    }

    private void DumpBufferUnlocked()
    {
        Debug.Assert(RWLock.IsWriteLockHeld);
        while (_buffer.TryDequeue(out var item))
            InternalSource.Add(item);
    }

    public ConcurrentList() : base(new List<T>())
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertValidIndex(int index)
    {
        if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index), index, "Must be greater than zero and less than the collection.");
    }

    /// <inheritdoc />
    public override T this[int index]
    {
        get
        {
            DumpBuffer();
            return InternalSource[index];
        }
        set
        {
            DumpBuffer();
            InternalSource[index] = value;
        }
    }

    protected override void AddInternal(T item)
    {
        _buffer.Enqueue(item);
        Interlocked.Increment(ref _count);
    }

    /// <inheritdoc />
    public override int IndexOf(T item)
    {
        int i = RWLock.Read(() => base.IndexOf(item));
        if (i != -1 || _buffer.IsEmpty) return i;
        DumpBuffer(); // one dump then accept results.
        return RWLock.Read(() => base.IndexOf(item));
    }

    /// <inheritdoc />
    public override void Insert(int index, T item)
    {
        AssertValidIndex(index);
        DumpBuffer();
        using var write = RWLock.WriteLock();
        base.Insert(index, item);
        Interlocked.Increment(ref _count);
    }

    /// <inheritdoc />
    public override void RemoveAt(int index)
    {
        AssertValidIndex(index);
        DumpBuffer();
        RemoveAtCore(index);
    }

    private void RemoveAtCore(int index)
    {
        using var write = RWLock.WriteLock();
        base.RemoveAt(index);
        Interlocked.Decrement(ref _count);
    }

    /// <inheritdoc />
    public override bool Remove(T item)
        // Assume the majority case is that the item exists.
        => RWLock.ReadUpgradeable(() =>
        {
            DumpBuffer();
            int i = base.IndexOf(item);
            if (i != -1) return false;
            RemoveAtCore(i);
            return true;
        });

    /// <inheritdoc />
    public override void Clear()
        => RWLock.Write(() =>
        {
            DumpBufferUnlocked();
            int i = InternalSource.Count;
            base.Clear();
            while (0 < i--) Interlocked.Decrement(ref _count);
        });

    /// <inheritdoc />
    public override bool Contains(T item)
        => IndexOf(item)!=-1;

    /// <inheritdoc />
    public override void CopyTo(T[] array, int arrayIndex)
    {
        DumpBuffer();
        RWLock.Read(() => base.CopyTo(array, arrayIndex));
    }

    /// <inheritdoc />
    public override IEnumerator<T> GetEnumerator()
    {
        DumpBuffer();
        return base.GetEnumerator();
    }

    /// <inheritdoc />
    public T[] Snapshot()
    {
        DumpBuffer();
        return RWLock.Read(()=>InternalSource.ToArray());
    }
}