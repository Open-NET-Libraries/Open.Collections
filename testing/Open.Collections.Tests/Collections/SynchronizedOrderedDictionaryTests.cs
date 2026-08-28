using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// <see cref="LockSynchronizedOrderedDictionary{TKey, TValue}"/> and
/// <see cref="ReadWriteSynchronizedOrderedDictionary{TKey, TValue}"/> only expose a
/// <c>(int capacity = 0)</c> constructor, so they can't satisfy the <c>new()</c> constraint used by
/// <see cref="BasicDictionaryTests{TDictionary}"/> / <see cref="ParallelDictionaryTests{TDictionary}"/>.
/// Count lifecycle and Snapshot/enumeration parity for these two types is covered generically in
/// <see cref="SynchronizedCountConsistencyTests"/>; this file covers what that generic hierarchy would
/// otherwise have given for free: basic add/retrieve sanity and the disposed-Count contract.
/// </summary>
public class SynchronizedOrderedDictionaryTests
{
	[Fact]
	public void LockSyncOrderedDictionary_AddAndRetrieve()
	{
		var dictionary = new LockSynchronizedOrderedDictionary<int, int>();
		dictionary.Add(1, 10);
		dictionary.Add(2, 20);
		dictionary[1].Should().Be(10);
		dictionary[2].Should().Be(20);
		dictionary.Count.Should().Be(2);
	}

	[Fact]
	public void ReadWriteSyncOrderedDictionary_AddAndRetrieve()
	{
		var dictionary = new ReadWriteSynchronizedOrderedDictionary<int, int>();
		dictionary.Add(1, 10);
		dictionary.Add(2, 20);
		dictionary[1].Should().Be(10);
		dictionary[2].Should().Be(20);
		dictionary.Count.Should().Be(2);
	}

	[Fact]
	public void LockSyncOrderedDictionary_CountThrowsAfterDispose()
	{
		var dictionary = new LockSynchronizedOrderedDictionary<int, int>();
		dictionary.Add(1, 10);
		dictionary.Dispose();
		Assert.Throws<ObjectDisposedException>(() => _ = dictionary.Count);
	}

	[Fact]
	public void ReadWriteSyncOrderedDictionary_CountThrowsAfterDispose()
	{
		var dictionary = new ReadWriteSynchronizedOrderedDictionary<int, int>();
		dictionary.Add(1, 10);
		dictionary.Dispose();
		Assert.Throws<ObjectDisposedException>(() => _ = dictionary.Count);
	}
}
