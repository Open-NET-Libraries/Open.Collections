using Open.Collections.Numeric;
using Xunit;

namespace Open.Collections.Tests
{
	public class SumCombinationTests
	{
		readonly PossibleAddends SC = new();

		[Fact]
		public void NoAddendsLessThan2()
		{
			for (int i = 0; i < 2; i++)
				Assert.Equal(0, SC.UniqueAddendsFor(7, i).Count);
		}

		[Fact]
		public void NoAddendsWithLowSum()
		{
			for (int i = 0; i < 4; i++)
				Assert.Equal(0, SC.UniqueAddendsFor(2, i).Count);
		}

		[Fact]
		public void AddendsFor2()
		{
			{
				var result = SC.UniqueAddendsFor(3, 2);
				Assert.Equal(1, result.Count);
				Assert.Equal(new int[] { 1, 2 }, result[0]);
			}
			{
				var result = SC.UniqueAddendsFor(4, 2);
				Assert.Equal(1, result.Count);
				Assert.Equal(new int[] { 1, 3 }, result[0]);
			}
			{
				var result = SC.UniqueAddendsFor(5, 2);
				Assert.Equal(2, result.Count);
				Assert.Equal(new int[] { 1, 4 }, result[0]);
				Assert.Equal(new int[] { 2, 3 }, result[1]);
			}
			{
				var result = SC.UniqueAddendsFor(6, 2);
				Assert.Equal(2, result.Count);
				Assert.Equal(new int[] { 1, 5 }, result[0]);
				Assert.Equal(new int[] { 2, 4 }, result[1]);
			}
			{
				var result = SC.UniqueAddendsFor(7, 2);
				Assert.Equal(3, result.Count);
				Assert.Equal(new int[] { 1, 6 }, result[0]);
				Assert.Equal(new int[] { 2, 5 }, result[1]);
				Assert.Equal(new int[] { 3, 4 }, result[2]);
			}
		}


		[Fact]
		public void AddendsFor3()
		{
			{
				for (int i = 0; i < 6; i++)
				{
					var result = SC.UniqueAddendsFor(i, 3);
					Assert.Equal(0, result.Count);
				}
			}
			{
				var result = SC.UniqueAddendsFor(6, 3);
				Assert.Equal(1, result.Count);
				Assert.Equal(new int[] { 1, 2, 3 }, result[0]);
			}
			{
				var result = SC.UniqueAddendsFor(7, 3);
				Assert.Equal(1, result.Count);
				Assert.Equal(new int[] { 1, 2, 4 }, result[0]);
			}
			{
				var result = SC.UniqueAddendsFor(8, 3);
				Assert.Equal(2, result.Count);
				Assert.Equal(new int[] { 1, 2, 5 }, result[0]);
				Assert.Equal(new int[] { 1, 3, 4 }, result[1]);
			}
			{
				var result = SC.UniqueAddendsFor(9, 3);
				Assert.Equal(3, result.Count);
				Assert.Equal(new int[] { 1, 2, 6 }, result[0]);
				Assert.Equal(new int[] { 1, 3, 5 }, result[1]);
				Assert.Equal(new int[] { 2, 3, 4 }, result[2]);
			}
			{
				var result = SC.UniqueAddendsFor(10, 3);
				Assert.Equal(4, result.Count);
				Assert.Equal(new int[] { 1, 2, 7 }, result[0]);
				Assert.Equal(new int[] { 1, 3, 6 }, result[1]);
				Assert.Equal(new int[] { 1, 4, 5 }, result[2]);
				Assert.Equal(new int[] { 2, 3, 5 }, result[3]);
			}

			{
				var result = SC.UniqueAddendsFor(15, 3);
				Assert.Equal(12, result.Count);
				Assert.Equal(new int[] { 1, 2, 12 }, result[0]);
				Assert.Equal(new int[] { 1, 3, 11 }, result[1]);
				Assert.Equal(new int[] { 1, 4, 10 }, result[2]);
				Assert.Equal(new int[] { 2, 3, 10 }, result[3]);
				Assert.Equal(new int[] { 1, 5, 9 }, result[4]);
				Assert.Equal(new int[] { 2, 4, 9 }, result[5]);
				Assert.Equal(new int[] { 1, 6, 8 }, result[6]);
				Assert.Equal(new int[] { 2, 5, 8 }, result[7]);
				Assert.Equal(new int[] { 3, 4, 8 }, result[8]);
				Assert.Equal(new int[] { 2, 6, 7 }, result[9]);
				Assert.Equal(new int[] { 3, 5, 7 }, result[10]);
				Assert.Equal(new int[] { 4, 5, 6 }, result[11]);
			}
		}

	}
}
