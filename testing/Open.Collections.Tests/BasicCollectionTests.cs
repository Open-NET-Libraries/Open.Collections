using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicCollectionTests<TCollection>(TCollection collection)
	where TCollection : ICollection<int>, new()
{
	protected BasicCollectionTests() : this(new()) { }

	protected readonly TCollection Collection = collection;

	protected virtual TCollection AssertWhenDisposedCore()
	{
		// Policy:
		// Ideally an exception should throw whenever access occurs after disposal.
		// But this isn't always possible, and sometimes remnants remain that can't easily be detected.
		// Therefore, the policy is:
		// - Behaviors should be consistent.
		// - Throw on any modification attempt.
		// - Return negative or empty results for any read attempt.

		TCollection c = new();
		c.Add(5);
		if (c is not IDisposable disposable) return c;
		disposable.Dispose();
		ThrowsDisposed(() => _ = c.Any());
		ThrowsDisposed(() => _ = c.Count);
		ThrowsDisposed(() => c.Contains(5));
		ThrowsDisposed(() => c.Add(5));
		ThrowsDisposed(() => c.Remove(5));
		return c;
	}

	[Fact]
	public void AssertWhenDisposed()
		=> AssertWhenDisposedCore();

	protected static void ThrowsDisposed(Action action)
		=> Assert.Throws<ObjectDisposedException>(action);

	[Fact]
	public void Add()
	{
		if (Collection.IsReadOnly)
		{
			Assert.Throws<Exception>(() => Collection.Add(1));
			return;
		}

		int count = Collection.Count;
		Collection.Add(1);
		Collection.Count.Should().Be(count + 1);

		if (Collection is not ISet<int> s) return;
		s.Add(1).Should().BeFalse();
		s.Remove(1).Should().BeTrue();
		s.Add(1).Should().BeTrue();
	}

	[Fact]
	public void AddThese()
	{
		if (Collection is not IAddMultiple<int> c) return;
		if (Collection.IsReadOnly)
		{
			Assert.Throws<Exception>(() => c.AddThese(1, 2, 3, 4));
			return;
		}

		int count = Collection.Count;
		c.AddThese(1, 2, 3, 4);
		Collection.Count.Should().Be(count + 4);
	}

	[Fact]
	public void AddRange()
	{
		if (Collection is not IAddMultiple<int> c) return;
		var e = Enumerable.Range(1, 4);
		if (Collection.IsReadOnly)
		{
			Assert.Throws<Exception>(() => c.AddRange(e));
			return;
		}

		int count = Collection.Count;
		c.AddRange(e);
		count += 4;
		Collection.Count.Should().Be(count);
		c.AddRange(Enumerable.Empty<int>());
		Collection.Count.Should().Be(count);
		c.AddRange(Array.Empty<int>());
		Collection.Count.Should().Be(count);
		c.AddRange(e.ToImmutableArray());
		Collection.Count.Should().Be(count + 4);
	}

	[Fact]
	public void Clear()
	{
		if (Collection.IsReadOnly)
		{
			Assert.Throws<Exception>(() => Collection.Clear());
			return;
		}

		Collection.Add(1);
		Collection.Add(2);
		Collection.Clear();
		Collection.Count.Should().Be(0);
	}

	[Fact]
	public void Contains()
	{
		int search;
		if (Collection.IsReadOnly)
		{
			search = Collection.LastOrDefault();
			if (search == 0) return;
		}
		else
		{
			Collection.Add(1);
			Collection.Add(2);
			Collection.Add(3);
			search = 2;
		}

		Collection.Contains(search).Should().BeTrue();

		if (Collection is not ISynchronizedCollectionWrapper<int, ICollection<int>> c) return;

		bool test = false;
		c.IfNotContains(1, _ => test = true);
		test.Should().BeFalse();
		c.IfContains(1, _ => test = true);
		test.Should().BeTrue();

		test = false;
		c.IfContains(1000, _ => test = true);
		test.Should().BeFalse();
		c.IfNotContains(1000, _ => test = true);
		test.Should().BeTrue();
	}

	[Fact]
	public void CopyTo()
	{
		if (!Collection.IsReadOnly)
		{
			Collection.Add(1);
			Collection.Add(2);
		}

		int[] copy = new int[Collection.Count];
		Collection.CopyTo(copy, 0);
		copy.Should().BeEquivalentTo(Collection);
	}

	[Fact]
	public void Remove()
	{
		if (Collection.IsReadOnly)
		{
			Assert.Throws<Exception>(() => Collection.Remove(1));
			return;
		}

		Collection.Add(1);
		Collection.Add(2);
		Collection.Remove(1).Should().BeTrue();
		Collection.Remove(int.MaxValue).Should().BeFalse();
	}
}
