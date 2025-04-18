using System.Collections.Concurrent;

namespace Open.Collections;

public static partial class Queue
{
	/// <summary>
	/// A standard <see cref="ConcurrentQueue{T}"/> implementation that allows for the <see cref="IQueue{T}"/> interface.
	/// </summary>
	/// <remarks>This allows for other queues to be used interchangeably.</remarks>
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
