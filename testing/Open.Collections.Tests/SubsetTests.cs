using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class SubsetTests
{
	static readonly ImmutableArray<int> Set1 = [1, 2, 3];
	static readonly ImmutableArray<char> Set2 = ['A', 'C', 'E'];
	static readonly ImmutableArray<char> Set3 = ['A', 'B', 'C', 'D'];
	static readonly ImmutableArray<int> Set4 = Enumerable.Range(1, 5).ToImmutableArray();
	static readonly ImmutableArray<int> Set5 = Enumerable.Range(1, 11).ToImmutableArray();

	[Fact]
	public void TestSubset1_2()
	{
		int[][] expected = [
			[1, 2],
			[1, 3],
			[2, 3],
		];
		int[][] actual = Set1.Subsets(2).ToArray();
		Assert.Equal(expected, actual);

		ReadOnlyMemory<int> mem = Set1.ToArray();
		Assert.Equal(expected, mem.Subsets(2).ToArray());

		int[][] progressive = Set1.SubsetsProgressive(2).ToArray();
		Assert.Equal(expected, progressive);
	}

	[Fact]
	public void TestSubset2_2()
	{
		char[][] expected = [
			['A', 'C'],
			['A', 'E'],
			['C', 'E'],
		];
		char[][] actual = Set2.Subsets(2).ToArray();
		Assert.Equal(expected, actual);

		char[][] progressive = Set2.SubsetsProgressive(2).ToArray();
		Assert.Equal(expected, progressive);
	}

	[Fact]
	public void TestSubset3_2()
	{
		char[][] expected = [
			['A', 'B'],
			['A', 'C'],
			['B', 'C'],
			['A', 'D'],
			['B', 'D'],
			['C', 'D'],
		];
		Assert.Equal(expected.Length, Set3.Subsets(2).Count());

		char[][] progressive = Set3.SubsetsProgressive(2).ToArray();
		Assert.Equal(expected, progressive);
	}

	[Fact]
	public void TestSubset3_3()
	{
		char[][] expected = [
			['A', 'B', 'C'],
			['A', 'B', 'D'],
			['A', 'C', 'D'],
			['B', 'C', 'D'],
		];
		char[][] actual = Set3.Subsets(3).ToArray();
		Assert.Equal(expected, actual);

		actual = Set3
			.Subsets(3, ArrayPool<char>.Shared, true)
			.Select(Selector).ToArray();
		Assert.Equal(expected, actual);

		char[][] progressive = Set3.SubsetsProgressive(3).ToArray();
		Assert.Equal(expected, progressive);

		progressive = Set3
			.SubsetsProgressive(3, ArrayPool<char>.Shared, true)
			.Select(Selector).ToArray();
		Assert.Equal(expected, progressive);
	}

	static T[] Selector<T>(ArrayPoolSegment<T> e)
	{
		T[] a = e.Segment.ToArray();
		e.Dispose();
		return a;
	}

	[Fact]
	public void TestSubset4_4()
	{
		int[][] expected = [
			[1, 2, 3, 4],
			[1, 2, 3, 5],
			[1, 2, 4, 5],
			[1, 3, 4, 5],
			[2, 3, 4, 5],
		];
		int[][] actual = Set4.Subsets(4).ToArray();
		Assert.Equal(expected, actual);

		actual = Set4
			.Subsets(4, ArrayPool<int>.Shared)
			.Select(Selector).ToArray();
		Assert.Equal(expected, actual);

		int[][] progressive = Set4.SubsetsProgressive(4).ToArray();
		Assert.Equal(expected, progressive);

		progressive = Set4
			.SubsetsProgressive(4, ArrayPool<int>.Shared)
			.Select(Selector).ToArray();
		Assert.Equal(expected, progressive);
	}

	[Fact]
	public void TestSubset4_3()
	{
		int[][] expected = [
			[1, 2, 3],
			[1, 2, 4],
			[1, 3, 4],
			[2, 3, 4],
			[1, 2, 5],
			[1, 3, 5],
			[1, 4, 5],
			[2, 3, 5],
			[2, 4, 5],
			[3, 4, 5],
		];
		int[][] actual = Set4.SubsetsProgressive(3).ToArray();
		Assert.Equal(expected, actual);

		System.Collections.Generic.IEnumerable<int[]> a2 = Set5.SubsetsProgressive(3);
		int[][] actual2 = a2.Take(10).ToArray();
		Assert.Equal(expected, actual2);

		Assert.Equal(Set5.Subsets(3).Count(), a2.Count());
		System.Collections.Generic.IEnumerable<int[]> a3 = Set5.Subsets(3);
		// Checksum.
		Assert.Equal(a3.SelectMany(e => e).Sum(), a2.SelectMany(e => e).Sum());
	}

	//[Theory]
	//[InlineData(8, 5)]
	//[InlineData(10, 7)]
	//[InlineData(12, 3)]
	//[InlineData(16, 4)]
	//public void LargerProgressiveCheck(int size, int count)
	//{
	//	var FullSet = Enumerable.Range(1, size).ToImmutableArray();
	//	int[] buffer = new int[count];
	//	var s1 = FullSet.Subsets(count, buffer).ToImmutableArray();
	//	var s2 = FullSet.SubsetsProgressive(count, buffer).ToImmutableArray();
	//	int s1s = s1.SelectMany(e => e).Sum();
	//	int s2s = s2.SelectMany(e => e).Sum();
	//	Assert.Equal(s1.Length, s2.Length);
	//	Assert.Equal(s1s, s2s);
	//}
}
