using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

public static partial class Queue
{
	public sealed class Concurrent<T> : ConcurrentQueue<T>, IQueue<T>
	{
#if NETSTANDARD2_0
		/// <inheritdoc />
		[ExcludeFromCodeCoverage]
		public void Clear()
		{
			while (TryDequeue(out _)) { }
		}
#endif
	}
}
