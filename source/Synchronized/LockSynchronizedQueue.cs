using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

[ExcludeFromCodeCoverage]
public class LockSynchronizedQueue<T> : Queue.Standard<T>, IQueue<T>
{
	/// <summary>
	/// The underlying object used for synchronization.  This is exposed to allow for more complex synchronization operations.
	/// </summary>
	public object SyncRoot { get; } = new();

	/// <inheritdoc />
	public new void Enqueue(T item)
	{
		lock (SyncRoot) base.Enqueue(item);
	}

	/// <inheritdoc />
	public override bool TryDequeue(
#if NETSTANDARD2_0
#else
		[MaybeNullWhen(false)]
#endif
		out T item)
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
	public override bool TryPeek(
#if NETSTANDARD2_0
#else
		[MaybeNullWhen(false)]
#endif
		out T item)
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
