using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class QueueParallelBenchmark : QueueBenchmark
	{
		public QueueParallelBenchmark(uint size, uint repeat, Func<IQueue<object>> factory) : base(size, repeat, factory)
		{
		}

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var queue = Param();

			yield return TimedResult.Measure("Fill (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => queue.Enqueue(_item));
			});

			yield return TimedResult.Measure("Empty (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => { queue.TryDequeue(out var _); });
			});


			yield return TimedResult.Measure("Mixed Enqueue/TryDequeue (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i =>
				{
					if (i % 2 == 0)
						queue.Enqueue(_item);
					else
						queue.TryDequeue(out _);
				});
			});

		}

		public new static TimedResult[] Results(uint size, uint repeat, Func<IQueue<object>> factory) => (new QueueParallelBenchmark(size, repeat, factory)).Result;

	}
}
