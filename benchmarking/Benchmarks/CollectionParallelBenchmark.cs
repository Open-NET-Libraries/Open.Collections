using Open.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class CollectionParallelBenchmark : CollectionBenchmark
	{
		public CollectionParallelBenchmark(uint size, uint repeat, Func<ICollection<object>> factory) : base(size,repeat,factory)
		{

		}

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var c = Param();

			yield return TimedResult.Measure("Fill (.Add(item)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => c.Add(_items[i]) );
			});

			yield return TimedResult.Measure("Enumerate", () =>
			{
				Parallel.ForEach(c, i => { });
			});

			yield return TimedResult.Measure(".Contains(item) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => { c.Contains(_items[i]); });
			});

			yield return TimedResult.Measure("Empty Backwards (.Remove(last)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => { c.Remove(_items[TestSize - i - 1]); });
			});

			yield return TimedResult.Measure("Refill (.Add(item)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => c.Add(_items[i]));
			});

			yield return TimedResult.Measure("Mixed Read/Write (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i =>
				{
					if (i % 2 == 0)
						c.Add(_items[i]);
					else
						c.Remove(_items[i]);
				});
			});

			yield return TimedResult.Measure("Empty Forwards (.Remove(first)) (In Parallel)", () =>
			{
				Parallel.For(0, TestSize, i => { c.Remove(_items[i]); });
			});

		}

		public static new TimedResult[] Results(uint size, uint repeat, Func<ICollection<object>> factory)
		{
			return (new CollectionParallelBenchmark(size, repeat, factory)).Result;
		}

	}
}
