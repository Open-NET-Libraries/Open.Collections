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
		static ImmutableArray<int> Set5 = Enumerable.Range(1, 11).ToImmutableArray();

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

			var progressive = Set1.SubsetsProgressive(2).ToArray();
			Assert.Equal(expected, progressive);
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

			var progressive = Set2.SubsetsProgressive(2).ToArray();
			Assert.Equal(expected, progressive);
		}

		[Fact]
		public void TestSubset3_2()
		{
			var expected = new char[][] {
				new char[] { 'A', 'B' },
				new char[] { 'A', 'C' },
				new char[] { 'B', 'C' },
				new char[] { 'A', 'D' },
				new char[] { 'B', 'D' },
				new char[] { 'C', 'D' },
			};
			Assert.Equal(expected.Length, Set3.Subsets(2).Count());

			var progressive = Set3.SubsetsProgressive(2).ToArray();
			Assert.Equal(expected, progressive);
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

			var progressive = Set3.SubsetsProgressive(3).ToArray();
			Assert.Equal(expected, progressive);
		}

		[Fact]
		public void TestSubset4_4()
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

			var progressive = Set4.SubsetsProgressive(4).ToArray();
			Assert.Equal(expected, progressive);
		}

		[Fact]
		public void TestSubset4_3()
		{
			var expected = new int[][] {
				new int[] { 1, 2, 3 },
				new int[] { 1, 2, 4 },
				new int[] { 1, 3, 4 },
				new int[] { 2, 3, 4 },
				new int[] { 1, 2, 5 },
				new int[] { 1, 3, 5 },
				new int[] { 1, 4, 5 },
				new int[] { 2, 3, 5 },
				new int[] { 2, 4, 5 },
				new int[] { 3, 4, 5 },
			};
			var actual = Set4.SubsetsProgressive(3).ToArray();
			Assert.Equal(expected, actual);

			var a2 = Set5.SubsetsProgressive(3);
			var actual2 = a2.Take(10).ToArray();
			Assert.Equal(expected, actual2);


			Assert.Equal(Set5.Subsets(3).Count(), a2.Count());
			var a3 = Set5.Subsets(3);
			// Checksum.
			Assert.Equal(a3.SelectMany(e => e).Sum(), a2.SelectMany(e => e).Sum());

		}

		[Theory]
		[InlineData(8, 5)]
		[InlineData(10, 7)]
		[InlineData(12, 3)]
		[InlineData(16, 4)]
		public void LargerProgressiveCheck(int size, int count)
		{
			
			var FullSet = Enumerable.Range(1, size).ToImmutableArray();
			var buffer = new int[count];
			var s1 = FullSet.Subsets(count, buffer).ToImmutableArray();
			var s2 = FullSet.SubsetsProgressive(count, buffer).ToImmutableArray();
			var s1s = s1.SelectMany(e => e).Sum();
			var s2s = s2.SelectMany(e => e).Sum();
			Assert.Equal(s1.Length, s2.Length);
			Assert.Equal(s1s, s2s);
		}
	}
}
