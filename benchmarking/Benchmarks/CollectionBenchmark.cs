using Open.Diagnostics;
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
		var c = Param();

		yield return TimedResult.Measure("Fill (.Add(item))", () =>
		{
			for (var i = 0; i < TestSize; i++) c.Add(_items[i]);
		});

		yield return TimedResult.Measure("Enumerate", () =>
		{
			// ReSharper disable once NotAccessedVariable
			var x = 0;
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var _ in c) { x++; }
		});

		yield return TimedResult.Measure(".Contains(item)", () =>
		{
			for (var i = 0; i < TestSize; i++)
			{
				var _ = c.Contains(_items[i]);
			}
		});

		if (c is IList<T> list)
		{
			yield return TimedResult.Measure("IList<T> Read Access", () =>
			{
				for (var i = 0; i < TestSize; i += 2)
				{
					var _ = list[i];
				}
			});
		}

		yield return TimedResult.Measure("Empty Backwards (.Remove(last))", () =>
		{
			for (var i = 0; i < TestSize; i++) c.Remove(_items[TestSize - i - 1]);
		});

		yield return TimedResult.Measure("Refill (.Add(item))", () =>
		{
			for (var i = 0; i < TestSize; i++) c.Add(_items[i]);
		});

		yield return TimedResult.Measure("Empty Forwards (.Remove(first))", () =>
		{
			for (var i = 0; i < TestSize; i++) c.Remove(_items[i]);
		});

		for (var i = 0; i < TestSize; i++) c.Add(_items[i]);

		yield return TimedResult.Measure(".Clear()", () => c.Clear());
	}
}

public class CollectionBenchmark : CollectionBenchmark<object>
{
	public CollectionBenchmark(uint size, uint repeat, Func<ICollection<object>> factory)
		: base(size, repeat, factory, _ => new object())
	{
	}

	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory, Func<int, T> itemFactory)
		=> new CollectionBenchmark<T>(size, repeat, factory, itemFactory).Result;

	public static TimedResult[] Results<T>(uint size, uint repeat, Func<ICollection<T>> factory)
		where T : new()
		=> new CollectionBenchmark<T>(size, repeat, factory, _ => new T()).Result;
}
