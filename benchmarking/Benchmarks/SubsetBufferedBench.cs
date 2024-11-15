using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections.Benchmarks;

/*
|                Method | Size | Range |           Mean |         Error |        StdDev |  Gen 0 | Allocated |
|---------------------- |----- |------ |---------------:|--------------:|--------------:|-------:|----------:|
|       SubsetsBuffered |    3 |     9 |       2.928 us |     0.0246 us |     0.0206 us | 0.0687 |     288 B |
| SubsetsMemoryBuffered |    3 |     9 |       2.862 us |     0.0235 us |     0.0208 us | 0.0763 |     320 B |
|       SubsetsBuffered |    3 |    32 |     150.029 us |     2.9457 us |     5.5327 us |      - |     288 B |
| SubsetsMemoryBuffered |    3 |    32 |     147.030 us |     2.9326 us |     4.4784 us |      - |     320 B |
|       SubsetsBuffered |    7 |     9 |       1.514 us |     0.0263 us |     0.0246 us | 0.0725 |     304 B |
| SubsetsMemoryBuffered |    7 |     9 |       1.394 us |     0.0125 us |     0.0104 us | 0.0801 |     336 B |
|       SubsetsBuffered |    7 |    32 | 105,295.284 us | 1,076.5585 us |   954.3411 us |      - |         - |
| SubsetsMemoryBuffered |    7 |    32 | 102,337.450 us | 1,462.6190 us | 1,296.5736 us |      - |         - |
*/

[MemoryDiagnoser]
//[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
public class SubsetBufferedBench
{
	int[] FullSet = Array.Empty<int>();
	ReadOnlyMemory<int> FullMemorySet = ReadOnlyMemory<int>.Empty;

	[Params(3, 7)]
	public int Size { get; set; }

	[Params(9, 32)]
	public int Range
	{
		get => FullSet.Length;
		set
		{
			int[] s = Enumerable.Range(0, value).ToArray();
			FullSet = s;
			FullMemorySet = s;
		}
	}

	[Benchmark]
	public int SubsetsBuffered()
	{
		int count = 0;
		foreach (var e in FullSet.SubsetsBuffered(Size))
			++count;

		return count;
	}

	[Benchmark]
	public int SubsetsMemoryBuffered()
	{
		int count = 0;
		foreach (var e in FullMemorySet.SubsetsBuffered(Size))
			++count;

		return count;
	}
}
