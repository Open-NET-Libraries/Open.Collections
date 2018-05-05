using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class ListParallelBenchmark : CollectionParallelBenchmark
	{
		public ListParallelBenchmark(uint size, uint repeat, Func<ICollection<object>> factory) : base(size, repeat, factory)
		{

		}


		protected override IEnumerable<TimedResult> TestOnceInternal()
		{
			foreach (var t in base.TestOnceInternal())
			{
				yield return t;
			}

			var c = Param();
			for (var i = 0; i < TestSize; i++) c.Add(_items[i]);

			yield return TimedResult.Measure("Random Set/Get", () =>
			{
				for (var i = 0; i < TestSize; i++)
				{
					Parallel.For(0, TestSize, x =>
					{
						if (x % 2 == 0)
						{
							_items[i] = _items[x];
						}
						else
						{
							var value = _items[i];
						}
					});
				}
			});
		}

		public static new TimedResult[] Results(uint size, uint repeat, Func<ICollection<object>> factory)
		{
			return (new ListParallelBenchmark(size, repeat, factory)).Result;
		}
	}
}
