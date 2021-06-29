using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests
{
	public class SubsetTests
	{
		static ImmutableArray<int> Set1 = ImmutableArray.Create(1, 2, 3);
		static ImmutableArray<char> Set2 = ImmutableArray.Create('A', 'C', 'E');
		static ImmutableArray<char> Set3 = ImmutableArray.Create('A', 'B', 'C', 'D');
		static ImmutableArray<int> Set4 = Enumerable.Range(1, 5).ToImmutableArray();

		[Fact]
		public void TestSubset1_2()
		{
			var expected = new int[][] {
				new int[] { 1, 2 },
				new int[] { 1, 3 },
				new int[] { 2, 3 },
			};
			var actual = Set1.Subsets(2).ToArray();
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestSubset2_2()
		{
			var expected = new char[][] {
				new char[] { 'A', 'C' },
				new char[] { 'A', 'E' },
				new char[] { 'C', 'E' },
			};
			var actual = Set2.Subsets(2).ToArray();
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestSubset3_2()
		{
			var expected = new char[][] {
				new char[] { 'A', 'B' },
				new char[] { 'A', 'C' },
				new char[] { 'A', 'D' },
				new char[] { 'B', 'C' },
				new char[] { 'B', 'D' },
				new char[] { 'C', 'D' },
			};
			var actual = Set3.Subsets(2).ToArray();
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestSubset3_3()
		{
			var expected = new char[][] {
				new char[] { 'A', 'B', 'C' },
				new char[] { 'A', 'B', 'D' },
				new char[] { 'A', 'C', 'D' },
				new char[] { 'B', 'C', 'D' },
			};
			var actual = Set3.Subsets(3).ToArray();
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TestSubset3_4()
		{
			var expected = new int[][] {
				new int[] { 1, 2, 3, 4 },
				new int[] { 1, 2, 3, 5 },
				new int[] { 1, 2, 4, 5 },
				new int[] { 1, 3, 4, 5 },
				new int[] { 2, 3, 4, 5 },
			};
			var actual = Set4.Subsets(4).ToArray();
			Assert.Equal(expected, actual);
		}
	}
}
