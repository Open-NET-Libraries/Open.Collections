﻿using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections;

public class CollectionBenchmark<T> : BenchmarkBase<Func<ICollection<T>>>
{
	public CollectionBenchmark(uint size, uint repeat, Func<ICollection<T>> factory, Func<int, T> itemFactory) : base(size, repeat, factory) => _items = Enumerable.Range(0, (int)TestSize * 2).Select(itemFactory).ToArray();

	protected readonly T[] _items;

	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		ICollection<T> c = Param();

		yield return TimedResult.Measure("Fill (.Add(item))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.Add(_items[i]);
		});

		//yield return TimedResult.Measure("Enumerate (8 times)", () =>
		//{
		//    for (int i = 0; i < 8; ++i)
		//    {
		//        // ReSharper disable once NotAccessedVariable
		//        int x = 0;
		//        // ReSharper disable once LoopCanBeConvertedToQuery
		//        foreach (T _ in c) { x++; }
		//        Debug.Assert(x == TestSize);
		//    }
		//});

		yield return TimedResult.Measure(".Contains(item)", () =>
		{
			for (int i = 0; i < TestSize; i++)
			{
				bool _ = c.Contains(_items[i]);
			}
		});

		if (c is IList<T> list)
		{
			yield return TimedResult.Measure("IList<T> Read Access", () =>
			{
				for (int i = 0; i < TestSize; i += 2)
				{
					T _ = list[i];
				}
			});
		}

		yield return TimedResult.Measure("Empty Backwards (.Remove(last))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.Remove(_items[TestSize - i - 1]);
		});

		yield return TimedResult.Measure("Refill (.Add(item))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.Add(_items[i]);
		});

		yield return TimedResult.Measure("Empty Forwards (.Remove(first))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.Remove(_items[i]);
		});

		for (int i = 0; i < TestSize; i++) c.Add(_items[i]);

		yield return TimedResult.Measure(".Clear()", () => c.Clear());
	}
}

public class CollectionBenchmark(
	uint size, uint repeat, Func<ICollection<object>> factory) : CollectionBenchmark<object>(size, repeat, factory, _ => new object())
{
	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory, Func<int, T> itemFactory)
		=> new CollectionBenchmark<T>(size, repeat, factory, itemFactory).Result;

	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory)
		where T : new()
		=> new CollectionBenchmark<T>(size, repeat, factory, _ => new T()).Result;
}
