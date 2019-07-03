using System.Collections.Concurrent;

namespace Open.Collections
{
	public static partial class Queue
	{
		public class Concurrent<T> : ConcurrentQueue<T>, IQueue<T>
		{
			/// <inheritdoc />
			public void Clear()
			{
				while (TryDequeue(out _)) { }
			}
		}
	}
}
