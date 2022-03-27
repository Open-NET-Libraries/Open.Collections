using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class LockSynchronizedQueue<T> : Queue.Standard<T>, IQueue<T>
{
	/// <summary>
	/// The underlying object used for synchronization.  This is exposed to allow for more complex synchronization operations.
	/// </summary>
	public object SyncRoot { get; } = new();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public new void Enqueue(T item)
	{
		lock (SyncRoot) base.Enqueue(item);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override bool TryDequeue(out T item)
	{
		if (Count == 0)
		{
			item = default!;
			return false;
		}

		lock (SyncRoot)
			return base.TryDequeue(out item);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override bool TryPeek(out T item)
	{
		if (Count == 0)
		{
			item = default!;
			return false;
		}

		lock (SyncRoot)
			return base.TryPeek(out item);
	}
}
