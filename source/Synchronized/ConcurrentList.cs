using Open.Threading;
using System.Collections.Concurrent;

namespace Open.Collections.Synchronized;

/// <summary>
/// Buffers additions to the list using a <see cref="ConcurrentQueue{T}"/>
/// and defers synchronization until needed.
/// </summary>
public sealed class ConcurrentList<T> : ListWrapper<T, List<T>>, ISynchronizedCollection<T>
{
	int _count;

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	protected override int GetCount()
	{
		AssertIsAlive();
		return _count;
	}

	private readonly Queue.Concurrent<T> _buffer = new();

	/// <summary>
	/// The lock used to synchronize access to <c>InternalSource</c>.
	/// </summary>
	/// <remarks>
	/// Requires <see cref="LockRecursionPolicy.SupportsRecursion"/>: <see cref="Read(Action)"/> holds a
	/// read lock across the caller's delegate, which may recursively call <see cref="Read(Action)"/>
	/// again on the same thread. Other read-only members never re-enter this lock while it is already
	/// held on the calling thread; see <see cref="EnumerateReentrant"/>.
	/// </remarks>
	private readonly ReaderWriterLockSlim RWLock = new(LockRecursionPolicy.SupportsRecursion);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	protected override void OnDispose()
	{
		_count = 0;
		_buffer.Clear();
		RWLock.Dispose();
		base.OnDispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DumpBuffer()
	{
		if (_buffer.IsEmpty) return;
		using var write = RWLock.WriteLock();
		DumpBufferUnlocked();
	}

	private void DumpBufferUnlocked()
	{
		Debug.Assert(RWLock.IsWriteLockHeld);
		var list = Grow();
		while (_buffer.TryDequeue(out var item))
			list.Add(item);
	}

	private const int HalfMaxInt = int.MaxValue / 2;
	private List<T> Grow()
	{
		var list = InternalSource;
		int capacity = list.Capacity;
		if (capacity > _count) return list;
		if (capacity == 0) capacity = 4;
		while (capacity < _count)
		{
			if (capacity > HalfMaxInt)
			{
				capacity = int.MaxValue;
				break;
			}

			capacity *= 2;
		}

		list.Capacity = capacity;
		return list;
	}

	/// <summary>
	/// Gets or sets the capacity of the list.
	/// </summary>
	public int Capacity
	{
		get
		{
			using var read = RWLock.ReadLock();
			return InternalSource.Capacity;
		}
		set
		{
			using var write = RWLock.WriteLock();
			InternalSource.Capacity = value;
		}
	}

	/// <summary>
	/// Constructs a new instance with the specified capacity.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public ConcurrentList(int capacity) : base(new List<T>(capacity)) { }

	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public ConcurrentList() : base([]) { }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AssertValidIndex(int index)
	{
		if (index < 0 || index > _count) throw new ArgumentOutOfRangeException(nameof(index), index, "Must be greater than zero and less than the collection.");
	}

	/// <summary>
	/// Enumerates <c>InternalSource</c> followed by the buffered tail, in logical order.
	/// </summary>
	/// <remarks>
	/// Only valid while this thread already holds the read lock: that guarantees no writer can
	/// run concurrently, so <c>InternalSource</c> is stable and the buffer can only grow for the
	/// duration of the enumeration.
	/// </remarks>
	private IEnumerable<T> EnumerateReentrant()
	{
		foreach (var item in InternalSource) yield return item;
		foreach (var item in _buffer) yield return item;
	}

	/// <summary>
	/// Throws if this thread is currently inside a <see cref="Read(Action)"/> or
	/// <see cref="Read{TResult}(Func{TResult})"/> delegate.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// This thread already holds the read lock, so mutating would require an illegal read-to-write
	/// upgrade.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AssertNotReentrant()
	{
		if (RWLock.IsReadLockHeld) throw new InvalidOperationException(
			$"Cannot mutate a {nameof(ConcurrentList<T>)} from within a {nameof(Read)}(...) delegate running on the same thread.");
	}

	/// <inheritdoc />
	/// <remarks>
	/// Locked, unlike the deliberately unsynchronized <see cref="LockSynchronizedListWrapper{T,TList}.this[int]"/>
	/// and <see cref="ReadWriteSynchronizedListWrapper{T,TList}.this[int]"/>. Those disclaim synchronization
	/// in a comment; this type does not, and already drains the buffer here.
	/// A reentrant read walks the undrained buffer per index; prefer <see cref="Snapshot"/> for
	/// index-heavy reentrant access.
	/// </remarks>
	public override T this[int index]
	{
		get
		{
			if (!RWLock.IsReadLockHeld)
			{
				DumpBuffer();
				using var read = RWLock.ReadLock();
				return InternalSource[index];
			}

			// Called reentrantly from within Read(): DumpBuffer() would need to upgrade to a
			// write lock, which is illegal while this thread already holds a read lock. The held
			// read lock already blocks all writers, so InternalSource is stable and the buffer
			// can only grow; read straight through both, in logical order.
			var source = InternalSource;
			int drained = source.Count;
			if (index < drained) return source[index];

			int remaining = index - drained;
			foreach (var item in _buffer)
			{
				if (remaining == 0) return item;
				remaining--;
			}

			throw new ArgumentOutOfRangeException(nameof(index), index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		set
		{
			AssertNotReentrant();
			DumpBuffer();
			using var write = RWLock.WriteLock();
			InternalSource[index] = value;
		}
	}

	/// <inheritdoc />
	protected override void AddInternal(in T item)
	{
		_buffer.Enqueue(item);
		Interlocked.Increment(ref _count);
	}

	/// <inheritdoc />
	public override int IndexOf(T item)
	{
		if (!RWLock.IsReadLockHeld)
		{
			int i;
			using (RWLock.ReadLock()) i = base.IndexOf(item);
			if (i != -1 || _buffer.IsEmpty) return i;
			DumpBuffer(); // one dump then accept results.
			using var read = RWLock.ReadLock();
			return base.IndexOf(item);
		}

		// Reentrant: search the drained portion, then fall through to the buffered tail.
		int idx = base.IndexOf(item);
		if (idx != -1) return idx;

		int offset = InternalSource.Count;
		var comparer = EqualityComparer<T>.Default;
		foreach (var candidate in _buffer)
		{
			if (comparer.Equals(candidate, item)) return offset;
			offset++;
		}

		return -1;
	}

	/// <inheritdoc />
	public override void Insert(int index, T item)
	{
		AssertNotReentrant();
		AssertValidIndex(index);
		DumpBuffer();
		using var write = RWLock.WriteLock();
		base.Insert(index, item);
		Interlocked.Increment(ref _count);
	}

	/// <inheritdoc />
	public override void RemoveAt(int index)
	{
		AssertNotReentrant();
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
	{
		AssertNotReentrant();
		// Assume the majority case is that the item exists.
		using var upgradable = RWLock.UpgradableReadLock();
		DumpBuffer();
		int i = base.IndexOf(item);
		if (i == -1) return false;
		RemoveAtCore(i);
		return true;
	}

	/// <inheritdoc />
	public override void Clear()
	{
		AssertNotReentrant();
		using var write = RWLock.WriteLock();
		DumpBufferUnlocked();
		int i = InternalSource.Count;
		base.Clear();
		Interlocked.Add(ref _count, -i);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override bool Contains(T item)
		=> IndexOf(item) != -1;

	/// <inheritdoc />
	public override void CopyTo(T[] array, int arrayIndex)
	{
		if (!RWLock.IsReadLockHeld)
		{
			DumpBuffer();
			using var read = RWLock.ReadLock();
			base.CopyTo(array, arrayIndex);
			return;
		}

		// Reentrant: copy the drained portion, then the buffered tail, in logical order.
		var source = InternalSource;
		if (array.Length - arrayIndex < source.Count + _buffer.Count)
			throw new ArgumentException("Destination array is not long enough.", nameof(array));

		source.CopyTo(array, arrayIndex);
		int i = arrayIndex + source.Count;
		foreach (var item in _buffer)
			array[i++] = item;
	}

	/// <inheritdoc />
	/// <remarks>
	/// The base <see cref="ReadOnlyCollectionWrapper{T,TCollection}.Export(ICollection{T})"/> implementation
	/// neither drains the buffer nor takes a lock, so without this override buffered-but-undrained items
	/// would be silently omitted from <paramref name="to"/>.
	/// </remarks>
	public override void Export(ICollection<T> to)
	{
		if (!RWLock.IsReadLockHeld)
		{
			DumpBuffer();
			using var read = RWLock.ReadLock();
			to.AddRange(InternalSource);
			return;
		}

		// Reentrant: the drained portion, then the buffered tail, in logical order.
		to.AddRange(EnumerateReentrant());
	}

	/// <inheritdoc />
	/// <remarks>
	/// The base <see cref="ReadOnlyCollectionWrapper{T,TCollection}.CopyTo(Span{T})"/> implementation
	/// neither drains the buffer nor takes a lock, so without this override buffered-but-undrained items
	/// would be silently omitted from the copy.
	/// </remarks>
	public override Span<T> CopyTo(Span<T> span)
	{
		if (!RWLock.IsReadLockHeld)
		{
			DumpBuffer();
			using var read = RWLock.ReadLock();
			return base.CopyTo(span);
		}

		// Reentrant: the drained portion, then the buffered tail, in logical order.
		return EnumerateReentrant().CopyToSpan(span);
	}

	/// <inheritdoc />
	/// <remarks>
	/// Drains the buffer, but does not hold a lock for the enumeration; a concurrent structural change
	/// surfaces as <see cref="InvalidOperationException"/>, as it would for <see cref="List{T}"/>.
	/// Use <see cref="Snapshot"/> for a stable view.
	/// Unlike the other read members this does not support reentrant use: called from within
	/// <see cref="Read(Action)"/> with a non-empty buffer it throws <see cref="LockRecursionException"/>.
	/// </remarks>
	[ExcludeFromCodeCoverage]
	public override IEnumerator<T> GetEnumerator()
	{
		DumpBuffer();
		return base.GetEnumerator().Preflight(ThrowIfDisposedDelegate);
	}

	/// <inheritdoc />
	public T[] Snapshot()
	{
		if (!RWLock.IsReadLockHeld)
		{
			DumpBuffer();
			using var read = RWLock.ReadLock();
			return InternalSource.ToArray();
		}

		// Reentrant: the drained portion, then the buffered tail, in logical order.
		return EnumerateReentrant().ToArray();
	}

	/// <inheritdoc />
	public void Read(Action action)
	{
		// If this thread is already inside a Read(...) call, DumpBuffer() would need to
		// upgrade to a write lock, which is illegal while holding a read lock. Skipping it here
		// is safe: any buffered items are still visible to callers via the tail fallback in the
		// other read-only members (see EnumerateReentrant).
		if (!RWLock.IsReadLockHeld) DumpBuffer();
		using var read = RWLock.ReadLock();
		action();
	}

	/// <inheritdoc />
	public TResult Read<TResult>(Func<TResult> action)
	{
		if (!RWLock.IsReadLockHeld) DumpBuffer();
		using var read = RWLock.ReadLock();
		return action();
	}
}
