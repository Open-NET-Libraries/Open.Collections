using Open.Diagnostics;
using System;
using System.Collections.Generic;

namespace Open.Collections
{
	public class QueueBenchmark : BenchmarkBase<Func<IQueue<object>>>
	{
		public QueueBenchmark(uint size, uint repeat, Func<IQueue<object>> queueFactory) : base(size, repeat, queueFactory)
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
				for (var i = 0; i < TestSize; i++) queue.TryDequeue(out var item);
			});

		}

		public static TimedResult[] Results(uint size, uint repeat, Func<IQueue<object>> queueFactory)
		{
			return (new QueueBenchmark(size, repeat, queueFactory)).Result;
		}

	}
}
