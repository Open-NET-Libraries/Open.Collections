using Open.Diagnostics;
using System;
using System.Collections.Generic;

namespace Open.Collections;

public class LinkedListBenchmark(
	uint size, uint repeat, Func<ILinkedList<object>> factory)
	: BenchmarkBase<Func<ILinkedList<object>>>(size, repeat, factory)
{
	protected readonly object _item = new();

	protected override IEnumerable<TimedResult> TestOnceInternal()
	{
		ILinkedList<object> c = Param();

		yield return TimedResult.Measure("Fill (.AddLast(item))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.AddLast(_item);
		});

		yield return TimedResult.Measure("Empty Backwards (.RemoveLast())", () =>
		{
			for (int i = 0; i < TestSize; i++) c.RemoveLast();
		});

		yield return TimedResult.Measure("Refill (.AddLast(item))", () =>
		{
			for (int i = 0; i < TestSize; i++) c.AddLast(_item);
		});

		yield return TimedResult.Measure("Empty Forwards (.RemoveFirst())", () =>
		{
			for (int i = 0; i < TestSize; i++) c.RemoveFirst();
		});

		for (int i = 0; i < TestSize; i++) c.AddLast(_item);

		yield return TimedResult.Measure(".Clear()",
			() => c.Clear());
	}

	public static TimedResult[] Results(uint size, uint repeat, Func<ILinkedList<object>> factory)
		=> new LinkedListBenchmark(size, repeat, factory).Result;
}
