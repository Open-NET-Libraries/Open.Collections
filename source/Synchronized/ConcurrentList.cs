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
	/// Uses <see cref="LockRecursionPolicy.SupportsRecursion"/> (unlike a plain
	/// <see cref="ReadWriteSynchronizedCollectionWrapper{T,TCollection}"/>-style lock, this is required
	/// rather than merely convenient here) because <see cref="Read(Action)"/> and
	/// <see cref="Read{TResult}(Func{TResult})"/> hold a read lock for the entire duration of the
	/// caller-supplied delegate. That delegate is free to call back into this instance from the same
	/// thread &#8212; e.g. the indexer, <see cref="CopyTo(Span{T})"/>, or <see cref="Export(ICollection{T})"/>
	/// &#8212; and each of those acquires a nested read lock of its own. Recursive read &#8594; read
	/// acquisition on the same thread is legal only under <see cref="LockRecursionPolicy.SupportsRecursion"/>;
	/// under <see cref="LockRecursionPolicy.NoRecursion"/> the nested acquisition throws
	/// <see cref="LockRecursionException"/> even though both acquisitions are read-only. This alone does
	/// <b>not</b> make it safe to drain a <i>non-empty</i> buffer from within <see cref="Read(Action)"/>:
	/// doing that requires upgrading to a write lock, which a plain read lock can never do, under any
	/// recursion policy.
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

	/// <inheritdoc />
	/// <remarks>
	/// This indexer takes a lock, unlike the sibling <see cref="LockSynchronizedListWrapper{T,TList}.this[int]"/>
	/// and <see cref="ReadWriteSynchronizedListWrapper{T,TList}.this[int]"/>, which are deliberately left
	/// unlocked. Those two carry an explicit comment disclaiming full synchronization ("This is a
	/// simplified version ... If that fine grained of read-write control is necessary, then use the
	/// ThreadSafety utility and extensions.") and are marked <see cref="ExcludeFromCodeCoverageAttribute"/>
	/// to signal that intentional gap. <see cref="ConcurrentList{T}"/> carries no such disclaimer anywhere
	/// in its history, and this indexer already calls <c>DumpBuffer()</c> before touching
	/// <c>InternalSource</c> &#8212; i.e. the author's intent was full thread safety here; only the lock
	/// itself was missing. Without it, a concurrent <see cref="RemoveAt"/>/<see cref="Insert"/> could be
	/// observed mid-mutation (a torn read) or the index could go out of range between the bounds check
	/// and the access.
	/// </remarks>
	public override T this[int index]
	{
		get
		{
			DumpBuffer();
			using var read = RWLock.ReadLock();
			return InternalSource[index];
		}
		set
		{
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
		int i;
		using (RWLock.ReadLock()) i = base.IndexOf(item);
		if (i != -1 || _buffer.IsEmpty) return i;
		DumpBuffer(); // one dump then accept results.
		using var read = RWLock.ReadLock();
		return base.IndexOf(item);
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
	{
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
		DumpBuffer();
		using var read = RWLock.ReadLock();
		base.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	/// <remarks>
	/// The base <see cref="ReadOnlyCollectionWrapper{T,TCollection}.Export(ICollection{T})"/> implementation
	/// neither drains the buffer nor takes a lock, so without this override buffered-but-undrained items
	/// would be silently omitted from <paramref name="to"/>.
	/// </remarks>
	public override void Export(ICollection<T> to)
	{
		DumpBuffer();
		using var read = RWLock.ReadLock();
		to.AddRange(InternalSource);
	}

	/// <inheritdoc />
	/// <remarks>
	/// The base <see cref="ReadOnlyCollectionWrapper{T,TCollection}.CopyTo(Span{T})"/> implementation
	/// neither drains the buffer nor takes a lock, so without this override buffered-but-undrained items
	/// would be silently omitted from the copy.
	/// </remarks>
	public override Span<T> CopyTo(Span<T> span)
	{
		DumpBuffer();
		using var read = RWLock.ReadLock();
		return base.CopyTo(span);
	}

	/// <inheritdoc />
	/// <remarks>
	/// This drains the buffer before enumerating but, like every other non-thread-safe .NET collection
	/// enumerator, does not hold a lock for the full duration of the enumeration: a structural change to
	/// <c>InternalSource</c> made by another thread while the caller is iterating will surface as the
	/// standard <see cref="InvalidOperationException"/> ("Collection was modified"). Making the walk itself
	/// atomic would require either allocating a full snapshot on every call (paid even when nothing mutates
	/// concurrently) or holding a lock for a caller-controlled, unbounded duration (which would block
	/// writers indefinitely). Callers that need a fully safe point-in-time view should use
	/// <see cref="Snapshot"/> instead.
	/// </remarks>
	[ExcludeFromCodeCoverage]
	public override IEnumerator<T> GetEnumerator()
	{
		DumpBuffer();
		return base.GetEnumerator().Preflight(ThrowIfDisposedDelegate);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public T[] Snapshot()
	{
		DumpBuffer();
		using var read = RWLock.ReadLock();
		return InternalSource.ToArray();
	}

	/// <inheritdoc />
	public void Read(Action action)
	{
		DumpBuffer();
		using var read = RWLock.ReadLock();
		action();
	}

	/// <inheritdoc />
	public TResult Read<TResult>(Func<TResult> action)
	{
		DumpBuffer();
		using var read = RWLock.ReadLock();
		return action();
	}
}
