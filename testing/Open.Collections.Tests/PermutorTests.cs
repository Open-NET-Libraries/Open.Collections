using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Open.Collections.Tests;

public class PermutorTests
{
	[Fact]
	public void TestNoDuplicatePermutations()
	{
		int[] numbers = { 1, 2, 3, 4 };
		var permutations = numbers.Permutations().Select(p => string.Join(",", p.Span.ToArray())).ToHashSet();

		int expectedCount = Factorial(numbers.Length);
		Assert.Equal(expectedCount, permutations.Count);
	}

	[Fact]
	public void TestSpecificPermutation()
	{
		int[] numbers = { 1, 2, 3 };
		var permutations = numbers.AsMemory().Permutations().Select(m=>m.ToArray()).ToList();
		permutations.Count.Should().Be(6);
		int[] expectedPermutation = new[] { 2, 1, 3 };
		Assert.Contains(expectedPermutation, permutations);
	}

	[Fact]
	public void TestExactPermutationsFor123()
	{
		int[] numbers = { 1, 2, 3 };
		var expectedPermutations = new List<string>
		{
			"1,2,3",
			"2,1,3",
			"3,1,2",
			"1,3,2",
			"2,3,1",
			"3,2,1"
		};

		var permutations = numbers.AsMemory().Permutations()
			.Select(p => string.Join(",", p.Span.ToArray()))
			.ToList();

		Assert.Equal(expectedPermutations.Count, permutations.Count);
		foreach (string perm in expectedPermutations)
		{
			Assert.Contains(perm, permutations);
		}
	}

	[Fact]
	public void TestStableLexicographicOrder()
	{
		int[] original = { 1, 2, 3 };
		Span<int> span = original.AsSpan();

		var permutations = new List<string>();
		do
		{
			permutations.Add(string.Join(",", span.ToArray()));
		}
		while (span.NextLexicographic());

		var expectedPermutations = new List<string>
		{
			"1,2,3",
			"1,3,2",
			"2,1,3",
			"2,3,1",
			"3,1,2",
			"3,2,1"
		};

		Assert.Equal(expectedPermutations, permutations);
	}


	[Fact]
	public void TestStableHeapsAlgorithmOrder()
	{
		int[] original = { 1, 2, 3 };
		var permutations = new List<string>();
		foreach(var s in original.Permutations())
			permutations.Add(string.Join(",", s.ToArray()));

		var expectedPermutations = new List<string>
		{
			"1,2,3",
			"2,1,3",
			"3,1,2",
			"1,3,2",
			"2,3,1",
			"3,2,1"
		};

		Assert.Equal(expectedPermutations, permutations);
	}

	[Fact]
	public void TestStableIndexedOrder()
	{
		int[] original = { 1, 2, 3 };
		var permutations = new List<string>();
		for (int i = 0; i < 6; ++i)
		{
			string s = string.Join(",", original.ToArray().AsSpan().Permutation(i).ToArray());
			int c = permutations.IndexOf(s);
			Assert.True(c==-1, $"{s} already exists in the set at index [{c}] of {permutations.Count}.");
			permutations.Contains(s).Should().BeFalse(s + " ");
			permutations.Add(s);
		}

		var expectedPermutations = new List<string>
		{
			"1,2,3",
			"3,1,2",
			"3,2,1",
			"2,3,1",
			"1,3,2",
			"2,1,3",
		};

		Assert.Equal(expectedPermutations, permutations);
	}


	private static int Factorial(int n)
	{
		int result = 1;
		for (int i = 2; i <= n; i++)
			result *= i;
		return result;
	}
}
