using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicCollectionTests<TCollection>
    where TCollection : ICollection<int>
{
    protected BasicCollectionTests(TCollection collection)
        => Collection = collection;

    protected readonly TCollection Collection;

    [Fact]
    public void Add()
    {
        if(Collection.IsReadOnly)
        {
            Assert.Throws<Exception>(() => Collection.Add(1));
            return;
        }

        int count = Collection.Count;
        Collection.Add(1);
        Collection.Count.Should().Be(count + 1);
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
    }

    [Fact]
    public void CopyTo()
    {
        if(!Collection.IsReadOnly)
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
