using FluentAssertions;
using Open.Collections.Synchronized;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Non-concurrent coverage that <c>Count</c> stays correct through a full add/remove/clear/re-add
/// lifecycle, and that it always agrees with <see cref="ISynchronizedCollection{T}.Snapshot"/> and with
/// a fresh enumeration, for every <see cref="Synchronized"/> collection/dictionary wrapper that routes
/// through <see cref="ReadOnlyCollectionWrapper{T, TCollection}.GetCount"/>. This does not exercise
/// concurrency; see <see cref="LockSyncGetCountRegressionTests"/> for that.
/// </summary>
public class SynchronizedCountConsistencyTests
{
	private static void AssertCountConsistent<T>(ISynchronizedCollection<T> collection, int expected)
	{
		collection.Count.Should().Be(expected);
		collection.Snapshot().Length.Should().Be(expected);
#pragma warning disable CA1829 // Use Length/Count property instead of Count() when available
#pragma warning disable RCS1196 // Call extension method as instance method
		Enumerable.Count(collection).Should().Be(expected);
#pragma warning restore RCS1196 // Call extension method as instance method
#pragma warning restore CA1829 // Use Length/Count property instead of Count() when available
	}

	private static void ExerciseCountLifecycle<TCollection>(TCollection collection)
		where TCollection : ISynchronizedCollection<int>
	{
		collection.Clear();
		AssertCountConsistent(collection, 0);

		for (int i = 0; i < 20; i++) collection.Add(i);
		AssertCountConsistent(collection, 20);

		for (int i = 0; i < 5; i++) collection.Remove(i);
		AssertCountConsistent(collection, 15);

		collection.Clear();
		AssertCountConsistent(collection, 0);

		// Re-add after a clear: guards against any cached/leftover count state.
		for (int i = 100; i < 110; i++) collection.Add(i);
		AssertCountConsistent(collection, 10);
	}

	private static void ExerciseDictionaryCountLifecycle<TDictionary>(TDictionary dictionary)
		where TDictionary : IDictionary<int, int>, ISynchronizedCollection<KeyValuePair<int, int>>
	{
		dictionary.Clear();
		AssertCountConsistent(dictionary, 0);

		for (int i = 0; i < 20; i++) dictionary.Add(i, i * 2);
		AssertCountConsistent(dictionary, 20);

		for (int i = 0; i < 5; i++) dictionary.Remove(i);
		AssertCountConsistent(dictionary, 15);

		dictionary.Clear();
		AssertCountConsistent(dictionary, 0);

		// Re-add after a clear: guards against any cached/leftover count state.
		for (int i = 100; i < 110; i++) dictionary.Add(i, i);
		AssertCountConsistent(dictionary, 10);
	}

	[Fact]
	public void LockSyncList_CountLifecycle()
		=> ExerciseCountLifecycle(new LockSynchronizedList<int>());

	[Fact]
	public void ReadWriteSyncList_CountLifecycle()
		=> ExerciseCountLifecycle(new ReadWriteSynchronizedList<int>());

	[Fact]
	public void LockSyncHashSet_CountLifecycle()
		=> ExerciseCountLifecycle(new LockSynchronizedHashSet<int>());

	[Fact]
	public void ReadWriteSyncHashSet_CountLifecycle()
		=> ExerciseCountLifecycle(new ReadWriteSynchronizedHashSet<int>());

	[Fact]
	public void LockSyncLinkedList_CountLifecycle()
		=> ExerciseCountLifecycle(new LockSynchronizedLinkedList<int>());

	[Fact]
	public void ReadWriteSyncLinkedList_CountLifecycle()
		=> ExerciseCountLifecycle(new ReadWriteSynchronizedLinkedList<int>());

	[Fact]
	public void ConcurrentList_CountLifecycle()
		=> ExerciseCountLifecycle(new ConcurrentList<int>());

	[Fact]
	public void LockSyncDictionary_CountLifecycle()
		=> ExerciseDictionaryCountLifecycle(new LockSynchronizedDictionary<int, int>());

	[Fact]
	public void ReadWriteSyncDictionary_CountLifecycle()
		=> ExerciseDictionaryCountLifecycle(new ReadWriteSynchronizedDictionary<int, int>());

	[Fact]
	public void LockSyncOrderedDictionary_CountLifecycle()
		=> ExerciseDictionaryCountLifecycle(new LockSynchronizedOrderedDictionary<int, int>());

	[Fact]
	public void ReadWriteSyncOrderedDictionary_CountLifecycle()
		=> ExerciseDictionaryCountLifecycle(new ReadWriteSynchronizedOrderedDictionary<int, int>());
}
