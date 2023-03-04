using FluentAssertions;
using System;
using Xunit;

namespace Open.Collections.Tests;
public static class TrieTests
{
    [Fact]
    public static void ValidateExists()
    {
        string[] examples = new[]
        {
            "abcd",
            "dcba",
            "abcdef",
            "the brown fox",
            "xxx"
        };

        var trie = new Trie<char, string>();

        trie.TryGetValueFromPath(default, out _).Should().BeFalse();

        for (int i = 0; i < examples.Length; i++)
        {
            for (int j = i; j < examples.Length; j++)
            {
                string e = examples[j];
                trie.ContainsKeyFromPath(e).Should().BeFalse();
                trie.TryGetValueFromPath(e, out _).Should().BeFalse();
                trie.ContainsKey(e).Should().BeFalse();
                trie.TryGetValue(e, out _).Should().BeFalse();
            }

            {
                string e = examples[i];
                trie.AddPath(e, e);
                Assert.Throws<ArgumentException>(() => trie.Add(e, e));
                Assert.Throws<ArgumentException>(() => trie.AddPath(e, e));
            }

            for (int j = 0; j <= i; j++)
            {
                string e = examples[j];
                trie.ContainsKeyFromPath(e).Should().BeTrue();
                trie.TryGetValueFromPath(e, out _).Should().BeTrue();
                trie.ContainsKey(e).Should().BeTrue();
                trie.TryGetValue(e, out _).Should().BeTrue();
            }
        }

        trie.Clear();


    }
}
