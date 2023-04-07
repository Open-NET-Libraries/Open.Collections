using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class CombinationTests
{
	static readonly ImmutableArray<int> Set1 = ImmutableArray.Create(1, 2);
	static readonly ImmutableArray<char> Set2 = ImmutableArray.Create('A', 'C', 'E');
	static readonly ImmutableArray<int> Set3 = ImmutableArray.Create(0, 1);

	[Fact]
	public void TestCombination1()
	{
		int[][] expected = new int[][] {
			new int[] { 1, 1 },
			new int[] { 1, 2 },
			new int[] { 2, 1 },
			new int[] { 2, 2 },
		};
		int[][] actual = Set1.Combinations().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination1Distinct()
	{
		int[][] expected = new int[][] {
			new int[] { 1, 1 },
			new int[] { 1, 2 },
			new int[] { 2, 2 },
		};
		int[][] actual = Set1.CombinationsDistinct().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination2()
	{
		char[][] expected = new char[][] {
			new char[] { 'A', 'A' },
			new char[] { 'A', 'C' },
			new char[] { 'A', 'E' },
			new char[] { 'C', 'A' },
			new char[] { 'C', 'C' },
			new char[] { 'C', 'E' },
			new char[] { 'E', 'A' },
			new char[] { 'E', 'C' },
			new char[] { 'E', 'E' },
		};
		char[][] actual = Set2.Combinations(2).ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination2Distinct()
	{
		char[][] expected = new char[][] {
			new char[] { 'A', 'A' },
			new char[] { 'A', 'C' },
			new char[] { 'A', 'E' },
			new char[] { 'C', 'C' },
			new char[] { 'C', 'E' },
			new char[] { 'E', 'E' },
		};
		char[][] actual = Set2.CombinationsDistinct(2).ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestCombination3()
	{
		int[][] expected = new int[][] {
			new int[] { 0, 0, 0, 0 },
			new int[] { 0, 0, 0, 1 },
			new int[] { 0, 0, 1, 0 },
			new int[] { 0, 0, 1, 1 },
			new int[] { 0, 1, 0, 0 },
			new int[] { 0, 1, 0, 1 },
			new int[] { 0, 1, 1, 0 },
			new int[] { 0, 1, 1, 1 },
			new int[] { 1, 0, 0, 0 },
			new int[] { 1, 0, 0, 1 },
			new int[] { 1, 0, 1, 0 },
			new int[] { 1, 0, 1, 1 },
			new int[] { 1, 1, 0, 0 },
			new int[] { 1, 1, 0, 1 },
			new int[] { 1, 1, 1, 0 },
			new int[] { 1, 1, 1, 1 },
		};
		int[][] actual = Set3.Combinations(4).ToArray();
		Assert.Equal(expected, actual);
	}
}
