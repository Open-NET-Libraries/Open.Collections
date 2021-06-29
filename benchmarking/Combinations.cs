using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Open.Collections.Benchmarking
{
	public class Combinations
	{
		public int Length { get; }
		public int Bounds { get; }

		readonly int[] Source;
		readonly Collections.Combinations Combs = new();

		public Combinations(int length = 6, int bounds = 7)
		{
			Length = length;
			Bounds = bounds;
			Source = Enumerable.Range(0, Bounds).ToArray();
		}


		[Benchmark]
		public int[][] AllPossible() => Source.Combinations(Length).Select(s=>s.ToArray()).ToArray();

		[Benchmark]
		public int[][] Subsets() => Source.Subsets(Length).ToArray();

		//[Benchmark]
		//public ImmutableArray<int>[] MemoizedStyle() => Combs.GetIndexes(Length).ToArray();
	}
}
