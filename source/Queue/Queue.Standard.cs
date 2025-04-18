namespace Open.Collections;

/// <summary>
/// Static collection of queue implementations.
/// </summary>
public static partial class Queue
{
	/// <summary>
	/// A standard queue implementation that is based upon <see cref="Queue{T}"/>.
	/// </summary>
	public class Standard<T> : Queue<T>, IQueue<T>
	{
		/// <summary>
		/// Construct an empty queue.
		/// </summary>
		[ExcludeFromCodeCoverage]
		public Standard()
		{
		}

		/// <summary>
		/// Construct a queue with an initial set of items.
		/// </summary>
		[ExcludeFromCodeCoverage]
		public Standard(IEnumerable<T> initial) : base(initial)
		{
		}

#if NETSTANDARD2_0
		/// <inheritdoc />
		public virtual bool TryDequeue(out T item)
		{
			bool ok = Count != 0;
			item = ok ? Dequeue() : default!;
			return ok;
		}

		/// <inheritdoc />
		public virtual bool TryPeek(out T item)
		{
			bool ok = Count != 0;
			item = ok ? Peek() : default!;
			return ok;
		}
#else
		/* Allow for overriding. */

		/// <inheritdoc />
		[ExcludeFromCodeCoverage]
		public new virtual bool TryDequeue(
			[MaybeNullWhen(false)]
			out T item)
			=> base.TryDequeue(out item);

		/// <inheritdoc />
		[ExcludeFromCodeCoverage]
		public new virtual bool TryPeek(
			[MaybeNullWhen(false)]
			out T item)
			=> base.TryPeek(out item);
#endif

	}
}
