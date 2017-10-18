using System.Collections.Generic;

namespace Open.Collections
{
	public static partial class Queue
	{
		public class Standard<T> : System.Collections.Generic.Queue<T>, IQueue<T>
		{
			public Standard() : base()
			{

			}

			public Standard(IEnumerable<T> initial) : base(initial)
			{

			}

			public virtual bool TryDequeue(out T item)
			{
				var ok = Count != 0;
				item = ok ? Dequeue() : default(T);
				return ok;
			}

			public virtual bool TryPeek(out T item)
			{
				var ok = Count != 0;
				item = ok ? Peek() : default(T);
				return ok;
			}
		}
	}
}
