using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Open.Collections.Synchronized;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression tests for <c>AddRange(ReadOnlySpan{T})</c> on the synchronized collection wrappers.
/// </summary>
/// <remarks>
/// <see cref="CollectionWrapper{T, TCollection}.AddRange(ReadOnlySpan{T})"/> is a <see langword="virtual"/>
/// member declared on the (unsynchronized) base wrapper. Neither <see cref="LockSynchronizedCollectionWrapper{T, TCollection}"/>
/// nor <see cref="ReadWriteSynchronizedCollectionWrapper{T, TCollection}"/> used to override it, so calling
/// this overload on a "synchronized" wrapper silently fell back to the unlocked base implementation and
/// mutated the non-thread-safe backing collection without ever taking the wrapper's lock. Concurrent callers
/// then raced directly on the backing <see cref="System.Collections.Generic.List{T}"/> /
/// <see cref="System.Collections.Generic.HashSet{T}"/>, silently losing items (list) or triggering the
/// runtime's own concurrent-modification detector (hash set).
/// </remarks>
public class SpanAddRangeConcurrencyTests
{
	private const int ThreadCount = 8;
	private const int IterationsPerThread = 500;
	private const int ItemsPerCall = 4;
	private const int ExpectedTotal = ThreadCount * IterationsPerThread * ItemsPerCall; // 16,000

	/// <summary>
	/// Runs <paramref name="threadAction"/> concurrently on <see cref="ThreadCount"/> real OS threads
	/// (not a thread pool / <c>Parallel.For</c>) and waits for all of them to finish. Using dedicated
	/// threads maximizes the chance of true concurrent execution, which is what is needed to reliably
	/// expose a missing lock.
	/// </summary>
	/// <remarks>
	/// When the wrapper under test is unsynchronized, a corrupted <see cref="System.Collections.Generic.HashSet{T}"/>
	/// can throw from inside a worker thread. An unhandled exception on a bare <see cref="Thread"/> crashes the
	/// whole process, so every exception is captured here and re-thrown on the calling thread once all workers
	/// have finished, turning that crash into an ordinary (and much more useful) test failure.
	/// </remarks>
	private static void RunOnThreads(Action<int> threadAction)
	{
		var exceptions = new ConcurrentQueue<Exception>();
		var threads = new Thread[ThreadCount];
		for (int t = 0; t < ThreadCount; t++)
		{
			int threadIndex = t;
			threads[t] = new Thread(() =>
			{
				try
				{
					threadAction(threadIndex);
				}
				catch (Exception ex)
				{
					exceptions.Enqueue(ex);
				}
			});
		}

		foreach (var thread in threads)
			thread.Start();

		foreach (var thread in threads)
			thread.Join();

		if (!exceptions.IsEmpty)
			throw new AggregateException(exceptions.ToArray());
	}

	[Fact]
	public void LockSynchronizedList_AddRange_Span_IsThreadSafe()
	{
		var list = new LockSynchronizedList<int>();

		RunOnThreads(threadIndex =>
		{
			for (int i = 0; i < IterationsPerThread; i++)
			{
				ReadOnlySpan<int> items = [threadIndex, i, i + 1, i + 2];
				list.AddRange(items);
			}
		});

		list.Count.Should().Be(ExpectedTotal);
	}

	[Fact]
	public void ReadWriteSynchronizedList_AddRange_Span_IsThreadSafe()
	{
		var list = new ReadWriteSynchronizedList<int>();

		RunOnThreads(threadIndex =>
		{
			for (int i = 0; i < IterationsPerThread; i++)
			{
				ReadOnlySpan<int> items = [threadIndex, i, i + 1, i + 2];
				list.AddRange(items);
			}
		});

		list.Count.Should().Be(ExpectedTotal);
	}

	[Fact]
	public void LockSynchronizedHashSet_AddRange_Span_IsThreadSafe()
	{
		var set = new LockSynchronizedHashSet<int>();

		// HashSet de-duplicates, so every value added across every thread/iteration
		// must be globally unique or the final Count assertion would be meaningless.
		int nextValue = -1;

		RunOnThreads(_ =>
		{
			for (int i = 0; i < IterationsPerThread; i++)
			{
				int a = Interlocked.Increment(ref nextValue);
				int b = Interlocked.Increment(ref nextValue);
				int c = Interlocked.Increment(ref nextValue);
				int d = Interlocked.Increment(ref nextValue);
				ReadOnlySpan<int> items = [a, b, c, d];
				set.AddRange(items);
			}
		});

		set.Count.Should().Be(ExpectedTotal);
	}
}
