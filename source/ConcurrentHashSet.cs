using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

[ExcludeFromCodeCoverage]
public class ConcurrentHashSet<T> : DictionaryToHashSetWrapper<T>
{
	public ConcurrentHashSet(IEnumerable<T>? intialValues = null)
		: base(new ConcurrentDictionary<T, bool>())
	{
		if (intialValues is not null)
			UnionWith(intialValues);
	}
}
