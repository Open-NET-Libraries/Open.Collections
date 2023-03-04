using System;
using System.Collections.Generic;

namespace Open.Collections;
public interface ITrie<TKey, TValue>
{
    /// <summary>
    /// Adds an entry to the Trie.
    /// </summary>
    /// <exception cref="ArgumentException">If the entry already exists.</exception>
    void Add(ReadOnlySpan<TKey> key, TValue value);

    /// <inheritdoc cref="Add(ReadOnlySpan{TKey}, TValue)"/>
    void AddPath(IEnumerable<TKey> key, TValue value);

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
    TValue GetOrAdd(ReadOnlySpan<TKey> key, TValue value);

    /// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, Func{TValue})"/>
    TValue GetOrAddFromPath(ICollection<TKey> key, Func<TValue> factory);

    /// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, TValue)"/>
    TValue GetOrAddFromPath(ICollection<TKey> key, TValue value);

    /// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, Func{TValue})"/>
    TValue GetOrAddFromPath(IEnumerable<TKey> key, Func<TValue> factory);

    /// <inheritdoc cref="GetOrAdd(ReadOnlySpan{TKey}, TValue)"/>
    TValue GetOrAddFromPath(IEnumerable<TKey> key, TValue value);

    /// <summary>
    /// Tries to get a value using the key and sets the <paramref name="value"/> with it if found.
    /// </summary>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool TryGetValue(ReadOnlySpan<TKey> key, out TValue value);

    /// <inheritdoc cref="TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
    bool TryGetValueFromPath(ICollection<TKey> key, out TValue value);

    /// <inheritdoc cref="TryGetValue(ReadOnlySpan{TKey}, out TValue)"/>
    bool TryGetValueFromPath(IEnumerable<TKey> key, out TValue value);
}