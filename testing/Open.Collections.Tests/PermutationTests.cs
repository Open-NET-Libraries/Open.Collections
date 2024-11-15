using FluentAssertions;
using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class PermutationTests
{
	static readonly ImmutableArray<int> Set1 = [1, 2];
	static readonly ReadOnlyMemory<char> Set2 = new char[] { 'A', 'B', 'C' };

	[Fact]
	public void TestPermutation1a()
	{
		int[][] expected = [
			[1, 2],
			[2, 1],
		];
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
		int[][] expected = [
			[1, 2],
			[2, 1],
		];

		int[] buffer = new int[bufferLength];

		int[][] actual = Set1.Permutations(buffer).Select(b => b.ToArray()).ToArray();
		actual.Length.Should().Be(2);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestPermutation2()
	{
		char[][] expected = [
			['A', 'B', 'C'],
			['B', 'A', 'C'],
			['C', 'A', 'B'],
			['A', 'C', 'B'],
			['B', 'C', 'A'],
			['C', 'B', 'A'],
		];
		char[][] actual = Set2.Permutations().ToArray();
		Assert.Equal(expected, actual);
	}
}
