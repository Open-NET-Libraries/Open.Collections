using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

public static partial class Extensions
{
    [ExcludeFromCodeCoverage]

    public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        => new(source);

    [ExcludeFromCodeCoverage]
    public static IEnumerable<T> AsDequeueingEnumerable<T>(this ConcurrentQueue<T> source)
    {
        while (source.TryDequeue(out T? entry))
            yield return entry;
    }

    [ExcludeFromCodeCoverage]
    public static IEnumerable<T> AsDequeueingEnumerable<T>(this Queue<T> source)
    {
        while (source.Count != 0)
            yield return source.Dequeue();
    }
}
