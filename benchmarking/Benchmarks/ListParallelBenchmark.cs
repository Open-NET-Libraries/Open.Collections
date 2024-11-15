using Open.Diagnostics;
using System;
using System.Collections.Generic;

namespace Open.Collections;

public class ListParallelBenchmark(
	uint size, uint repeat, Func<IList<object>> factory)
	: CollectionParallelBenchmark(size, repeat, factory)
{
	// Get/Set (mutating entry) operations have no benefit to synchronization and are inherently thread safe.
	//protected override IEnumerable<TimedResult> TestOnceInternal()
	//{
	//	foreach (var t in base.TestOnceInternal())
	//	{
	//		yield return t;
	//	}

	//	const int mixSize = 100;
	//	var c = (IList<object>)Param();
	//	for (var i = 0; i < TestSize; i++) c.Add(_items[i]);
	//	var items = Enumerable.Range(0, mixSize).Select(i => new object()).ToArray();

	//	yield return TimedResult.Measure("Random Set/Get", () =>
	//	{
	//		for (var i = 0; i < TestSize; i++)
	//		{
	//			Parallel.For(0, mixSize, x =>
	//			{
	//				if (x % 2 == 0)
	//				{
	//					c[i] = items[x];
	//				}
	//				else
	//				{
	//					var value = c[i];
	//				}
	//			});
	//		}
	//	});
	//}

	public static TimedResult[] Results(uint size, uint repeat, Func<IList<object>> factory)
		=> new ListParallelBenchmark(size, repeat, factory).Result;
}
