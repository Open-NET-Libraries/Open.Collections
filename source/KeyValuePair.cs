using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public static class KeyValuePair
{
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new(key, value);
}
