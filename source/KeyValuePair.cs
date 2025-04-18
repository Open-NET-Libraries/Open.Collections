namespace Open.Collections;

/// <summary>
/// A static helper class for creating <see cref="KeyValuePair{TKey, TValue}"/> instances.
/// </summary>
public static class KeyValuePair
{
	/// <summary>
	/// Creates a new <see cref="KeyValuePair{TKey, TValue}"/>.
	/// </summary>
	[Pure]
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new(key, value);
}
