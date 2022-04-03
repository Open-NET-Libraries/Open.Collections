using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections;

public class CollectionParallelBenchmark<T> : CollectionBenchmark<T>
{
	public CollectionParallelBenchmark(uint size, uint repeat, Func<ICollection<T>> factory, Func<int, T> itemFactory) : base(size, repeat, factory, itemFactory)
	{
	}

	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
        ICollection<T> c = Param();

		yield return TimedResult.Measure("Fill (.Add(item)) (In Parallel)",
			() => Parallel.For(0, TestSize, i => c.Add(_items[i])));


        yield return TimedResult.Measure("Enumerate (8 times)", () =>
        {
            for (int i = 0; i < 8; ++i)
            {
                // ReSharper disable once NotAccessedVariable
                int x = 0;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (T _ in c) { x++; }
                Debug.Assert(x == TestSize);
            }
        });

        // It's obvious to note that you have to 'lock' a collection or acquire a 'snapshot' before enumerating.
        yield return TimedResult.Measure("Enumerate (8 times) (In Parallel)",
            () =>
            {
                for (int i = 0; i < 8; ++i)
                {
                    Parallel.ForEach(c, _ => { });
                }
            });

		yield return TimedResult.Measure(".Contains(item) (In Parallel)",
			() => Parallel.For(0, TestSize * 2, i => { bool _ = c.Contains(_items[i]); }));

		if (c is IList<T> list)
		{
			yield return TimedResult.Measure("IList<T> Read Access", () =>
			{
				for (int i = 0; i < TestSize; i += 2)
				{
					T _ = list[i];
				}
			});

			yield return TimedResult.Measure("IList<T> Read Access (In Parallel)",
				() => Parallel.For(0, (int)TestSize, i => { T _ = list[i]; }));
		}

		yield return c is ISynchronizedCollection<T> syncList
			? TimedResult.Measure(".Snapshot()", () =>
			{
                T[] _ = syncList.Snapshot();
			})
			: TimedResult.Measure(".Snapshot()", () =>
			{
                T[] _ = c.ToArray();
			});

		yield return TimedResult.Measure("Empty Backwards (.Remove(last)) (In Parallel)",
			() => Parallel.For(0, TestSize, i => c.Remove(_items[TestSize - i - 1])));

		yield return TimedResult.Measure("Refill (.Add(item)) (In Parallel)",
			() => Parallel.For(0, TestSize, i => c.Add(_items[i])));

		yield return TimedResult.Measure("50/50 Mixed Contains/Add (In Parallel)",
			() => Parallel.For(0, TestSize, i =>
			{
				if (i % 2 == 0)
					c.Contains(_items[i]);
				else
					c.Add(_items[i]);
			}));

		yield return TimedResult.Measure("10/90 Mixed Contains/Add (In Parallel)",
			() => Parallel.For(0, TestSize, i =>
			{
				if (i % 10 == 0)
					c.Contains(_items[i]);
				else
					c.Add(_items[i]);
			}));

		yield return TimedResult.Measure("90/10 Mixed Contains/Add (In Parallel)",
			() => Parallel.For(0, TestSize, i =>
			{
				if (i % 10 != 9)
					c.Contains(_items[i]);
				else
					c.Add(_items[i]);
			}));

		yield return TimedResult.Measure("50/50 Mixed Add/Remove (In Parallel)",
			() => Parallel.For(0, TestSize, i =>
			{
				if (i % 2 == 0)
					c.Add(_items[i]);
				else
					c.Remove(_items[i]);
			}));

		yield return TimedResult.Measure("Empty Forwards (.Remove(first)) (In Parallel)",
			() => Parallel.For(0, TestSize, i => c.Remove(_items[i])));
	}
}

public class CollectionParallelBenchmark : CollectionParallelBenchmark<object>
{
	public CollectionParallelBenchmark(uint size, uint repeat, Func<ICollection<object>> factory)
		: base(size, repeat, factory, _ => new object())
	{
	}

	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory, Func<int, T> itemFactory)
		=> new CollectionParallelBenchmark<T>(size, repeat, factory, itemFactory).Result;

	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory)
		where T : new()
		=> new CollectionParallelBenchmark<T>(size, repeat, factory, _ => new T()).Result;
}
