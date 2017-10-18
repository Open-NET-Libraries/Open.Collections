using Open.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class LinkedListBenchmark : BenchmarkBase<Func<ILinkedList<object>>>
	{
		public LinkedListBenchmark(uint size, uint repeat, Func<ILinkedList<object>> factory) : base(size,repeat,factory)
		{
		}

		protected readonly object _item = new object();

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var c = Param();

			yield return TimedResult.Measure("Fill (.AddLast(item))", () =>
			{
				for (var i = 0; i < TestSize; i++) c.AddLast(_item);
			});

			yield return TimedResult.Measure("Empty Backwards (.RemoveLast())", () =>
			{
				for (var i = 0; i < TestSize; i++) c.RemoveLast();
			});

			yield return TimedResult.Measure("Refill (.AddLast(item))", () =>
			{
				for (var i = 0; i < TestSize; i++) c.AddLast(_item);
			});

			yield return TimedResult.Measure("Empty Forwards (.RemoveFirst())", () =>
			{
				for (var i = 0; i < TestSize; i++) c.RemoveFirst();
			});

			for (var i = 0; i < TestSize; i++) c.AddLast(_item);

			yield return TimedResult.Measure(".Clear()", () =>
			{
				c.Clear();
			});

		}

		public static TimedResult[] Results(uint size, uint repeat, Func<ILinkedList<object>> factory)
		{
			return (new LinkedListBenchmark(size, repeat, factory)).Result;
		}

	}
}
