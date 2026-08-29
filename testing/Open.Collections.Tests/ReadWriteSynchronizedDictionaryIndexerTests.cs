#nullable enable

using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Collections.Tests;

/// <summary>
/// Lock-exclusion tests for
/// <see cref="ReadWriteSynchronizedDictionaryWrapper{TKey, TValue, TDictionary}"/>'s
/// indexer setter.
/// </summary>
/// <remarks>
/// The setter previously took an upgradable read lock and, when the key already
/// existed, wrote through it and returned without ever taking the write lock.
/// An upgradable read lock excludes other writers but permits concurrent
/// readers, so that store could land while readers were mid-enumeration. The
/// monitor-based sibling holds an exclusive lock for the same operation.
/// </remarks>
public class ReadWriteSynchronizedDictionaryIndexerTests
{
	private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan GraceForWriter = TimeSpan.FromMilliseconds(500);

	/// <summary>
	/// The existing-key path is the regression: it is the branch that returned
	/// early while holding only the upgradable read lock.
	/// </summary>
	[Fact]
	public void IndexerSetter_ExistingKey_DoesNotWriteWhileReaderHoldsLock()
	{
		using var dictionary = new ReadWriteSynchronizedDictionary<string, int>();
		dictionary.Add("a", 1);

		AssertSetterBlocksReader(dictionary, "a");
	}

	/// <summary>
	/// The missing-key path already took the write lock; this pins that it stays
	/// that way.
	/// </summary>
	[Fact]
	public void IndexerSetter_MissingKey_DoesNotWriteWhileReaderHoldsLock()
	{
		using var dictionary = new ReadWriteSynchronizedDictionary<string, int>();

		AssertSetterBlocksReader(dictionary, "absent");
	}

	private static void AssertSetterBlocksReader(
		ReadWriteSynchronizedDictionary<string, int> dictionary, string key)
	{
		using var readerHasLock = new ManualResetEventSlim(false);
		using var releaseReader = new ManualResetEventSlim(false);

		var reader = Task.Run(() => dictionary.Read(() =>
		{
			readerHasLock.Set();
			releaseReader.Wait(Patience);
		}));

		readerHasLock.Wait(Patience).Should()
			.BeTrue("the reader must acquire the read lock before the setter is attempted");

		using var writerStarted = new ManualResetEventSlim(false);
		var writer = Task.Run(() =>
		{
			writerStarted.Set();
			dictionary[key] = 99;
		});

		// Measure from when the writer is actually running, not from when it was
		// queued; otherwise a loaded machine can starve the task and report a
		// pass for an implementation that never took an exclusive lock at all.
		writerStarted.Wait(Patience).Should().BeTrue("the writer task must start");
		bool wroteWhileReaderHeldLock = writer.Wait(GraceForWriter);

		releaseReader.Set();
		Task.WaitAll([reader, writer], Patience);

		wroteWhileReaderHeldLock.Should().BeFalse(
			"the indexer setter mutates the dictionary and must exclude concurrent readers");

		dictionary[key].Should().Be(99);
	}
}
