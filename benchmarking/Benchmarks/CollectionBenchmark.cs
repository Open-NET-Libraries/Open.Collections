using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.Collections
{
	public class CollectionBenchmark : BenchmarkBase<Func<ICollection<object>>>
	{
		public CollectionBenchmark(uint size, uint repeat, Func<ICollection<object>> factory) : base(size,repeat,factory)
		{
			_items = Enumerable.Range(0, (int)TestSize).Select(i => new Object()).ToArray();
		}

		protected readonly object[] _items;

		protected override IEnumerable<TimedResult> TestOnceInternal()
		{

			var c = Param();

			yield return TimedResult.Measure("Fill (.Add(item))", () =>
			{
				for (var i = 0; i < TestSize; i++) c.Add(_items[i]);
			});

			yield return TimedResult.Measure("Enumerate", () =>
			{
				var x = 0;
				foreach(var i in c) { x++; }
			});

			yield return TimedResult.Measure(".Contains(item)", () =>
			{
				for (var i = 0; i < TestSize; i++) c.Contains(_items[i]);
			});

			yield return TimedResult.Measure("Empty Backwards (.Remove(last))", () =>
			{
				for (var i = 0; i < TestSize; i++) c.Remove(_items[TestSize-i-1]);
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

			yield return TimedResult.Measure(".Clear()", () =>
			{
				c.Clear();
			});

		}

		public static TimedResult[] Results(uint size, uint repeat, Func<ICollection<object>> factory)
		{
			return (new CollectionBenchmark(size, repeat, factory)).Result;
		}

	}
}
