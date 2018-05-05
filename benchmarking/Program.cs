using Open.Diagnostics;
using Open.Collections;
using System;
using System.IO;
using System.Text;
using Open.Collections.Synchronized;
using System.Collections.Generic;

class Program
{
	static void Main(string[] args)
	{
		CollectionTests();

		Console.Beep();
		Console.WriteLine("(press any key when finished)");
		Console.ReadKey();
	}

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

		{
			var report = new BenchmarkConsoleReport<Func<IQueue<object>>>(500000, (c, r, p) => QueueParallelBenchmark.Results(c, r, p));

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

	}

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
			var report = new BenchmarkConsoleReport<Func<ICollection<object>>>(100000, (c, r, p) => ListParallelBenchmark.Results(c, r, p));

			report.AddBenchmark("LockSynchronizedList",
				count => () => new LockSynchronizedList<object>());
			report.AddBenchmark("ReadWriteSynchronizedList",
				count => () => new ReadWriteSynchronizedList<object>());

			report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.


			report.Test(100);
			report.Test(250);
			report.Test(1000);
			report.Test(2000);
		}


		Console.WriteLine("::: Synchronized HashSets :::\n");
		{
			var report = new BenchmarkConsoleReport<Func<ICollection<object>>>(100000, (c, r, p) => CollectionParallelBenchmark.Results(c, r, p));

			report.AddBenchmark("LockSynchronizedHashSet",
				count => () => new LockSynchronizedHashSet<object>());
			report.AddBenchmark("ReadWriteSynchronizedHashSet",
				count => () => new ReadWriteSynchronizedHashSet<object>());

			report.Pretest(200, 200); // Run once through first to scramble/warm-up initial conditions.


			report.Test(100);
			report.Test(250);
			report.Test(1000);
			report.Test(2000,2);
		}

	}

}
