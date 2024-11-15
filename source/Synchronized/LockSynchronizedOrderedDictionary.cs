using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

[ExcludeFromCodeCoverage]
public sealed class LockSynchronizedOrderedDictionary<TKey, TValue>(
	int capacity = 0)
	: LockSynchronizedDictionaryWrapper<TKey, TValue, OrderedDictionary<TKey, TValue>>(new OrderedDictionary<TKey, TValue>(capacity))
	where TKey : notnull
{
}
