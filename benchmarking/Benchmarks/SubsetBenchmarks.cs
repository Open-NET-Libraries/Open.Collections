using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks;

[MemoryDiagnoser]
public class SubsetBenchmarks
{
	readonly IEnumerable<int> FullSet = Enumerable.Range(0, 32);

	[Benchmark]
	public int Subset()
	{
		var buffer = new int[7];
		var sum = FullSet.MemoizeUnsafe().Subsets(7, buffer).Count();
		return sum;
	}

	[Benchmark]
	public int SubsetProgressive()
	{
		var buffer = new int[7];
		var sum = FullSet.MemoizeUnsafe().SubsetsProgressive(7, buffer).Count();
		return sum;
	}
}
