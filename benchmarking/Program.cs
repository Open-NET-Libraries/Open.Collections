using Open.Collections;
using Open.Collections.Synchronized;
using Open.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

internal class Program
{
	static void Main()
	{
		TestEntry.Test1();
		TestEntry.Test2();
		CollectionTests();

		Console.Beep();
		Console.WriteLine("(press any key when finished)");
		Console.ReadKey();
	}

	class TestEntry
	{
		// ReSharper disable once MemberCanBePrivate.Local
		public int Value;

		static IEnumerable<TestEntry> EndlessTest(int limit = int.MaxValue)
		{
			var i = 0;
			while (i < limit)
				yield return new TestEntry { Value = i++ };
			// ReSharper disable once IteratorNeverReturns
		}

		public static void Test1()
		{
			Console.WriteLine("Beginning LazyList concurrency test.");
			var sw = new Stopwatch();
			using (var list = new LazyList<TestEntry>(EndlessTest()))
			{
				Parallel.For(0, 10000000, i =>
				{
					var e = list[i];
					if (e == null) throw new NullReferenceException();
					Debug.Assert(e.Value == i);
				});
				Console.WriteLine(sw.Elapsed);
				Debug.Assert(list.IndexOf(list[10000]) == 10000);
			}

		}

		public static void Test2()
		{
			Console.WriteLine("Beginning LazyList.GetEnumerator() concurrency test.");
			using (var list = new LazyList<TestEntry>(EndlessTest(10000000)))
			{
				var sw = new Stopwatch();
				Parallel.ForEach(list, e =>
				{
					if (e == null) throw new NullReferenceException();
				});
				Console.WriteLine(sw.Elapsed);
				Debug.Assert(list.IndexOf(list[10000]) == 10000);
			}

		}

	}

	// ReSharper disable once UnusedMember.Local
	static void QueueTests()
	{

		/* The obvious facts (and rank):
		 * 
		 * Non-synchronized:
		 * 1) Queue is the fastest when no sychronization is needed.
		 * 2) ConcurrentQueue isn't terribly slow, but definiltey slower than a regular queue for non-sychronized operations;
		 * 3) LockSynchronizedQueue is very slow for non-sychronized operations.
		 * 
		 * Synchronized:
		 * ConcurrentQueue beats a LockSynchronizedQueue almost every time.
		 */


		var report = new BenchmarkConsoleReport<Func<IQueue<object>>>(500000, QueueParallelBenchmark.Results);

		report.AddBenchmark("LockSynchronizedQueue",
			count => () => new LockSynchronizedQueue<object>());
		report.AddBenchmark("ConcurrentQueue",
			count => () => new Queue.Concurrent<object>());
		report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.

		report.Test(50, 4);
		report.Test(100, 8);
		report.Test(250, 16);
		report.Test(1000, 24);
		report.Test(2000, 32);
		report.Test(4000, 48);


	}

	// ReSharper disable once UnusedMember.Local
	static void LinkedListTests()
	{

		/* The obvious facts (and rank):
		 * 
		 * Non-synchronized:
		 * 1) LinkedList is the fastest when no sychronization is needed.
		 * 2) LockSynchronizedLinkedList is next in line.
		 * 3) ReadWriteSynchronizedLinkedList is very slow for non-sychronized operations.
		 * 
		 * Synchronized:
		 * LockSynchronizedLinkedList beats a ReadWriteSynchronizedLinkedList almost every time.
		 * This is expected as most of the operations are write/modify.
		 */
	}


	static void CollectionTests()
	{
		//{
		//	var report = new BenchmarkConsoleReport<Func<ICollection<object>>>(1600000, (c, r, p) => CollectionBenchmark.Results(c, r, p));

		//	report.AddBenchmark("LinkedList",
		//		count => () => new LinkedList<object>());
		//	report.AddBenchmark("HashSet",
		//		count => () => new HashSet<object>());
		//	report.AddBenchmark("List",
		//		count => () => new List<object>());
		//	report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.

		//	report.Test(100);
		//	report.Test(250);
		//	report.Test(1000);
		//}

		Console.WriteLine("::: Synchronized Lists :::\n");
		{
			var report = new BenchmarkConsoleReport<Func<IList<object>>>(100000, ListParallelBenchmark.Results);

			report.AddBenchmark("LockSynchronizedList",
				count => () => new LockSynchronizedList<object>());
			report.AddBenchmark("ReadWriteSynchronizedList",
				count => () => new ReadWriteSynchronizedList<object>());

			report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.


			report.Test(100, 4);
			report.Test(250, 4);
			report.Test(1000, 4);
			report.Test(2000, 4);
		}


		Console.WriteLine("::: Synchronized HashSets :::\n");
		{
			var report = new BenchmarkConsoleReport<Func<ICollection<object>>>(100000, CollectionParallelBenchmark.Results);

			report.AddBenchmark("ConcurrentDictionary",
				count => () => new ConcurrentHashSet<object>());

			report.AddBenchmark("LockSynchronizedHashSet",
				count => () => new LockSynchronizedHashSet<object>());
			report.AddBenchmark("ReadWriteSynchronizedHashSet",
				count => () => new ReadWriteSynchronizedHashSet<object>());

			report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.


			report.Test(100, 4);
			report.Test(250, 4);
			report.Test(1000, 4 * 4);
			report.Test(2000, 8 * 4);
		}

	}

}
