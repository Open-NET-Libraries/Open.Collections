using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

/// <summary>
/// A node within a <see cref="ITrie{TKey, TValue}"/>.
/// </summary>
public interface ITrieNode<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// <see langword="true"/> if the value is set; otherwise <see langword="false"/>.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Gets or adds a child node for the specified key.
    /// </summary>
    ITrieNode<TKey, TValue> GetOrAddChild(TKey key);

    /// <summary>
    /// Tries to get the child node for the specified key.
    /// </summary>
    bool TryGetChild(TKey key, [MaybeNullWhen(false)] out ITrieNode<TKey, TValue> child);

    /// <summary>
    /// Gets the child node for the specified key or throws if not found
    /// </summary>
    ITrieNode<TKey, TValue> GetChild(TKey key);

    /// <summary>
    /// Tries to get the value if it is set.
    /// </summary>
    bool TryGetValue([MaybeNullWhen(false)] out TValue value);

    /// <summary>
    /// Tries to set the value if it is not already set.
    /// </summary>
    bool TrySetValue(in TValue value);

    /// <summary>
    /// Gets the value if it is set; otherwise adds it and returns the value provided.
    /// </summary>
    TValue GetOrAdd(in TValue value);

    /// <summary>
    /// Gets the value.
    /// </summary>
    TValue? Value { get; }
}