using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Queue
	{
		public class Standard<T> : Queue<T>, IQueue<T>
		{
			protected Standard()
			{

			}

			public Standard(IEnumerable<T> initial) : base(initial)
			{

			}

			/// <inheritdoc />
			public virtual bool TryDequeue(out T item)
			{
				var ok = Count != 0;
				item = ok ? Dequeue() : default;
				return ok;
			}

			/// <inheritdoc />
			public virtual bool TryPeek(out T item)
			{
				var ok = Count != 0;
				item = ok ? Peek() : default;
				return ok;
			}
		}
	}
}
