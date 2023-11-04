using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

public static partial class Queue
{
	public class Standard<T> : Queue<T>, IQueue<T>
	{
		[ExcludeFromCodeCoverage]
		protected Standard()
		{
		}

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
		public new virtual bool TryDequeue(out T item)
			=> base.TryDequeue(out item);

		/// <inheritdoc />
		[ExcludeFromCodeCoverage]
		public new virtual bool TryPeek(out T item)
			=> base.TryPeek(out item);
#endif

	}
}
