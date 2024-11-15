using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class CombinationTests
{
	static readonly ImmutableArray<int> Set1 = [1, 2];
	static readonly ImmutableArray<char> Set2 = ['A', 'C', 'E'];
	static readonly ImmutableArray<int> Set3 = [0, 1];

	[Fact]
	public void TestCombination1()
	{
		int[][] expected = [
			[1, 1],
			[1, 2],
			[2, 1],
			[2, 2],
		];
		int[][] actual = Set1.Combinations().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination1Distinct()
	{
		int[][] expected = [
			[1, 1],
			[1, 2],
			[2, 2],
		];
		int[][] actual = Set1.CombinationsDistinct().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination2()
	{
		char[][] expected = [
			['A', 'A'],
			['A', 'C'],
			['A', 'E'],
			['C', 'A'],
			['C', 'C'],
			['C', 'E'],
			['E', 'A'],
			['E', 'C'],
			['E', 'E'],
		];
		char[][] actual = Set2.Combinations(2).ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination2Distinct()
	{
		char[][] expected = [
			['A', 'A'],
			['A', 'C'],
			['A', 'E'],
			['C', 'C'],
			['C', 'E'],
			['E', 'E'],
		];
		char[][] actual = Set2.CombinationsDistinct(2).ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination3()
	{
		int[][] expected = [
			[0, 0, 0, 0],
			[0, 0, 0, 1],
			[0, 0, 1, 0],
			[0, 0, 1, 1],
			[0, 1, 0, 0],
			[0, 1, 0, 1],
			[0, 1, 1, 0],
			[0, 1, 1, 1],
			[1, 0, 0, 0],
			[1, 0, 0, 1],
			[1, 0, 1, 0],
			[1, 0, 1, 1],
			[1, 1, 0, 0],
			[1, 1, 0, 1],
			[1, 1, 1, 0],
			[1, 1, 1, 1],
		];
		int[][] actual = Set3.Combinations(4).ToArray();
		Assert.Equal(expected, actual);
	}
}
