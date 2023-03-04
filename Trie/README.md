# Open.Collections.Trie

[![NuGet](https://img.shields.io/nuget/v/Open.Collections.Trie.svg)](https://www.nuget.org/packages/Open.Collections.Trie/)

A trie (pronounced "try") is a data structure that allows efficient lookup of words in a dictionary or set. It is particularly useful for applications where a large number of strings need to be stored and searched efficiently, such as spell checkers or autocomplete systems. Tries can also be used to implement a string pool, which can help reduce memory usage in programs that frequently use the same strings.

The name "Trie" comes from the word re**trie**val, which refers to the process of finding a key or value in the structure. Tries are also known as digital trees, radix trees, or prefix trees.

## Implementation

`Trie<TKey, TValue` and `ConcurrentTrie<TKey, TValue>` are simple implementations that use a length optimization to the lookups. These implementations do not support deletion or modification.

## Structure

A Trie is a rooted tree, where each node represents a single character or a group of characters in a string. The root of the tree represents the empty string, and each path from the root to a leaf represents a complete string in the set.

Here is an example of a Trie that stores the following strings: "a", "apple", "apply", "bear", and "be".

```
    ┌───a───p───p───l───e
    │       └───y
root┤
    │   ┌───b───e───a───r
    └───┤
        └───e
```