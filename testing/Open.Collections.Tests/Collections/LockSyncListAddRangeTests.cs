using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression tests for <see cref="LockSynchronizedCollectionWrapper{T, TCollection}.AddRange(IEnumerable{T})"/>.
/// </summary>
/// <remarks>
/// <see cref="LockSynchronizedCollectionWrapper{T, TCollection}.AddRange(IEnumerable{T})"/> materializes its
/// input into an array/list specifically so the internal lock is not held across enumeration of an
/// arbitrary (potentially slow, or side-effecting) caller-supplied sequence. A prior bug passed the
/// original, unmaterialized sequence to the base implementation instead of the materialized copy,
/// which silently enumerated the source a second time and defeated the whole point of materializing
/// it first. These tests assert the source is enumerated exactly once.
/// </remarks>
public class LockSyncListAddRangeTests
{
	/// <summary>
	/// A single-pass <see cref="IEnumerable{T}"/> that counts how many times
	/// <see cref="GetEnumerator"/> is invoked and throws if invoked more than once.
	/// </summary>
	private sealed class SinglePassEnumerable(IEnumerable<int> source) : IEnumerable<int>
	{
		public int EnumerationCount { get; private set; }

		public IEnumerator<int> GetEnumerator()
		{
			EnumerationCount++;
			if (EnumerationCount > 1)
				throw new InvalidOperationException("This sequence may only be enumerated once.");

			return Enumerate();

			IEnumerator<int> Enumerate()
			{
				foreach (int i in source)
					yield return i;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	[Fact]
	public void AddRange_EnumeratesSourceExactlyOnce()
	{
		var list = new LockSynchronizedList<int>();
		var source = new SinglePassEnumerable([1, 2, 3, 4, 5]);

		list.AddRange(source);

		source.EnumerationCount.Should().Be(1);
		list.Should().Equal(1, 2, 3, 4, 5);
	}

	[Fact]
	public void AddRange_EmptySequence_EnumeratesAtMostOnceAndAddsNothing()
	{
		var list = new LockSynchronizedList<int>();
		var source = new SinglePassEnumerable([]);

		list.AddRange(source);

		source.EnumerationCount.Should().Be(1);
		list.Should().BeEmpty();
	}

	[Fact]
	public void AddRange_NullSequence_DoesNotThrowAndAddsNothing()
	{
		var list = new LockSynchronizedList<int>();

		list.AddRange(null!);

		list.Should().BeEmpty();
	}
}
