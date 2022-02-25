using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class PermutationTests
{
	static readonly ImmutableArray<int> Set1 = ImmutableArray.Create(1, 2);
	static readonly ImmutableArray<char> Set2 = ImmutableArray.Create('A', 'B', 'C');

	[Fact]
	public void TestPermutation1()
	{
		var expected = new int[][] {
			new int[] { 1, 2 },
			new int[] { 2, 1 },
		};
		var actual = Set1.Permutations().ToArray();
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestPermutation2()
	{
		var expected = new char[][] {
			new char[] { 'A', 'B', 'C' },
			new char[] { 'B', 'A', 'C' },
			new char[] { 'C', 'A', 'B' },
			new char[] { 'A', 'C', 'B' },
			new char[] { 'B', 'C', 'A' },
			new char[] { 'C', 'B', 'A' },
		};
		var actual = Set2.Permutations().ToArray();
		Assert.Equal(expected, actual);
	}
}
