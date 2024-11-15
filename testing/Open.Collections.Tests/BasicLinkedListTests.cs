using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicLinkedListTests<TList>(TList collection) : BasicCollectionTests<TList>(collection)
	where TList : ILinkedList<int>, new()
{
	protected BasicLinkedListTests()
		: this(new()) { }

	[Fact]
	public void TryTake()
	{
		Collection.Clear();
		Collection.TryTakeFirst(out _).Should().BeFalse();
		Collection.TryTakeLast(out _).Should().BeFalse();
		Collection.Add(10);
		Collection.AddFirst(1);
		Collection.AddLast(2);
		Collection.TryTakeFirst(out int first).Should().BeTrue();
		first.Should().Be(1);
		Collection.TryTakeLast(out int last).Should().BeTrue();
		last.Should().Be(2);
	}

	[Fact]
	public void AddRemoveNodes()
	{
		Collection.Clear();
		var remain = new LinkedListNode<int>(2);
		Collection.AddFirst(remain);
		Collection.AddFirst(new LinkedListNode<int>(1));
		Collection.AddLast(new LinkedListNode<int>(3));
		Collection.First.Value.Should().Be(1);
		Collection.Last.Value.Should().Be(3);
		Collection.RemoveFirst();
		Collection.RemoveLast();
		Collection.First.Value.Should().Be(2);
		Collection.Last.Value.Should().Be(2);
		Collection.Remove(remain);
		Collection.Count.Should().Be(0);
	}
}
