using System;
using System.Collections.Generic;
using FluentAssertions;
using Open.Collections.Synchronized;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression tests for <see cref="TrackedCollectionWrapper{T, TCollection}"/>.
/// </summary>
public class TrackedCollectionWrapperTests
{
	// Regression test for a bug where AddRange(ReadOnlySpan<T>) forwarded to
	// AddThese(default!, default!, items.ToArray()), which added two default(T)
	// values as *items* in addition to the span's contents.
	// Reference: source/Synchronized/TrackedCollectionWrapper.cs
	[Fact]
	public void AddRange_WithSpan_AddsExactlyTheSpanItems()
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);

		int changedCount = 0;
		int modifiedCount = 0;
		List<int> addedItems = [];

		wrapper.Changed += (_, args) =>
		{
			changedCount++;
			args.Change.Should().Be(ItemChange.Added);
			addedItems.Add(args.Value);
		};
		wrapper.Modified += (_, _) => modifiedCount++;

		ReadOnlySpan<int> span = [1, 2, 3];

#pragma warning disable CS0618 // Intentionally exercising the obsolete span overload.
		wrapper.AddRange(span);
#pragma warning restore CS0618

		// The exact bug: previously this would be [0, 0, 1, 2, 3].
		wrapper.Snapshot().Should().Equal(1, 2, 3);
		wrapper.Count.Should().Be(3);

		// Each item in the span should have raised exactly one Changed/Added event,
		// and the whole span-add should be one batch of modification.
		addedItems.Should().Equal(1, 2, 3);
		changedCount.Should().Be(3);
		modifiedCount.Should().Be(1);
	}

	[Fact]
	public void AddRange_WithEmptySpan_AddsNothing()
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);

		int changedCount = 0;
		int modifiedCount = 0;
		wrapper.Changed += (_, _) => changedCount++;
		wrapper.Modified += (_, _) => modifiedCount++;

		ReadOnlySpan<int> span = [];

#pragma warning disable CS0618 // Intentionally exercising the obsolete span overload.
		wrapper.AddRange(span);
#pragma warning restore CS0618

		wrapper.Count.Should().Be(0);
		changedCount.Should().Be(0);
	}

	[Fact]
	public void AddThese_WithSpanParams_AddsExactItems()
	{
		// Sibling overload: AddThese(T, T, params ReadOnlySpan<T>).
		// Verifying it correctly adds item1, item2, and the extra span items
		// (no default(T) values, no dropped items).
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		ReadOnlySpan<int> extra = [3, 4];

#pragma warning disable CS0618 // Intentionally exercising the obsolete span overload.
		wrapper.AddThese(1, 2, extra);
#pragma warning restore CS0618

		wrapper.Snapshot().Should().Equal(1, 2, 3, 4);
	}
}
