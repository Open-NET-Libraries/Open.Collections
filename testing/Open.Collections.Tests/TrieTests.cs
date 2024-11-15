using FluentAssertions;
using Xunit;

namespace Open.Collections.Tests;
public static class TrieTests
{
	static readonly string[] Examples =
	[
		"",
		"abcd",
		"dcba",
		"abcdef",
		"the brown fox",
		"xxx"
	];

	[Fact]
	public static void TrieValidate()
		=> Test(Examples, new Trie<char, string>());

	[Fact]
	public static void ConcurrentTrieValidate()
		=> Test(Examples, new ConcurrentTrie<char, string>());

	static void Test(string[] examples, ITrie<char, string> trie)
	{
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
				trie.Add(e, e).Should().BeFalse();
				trie.AddPath(e, e).Should().BeFalse();
			}

			for (int j = 0; j <= i; j++)
			{
				string e = examples[j];
				trie.ContainsKeyFromPath(e).Should().BeTrue();
				trie.TryGetValueFromPath(e, out _).Should().BeTrue();
				trie.ContainsKey(e).Should().BeTrue();
				trie.TryGetValue(e, out string v).Should().BeTrue();
				trie.Get(e).Should().Be(v);
				trie.GetOrAdd(e, e).Should().Be(v);
			}
		}

		// Test negative case.
		trie.Get("x").Should().Be("x");
		trie.GetOrAdd("y", "y").Should().Be("y");

		trie.Clear();
	}

	[Theory]
	[InlineData("")]
	[InlineData(",")]
	[InlineData("||")]
	public static void StringJoinPoolTest(string sep)
	{
		var pool = new StringJoinPool(sep);
		string ex = pool.Get(Examples);
		ex.Should().Be(string.Join(sep, ex));
		string ex2 = pool.Get(Examples);
		ReferenceEquals(ex, ex2).Should().BeTrue();
	}
}
