using FluentAssertions;
using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class PermutationTests
{
	static readonly ImmutableArray<int> Set1 = ImmutableArray.Create(1, 2);
	static readonly ReadOnlyMemory<char> Set2 = new char[] { 'A', 'B', 'C' };

	[Fact]
	public void TestPermutation1a()
	{
		int[][] expected = new int[][] {
			new int[] { 1, 2 },
			new int[] { 2, 1 },
		};
		int[][] actual = Set1.Permutations().ToArray();
		actual.Length.Should().Be(2);
		Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(12)]
	[InlineData(24)]
	public void TestPermutation1b(int bufferLength)
	{
		int[][] expected = new int[][] {
			new int[] { 1, 2 },
			new int[] { 2, 1 },
		};

		int[] buffer = new int[bufferLength];

		int[][] actual = Set1.Permutations(buffer).Select(b => b.ToArray()).ToArray();
		actual.Length.Should().Be(2);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestPermutation2()
	{
		char[][] expected = new char[][] {
			new char[] { 'A', 'B', 'C' },
			new char[] { 'B', 'A', 'C' },
			new char[] { 'C', 'A', 'B' },
			new char[] { 'A', 'C', 'B' },
			new char[] { 'B', 'C', 'A' },
			new char[] { 'C', 'B', 'A' },
		};
		char[][] actual = Set2.Permutations().ToArray();
		Assert.Equal(expected, actual);
	}
}
