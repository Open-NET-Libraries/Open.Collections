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
	/// read lock across the caller's delegate, which may re-enter members that take their own read lock.
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
	/// Locked, unlike the deliberately unsynchronized <see cref="LockSynchronizedListWrapper{T,TList}.this[int]"/>
	/// and <see cref="ReadWriteSynchronizedListWrapper{T,TList}.this[int]"/>. Those disclaim synchronization
	/// in a comment; this type does not, and already drains the buffer here.
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
	/// Drains the buffer, but does not hold a lock for the enumeration; a concurrent structural change
	/// surfaces as <see cref="InvalidOperationException"/>, as it would for <see cref="List{T}"/>.
	/// Use <see cref="Snapshot"/> for a stable view.
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
