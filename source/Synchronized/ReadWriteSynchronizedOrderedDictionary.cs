namespace Open.Collections.Synchronized;

/// <summary>
/// A read/write synchronized <see cref="OrderedDictionary{TKey, TValue}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ReadWriteSynchronizedOrderedDictionary<TKey, TValue>(
	int capacity = 0)
	: ReadWriteSynchronizedDictionaryWrapper<TKey, TValue, OrderedDictionary<TKey, TValue>>(new OrderedDictionary<TKey, TValue>(capacity), true)
	where TKey : notnull
{
}
