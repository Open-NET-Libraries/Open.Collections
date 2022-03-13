using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections;

public class DictionaryParallelBenchmark : CollectionParallelBenchmark<KeyValuePair<int, object>>
{
	public DictionaryParallelBenchmark(uint size, uint repeat, Func<IDictionary<int, object>> factory)
		: base(size, repeat, factory, i => new KeyValuePair<int, object>(i, new object()))
	{
	}

	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		foreach (TimedResult t in base.TestOnceInternal())
		{
			yield return t;
		}

		const int mixSize = 100;
		var c = (IDictionary<int, object>)Param();
		for (int i = 0; i < TestSize; i++) c.Add(_items[i]);
        object[] items = Enumerable.Range(0, mixSize).Select(_ => new object()).ToArray();

		yield return TimedResult.Measure("Random Set/Get", () =>
		{
			for (int i = 0; i < TestSize; i++)
			{
                int i1 = i;
				Parallel.For(0, mixSize, x =>
				{
					if (x % 2 == 0)
					{
						c[i1] = items[x];
					}
					else
					{
                        object _ = c[i1];
					}
				});
			}
		});
	}

	public static TimedResult[] Results(uint size, uint repeat, Func<IDictionary<int, object>> factory)
		=> new DictionaryParallelBenchmark(size, repeat, factory).Result;
}
