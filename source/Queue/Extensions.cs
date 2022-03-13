using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Open.Collections;

public static partial class Extensions
{
	public static Queue<T> ToQueue<T>(this IEnumerable<T> source) => new(source);

	public static IEnumerable<T> AsDequeueingEnumerable<T>(this ConcurrentQueue<T> source)
	{
		while (source.TryDequeue(out T? entry))
			yield return entry;
	}

	public static IEnumerable<T> AsDequeueingEnumerable<T>(this Queue<T> source)
	{
		while (source.Count != 0)
			yield return source.Dequeue();
	}
}
