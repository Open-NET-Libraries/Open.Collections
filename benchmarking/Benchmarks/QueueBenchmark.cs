﻿using Open.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class QueueBenchmark : BenchmarkBase<Func<IQueue<object>>>
	{
		public QueueBenchmark(uint size, uint repeat, Func<IQueue<object>> queueFactory) : base(size,repeat,queueFactory)
		{
		}

		protected readonly object _item = new object();

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var queue = Param();

			yield return TimedResult.Measure("Fill", () =>
			{
				for (var i = 0; i < TestSize; i++) queue.Enqueue(_item);
			});

			yield return TimedResult.Measure("Empty", () =>
			{
				for (var i = 0; i < TestSize; i++) queue.TryDequeue(out object item);
			});

		}

		public static TimedResult[] Results(uint size, uint repeat, Func<IQueue<object>> queueFactory)
		{
			return (new QueueBenchmark(size, repeat, queueFactory)).Result;
		}

	}
}