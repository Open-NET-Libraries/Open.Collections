using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks
{
	public class SubsetBenchmarks
	{
		readonly IEnumerable<int> FullSet = Enumerable.Range(0, 10);

		[Benchmark]
		public int Subset()
		{
			var buffer = new int[7];
			var sum = FullSet.MemoizeUnsafe().Subsets(7, buffer).SelectMany(e => e).Sum();
			return sum;
		}

		[Benchmark]
		public int SubsetProgressive()
		{
			var buffer = new int[7];
			var sum = FullSet.MemoizeUnsafe().SubsetsProgressive(7, buffer).SelectMany(e => e).Sum();
			return sum;
		}
	}
}
