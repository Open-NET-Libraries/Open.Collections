using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public sealed class LockSynchronizedOrderedDictionary<TKey, TValue>
	: LockSynchronizedDictionaryWrapper<TKey, TValue, OrderedDictionary<TKey, TValue>>
{
	/// <inheritdoc />
	public LockSynchronizedOrderedDictionary(int capacity = 0)
		: base(new OrderedDictionary<TKey, TValue>(capacity)) { }
}
