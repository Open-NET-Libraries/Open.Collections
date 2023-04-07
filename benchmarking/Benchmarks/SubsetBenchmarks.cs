using BenchmarkDotNet.Attributes;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks;

[MemoryDiagnoser]
public class SubsetBenchmarks
{
	const int BufferSize = 7;
	readonly IEnumerable<int> FullSet = Enumerable.Range(0, 32);

	[Benchmark(Baseline = true)]
	public int Subsets()
	{
		int[] buffer = new int[BufferSize];
		int sum = FullSet.MemoizeUnsafe()
			.Subsets(BufferSize, buffer).Count();
		return sum;
	}

	[Benchmark]
	public int SubsetsBuffered()
	{
		int sum = FullSet.MemoizeUnsafe()
			.SubsetsBuffered(BufferSize).Count();
		return sum;
	}

	//[Benchmark]
	public int SubsetsCopied()
	{
		int sum = FullSet.MemoizeUnsafe()
			.Subsets(BufferSize).Count();
		return sum;
	}

	//[Benchmark]
	public int SubsetArrayPool()
	{
		int sum = FullSet.MemoizeUnsafe()
			.Subsets(BufferSize, ArrayPool<int>.Shared)
			.Select(e =>
			{
				using (e) return 1;
			}).Count();
		return sum;
	}

	//[Benchmark]
	public int SubsetArrayNoPool()
	{
		int sum = FullSet.MemoizeUnsafe()
			.Subsets(BufferSize, default(ArrayPool<int>))
			.Select(e =>
			{
				using (e) return 1;
			}).Count();
		return sum;
	}

	//[Benchmark]
	public int SubsetProgressive()
	{
		int[] buffer = new int[BufferSize];
		int sum = FullSet.MemoizeUnsafe()
			.SubsetsProgressive(BufferSize, buffer).Count();
		return sum;
	}
}
