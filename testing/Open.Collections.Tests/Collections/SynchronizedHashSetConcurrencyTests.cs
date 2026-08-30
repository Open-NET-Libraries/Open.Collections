#nullable enable

using FluentAssertions;
using Open.Collections.Synchronized;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Concurrency contract for <c>Add</c> and <c>Remove</c> on the synchronized
/// <see cref="System.Collections.Generic.HashSet{T}"/> wrappers.
/// </summary>
/// <remarks>
/// These methods use an unsynchronized pre-check to keep a no-op call off the lock,
/// then delegate to the underlying set under the lock. Because the pre-check is not
/// synchronized, several racing callers can pass it at once; the underlying
/// <c>Add</c>/<c>Remove</c> is what makes exactly one of them the winner. That is the
/// property worth pinning, and it is not covered by the shared collection tests.
/// </remarks>
public class SynchronizedHashSetConcurrencyTests
{
	private const int Racers = 64;

	[Fact]
	public async Task LockSynchronizedHashSet_ConcurrentAdd_ExactlyOneWins()
	{
		var set = new LockSynchronizedHashSet<int>();
		(await RaceAsync(() => set.Add(7)).ConfigureAwait(true)).Should().Be(1);
		set.Count.Should().Be(1);
	}

	[Fact]
	public async Task ReadWriteSynchronizedHashSet_ConcurrentAdd_ExactlyOneWins()
	{
		using var set = new ReadWriteSynchronizedHashSet<int>();
		(await RaceAsync(() => set.Add(7)).ConfigureAwait(true)).Should().Be(1);
		set.Count.Should().Be(1);
	}

	[Fact]
	public async Task LockSynchronizedHashSet_ConcurrentRemove_ExactlyOneWins()
	{
		var set = new LockSynchronizedHashSet<int>
		{
			7
		};
		(await RaceAsync(() => set.Remove(7)).ConfigureAwait(true)).Should().Be(1);
		set.Count.Should().Be(0);
	}

	[Fact]
	public async Task ReadWriteSynchronizedHashSet_ConcurrentRemove_ExactlyOneWins()
	{
		using var set = new ReadWriteSynchronizedHashSet<int>();
		set.Add(7);
		(await RaceAsync(() => set.Remove(7)).ConfigureAwait(true)).Should().Be(1);
		set.Count.Should().Be(0);
	}

	/// <summary>
	/// Releases all racers at once so they genuinely contend, and returns how many
	/// reported that they were the one to change the set.
	/// </summary>
	private static async Task<int> RaceAsync(System.Func<bool> operation)
	{
		using var gate = new ManualResetEventSlim(false);
		int winners = 0;
		var tasks = new Task[Racers];

		for (int i = 0; i < tasks.Length; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				gate.Wait();
				if (operation()) Interlocked.Increment(ref winners);
			});
		}

		gate.Set();
		await Task.WhenAll(tasks).ConfigureAwait(true);
		return winners;
	}
}
