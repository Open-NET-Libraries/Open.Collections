using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;
/// <summary>
/// Interface for implementing a Trie.
/// </summary>
public interface ITrie<TKey, TValue>
	where TKey : notnull
{
	// NOTE: Path suffixed methods are provided to avoid ambiguity.

	internal ITrieNode<TKey, TValue> EnsureNodes(ReadOnlySpan<TKey> key);

	/// <summary>
	/// Adds an entry to the Trie.
	/// </summary>
	/// <returns>Returns <see langword="true"/> if added; otherwise <see langword="false"/> if already exists.</returns>
	bool Add(ReadOnlySpan<TKey> key, in TValue value);

	/// <inheritdoc cref="Add(ReadOnlySpan{TKey}, in TValue)"/>
	bool AddPath(IEnumerable<TKey> key, in TValue value);

	/// <summary>
	/// Resets the trie.
	/// </summary>
	void Clear();

	/// <summary>
	/// Determines if a the specified <typeparamref name="TKey"/> exists.
	/// </summary>
	/// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
	bool ContainsKey(ReadOnlySpan<TKey> key);

	/// <inheritdoc cref="ContainsKey(ReadOnlySpan{TKey})"/>
	bool ContainsKeyFromPath(ICollection<TKey> key);

	/// <inheritdoc cref="ContainsKey(ReadOnlySpan{TKey})"/>
	bool ContainsKeyFromPath(IEnumerable<TKey> key);

	/// <summary>
	/// Returns the value if found, otherwise adds using the provided factory function and then returns that value.
	/// </summary>
	TValue GetOrAdd(ReadOnlySpan<TKey> key, Func<TValue> factory);

	/// <summary>
	/// Returns the value if found, otherwise adds it and returns the value provided.
	/// </summary>
	TValue GetOrAdd(ReadOnlySpan<TKey> key, in TValue value);

	/// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, Func{TValue})"/>
	TValue GetOrAddFromPath(ICollection<TKey> key, Func<TValue> factory);

	/// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, in TValue)"/>
	TValue GetOrAddFromPath(ICollection<TKey> key, in TValue value);

	/// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, Func{TValue})"/>
	TValue GetOrAddFromPath(IEnumerable<TKey> key, Func<TValue> factory);

	/// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, in TValue)"/>
	TValue GetOrAddFromPath(IEnumerable<TKey> key, in TValue value);

	/// <summary>
	/// Tries to get a value using the key and sets the <paramref name="value"/> with it if found.
	/// </summary>
	/// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
	bool TryGetValue(ReadOnlySpan<TKey> key, [MaybeNullWhen(false)] out TValue value);

	/// <inheritdoc cref="TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
	bool TryGetValueFromPath(ICollection<TKey> key, [MaybeNullWhen(false)] out TValue value);

	/// <inheritdoc cref="TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
	bool TryGetValueFromPath(IEnumerable<TKey> key, [MaybeNullWhen(false)] out TValue value);
}

/// <summary>
/// Extensions for special cases for a Trie where some .NET versions may not have implicit span conversions for strings.
/// </summary>
public static class TrieExtensions
{
	/// <inheritdoc cref="ITrie{TKey, TValue}.Add(ReadOnlySpan{TKey}, in TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[ExcludeFromCodeCoverage]
	public static bool Add<T>(this ITrie<char, T> target, string key, T value)
		=> target.Add(key.AsSpan(), value);

	/// <inheritdoc cref="ITrie{TKey, TValue}.TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[ExcludeFromCodeCoverage]
	public static bool TryGetValue<T>(this ITrie<char, T> target, string key, [MaybeNullWhen(false)] out T value)
		=> target.TryGetValue(key.AsSpan(), out value);

	/// <inheritdoc cref="ITrie{TKey, TValue}.ContainsKey(ReadOnlySpan{TKey})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[ExcludeFromCodeCoverage]
	public static bool ContainsKey<T>(this ITrie<char, T> target, string key)
		=> target.ContainsKey(key.AsSpan());

	/// <summary>
	/// Gets the string instance representing the key.
	/// </summary>
	/// <remarks>Useful for using as a string pool.</remarks>
	public static string Get(this ITrie<char, string> target, ReadOnlySpan<char> key)
	{
		var node = target.EnsureNodes(key);
		if (node.TryGetValue(out string? v))
			return v;

		string value = key.ToString();
		return node.GetOrAdd(in value);
	}
}