namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized <see cref="OrderedDictionary{TKey, TValue}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class LockSynchronizedOrderedDictionary<TKey, TValue>(
	int capacity = 0)
	: LockSynchronizedDictionaryWrapper<TKey, TValue, OrderedDictionary<TKey, TValue>>(new OrderedDictionary<TKey, TValue>(capacity))
	where TKey : notnull
{
}
