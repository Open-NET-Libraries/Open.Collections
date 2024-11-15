﻿using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.Collections;

public class QueueParallelBenchmark(uint size, uint repeat, Func<IQueue<object>> factory) : QueueBenchmark(size, repeat, factory)
{
	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		IQueue<object> queue = Param();

		yield return TimedResult.Measure("Fill (In Parallel)",
			() => Parallel.For(0, TestSize, _ => queue.Enqueue(_item)));

		yield return TimedResult.Measure("Empty (In Parallel)",
			() => Parallel.For(0, TestSize, _ => queue.TryDequeue(out object _)));

		yield return TimedResult.Measure("Mixed Enqueue/TryDequeue (In Parallel)",
			() => Parallel.For(0, TestSize, i =>
			{
				if (i % 2 == 0)
					queue.Enqueue(_item);
				else
					queue.TryDequeue(out _);
			}));
	}

	public new static TimedResult[] Results(uint size, uint repeat, Func<IQueue<object>> factory)
		=> new QueueParallelBenchmark(size, repeat, factory).Result;
}
