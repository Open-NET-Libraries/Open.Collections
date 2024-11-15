using Open.Diagnostics;
using System;
using System.Collections.Generic;

namespace Open.Collections;

public class QueueBenchmark(uint size, uint repeat, Func<IQueue<object>> queueFactory) : BenchmarkBase<Func<IQueue<object>>>(size, repeat, queueFactory)
{
	protected readonly object _item = new();

	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		IQueue<object> queue = Param();

		yield return TimedResult.Measure("Fill", () =>
		{
			for (int i = 0; i < TestSize; i++) queue.Enqueue(_item);
		});

		yield return TimedResult.Measure("Empty", () =>
		{
			for (int i = 0; i < TestSize; i++) queue.TryDequeue(out object _);
		});
	}

	public static TimedResult[] Results(uint size, uint repeat, Func<IQueue<object>> queueFactory)
		=> new QueueBenchmark(size, repeat, queueFactory).Result;
}
