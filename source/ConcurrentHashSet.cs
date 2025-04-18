using System.Collections.Concurrent;

namespace Open.Collections;

/// <summary>
/// A thread-safe hash by wrapping a <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ConcurrentHashSet<T> : DictionaryToHashSetWrapper<T>
	where T : notnull
{
	/// <summary>
	/// Construct a new instance with optional initial values.
	/// </summary>
	public ConcurrentHashSet(IEnumerable<T>? intialValues = null)
		: base(new ConcurrentDictionary<T, bool>())
	{
		if (intialValues is not null)
			UnionWith(intialValues);
	}
}
