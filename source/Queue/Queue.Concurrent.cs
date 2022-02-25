using System.Collections.Concurrent;

namespace Open.Collections;

public static partial class Queue
{
	public class Concurrent<T> : ConcurrentQueue<T>, IQueue<T>
	{
#if NETSTANDARD2_0
		/// <inheritdoc />
		public void Clear()
		{
			while (TryDequeue(out _)) { }
		}
#endif
	}
}
