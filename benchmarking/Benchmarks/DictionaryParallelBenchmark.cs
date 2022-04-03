using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //foreach (TimedResult t in base.TestOnceInternal())
        //{
        //	yield return t;
        //}

        int testSize;
        checked { testSize = (int)TestSize; }
        const int mixSize = 100;
        var c = (IDictionary<int, object>)Param();
        yield return TimedResult.Measure("Fill (.Add(keyValuePair)) (In Parallel)",
            () => Parallel.For(0, testSize, i => c.Add(_items[i])));

        yield return TimedResult.Measure("Update To Expected Values",
            () => Parallel.For(0, testSize, i => c[i] = _items[i].Value));

#if DEBUG
        for (int i = 0; i < testSize; ++i)
            Debug.Assert(c[i] == _items[i].Value);
#endif

        yield return TimedResult.Measure("Enumerate", () =>
        {
            // ReSharper disable once NotAccessedVariable
            int x = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var _ in c) { x++; }
            Debug.Assert(x == testSize);
        });

        yield return TimedResult.Measure("Enumerate (In Parallel)",
            () => Parallel.ForEach(c, _ => { }));

        yield return TimedResult.Measure(".Contains(item) (In Parallel)",
            () => Parallel.For(0, testSize * 2, i =>
            {
                int n = i * 2;
                bool has = c.ContainsKey(n);
                Debug.Assert(has == (n < testSize));
            }));

        object[] items = Enumerable.Range(0, mixSize).Select(_ => new object()).ToArray();
        yield return TimedResult.Measure("Random Set/Get", () =>
        {
            for (int i = 0; i < testSize; i++)
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
