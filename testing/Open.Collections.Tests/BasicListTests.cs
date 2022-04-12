using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicListTests<TList>
    : BasicCollectionTests<TList>
    where TList : IList<int>, new()
{
    protected BasicListTests(TList list) : base(list) { }

    protected BasicListTests() : this(new()) { }

    public override TList AssertWhenDisposed()
    {
        var list = base.AssertWhenDisposed();
        if (list is not IDisposable) return list;
        ThrowsDisposed(() => list.IndexOf(5));
        return list;
    }

    [Fact]
    public void SetAndCheck()
    {
        if (Collection.IsReadOnly) return;

        Collection.Add(10);
        Collection.IndexOf(10).Should().Be(Collection.Count - 1);
        int value = Collection[0];
        Collection.IndexOf(int.MaxValue).Should().Be(-1);
        Collection[0] = int.MaxValue;
        Collection[0].Should().Be(int.MaxValue);
        Collection.IndexOf(int.MaxValue).Should().Be(0);
        Collection[0] = value;
        Collection[0].Should().Be(value);
    }

    [Fact]
    public void InsertAndRemoveAt()
    {
        if (Collection.IsReadOnly)
        {
            Assert.Throws<Exception>(() => Collection.Insert(0, 1));
            Assert.Throws<Exception>(() => Collection.RemoveAt(0));
            return;
        }

        Collection.Insert(0, 1);
        Collection.Insert(0, 2);
        Collection.Insert(1, 3);
        Collection[0].Should().Be(2);
        Collection[1].Should().Be(3);
        Collection[2].Should().Be(1);
        Collection.RemoveAt(1);
        Collection[1].Should().Be(1);
    }
}
