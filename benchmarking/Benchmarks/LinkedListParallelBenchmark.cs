using Open.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class LinkedListParallelBenchmark : LinkedListBenchmark
	{
		public LinkedListParallelBenchmark(uint size, uint repeat, Func<ILinkedList<object>> factory) : base(size, repeat, factory)
		{

		}

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var c = Param();

			yield return TimedResult.Measure("Fill (.AddLast(item)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => c.AddLast(_item));
			});

			yield return TimedResult.Measure("Empty Backwards (.RemoveLast()) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i =>
				{
					c.RemoveLast();
				});
			});

			yield return TimedResult.Measure("Refill (.AddLast(item)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => c.AddLast(_item));
			});

			yield return TimedResult.Measure("Mixed Add/Remove Last (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i =>
				{
					if (i % 2 == 0)
						c.AddLast(_item);
					else
						c.RemoveLast();
				});
			});

			yield return TimedResult.Measure("Empty Forwards (.RemoveLast()) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => c.RemoveLast());
			});

		}

		public static new TimedResult[] Results(uint size, uint repeat, Func<ILinkedList<object>> factory)
		{
			return (new LinkedListParallelBenchmark(size, repeat, factory)).Result;
		}

	}
}
