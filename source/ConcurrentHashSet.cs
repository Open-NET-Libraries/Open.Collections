﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

/// <summary>
/// A thread-safe hash by wrapping a <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ConcurrentHashSet<T> : DictionaryToHashSetWrapper<T>
	where T : notnull
{
	/// <summary>
	/// Construct a new instance with optoinal initial values.
	/// </summary>
	public ConcurrentHashSet(IEnumerable<T>? intialValues = null)
		: base(new ConcurrentDictionary<T, bool>())
	{
		if (intialValues is not null)
			UnionWith(intialValues);
	}
}
