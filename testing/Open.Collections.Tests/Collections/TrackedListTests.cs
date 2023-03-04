using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using Xunit;

namespace Open.Collections.Tests;
public class TrackedListTests : BasicListTests<TrackedList<int>>
{
    [Fact]
    public void AddRemoveEvents()
    {
        Collection.Clear();

        int version = 0;
        bool cleared = false;
        void OnCleared(object _, int ver)
        {
            cleared = true;
            version = ver;
        }

        Collection.Cleared += OnCleared;
        Collection.Clear();
        cleared.Should().BeFalse();
        Collection.Add(1);
        Collection.Clear();
        cleared.Should().BeTrue();

        int count = 0;

        void OnChanged(object source, ItemChangedEventArgs<int> args)
        {
            switch (args.Change)
            {
                case ItemChange.Added:
                    count++;
                    break;
                case ItemChange.Removed:
                    count--;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            args.Version.Should().BeGreaterThan(version);
            version = args.Version;
        }

        Collection.Changed += OnChanged;
        try
        {
            for (int i = 0; i < 20; i++)
            {
                Collection.Add(i);
            }
            Collection.Count.Should().Be(20);
            count.Should().Be(20);

            for (int i = 0; i < 10; i++)
            {
                Collection.Remove(i);
            }
            Collection.Count.Should().Be(10);
            count.Should().Be(10);

            Collection.Clear();
            cleared.Should().BeTrue();

            Collection.Count.Should().Be(0);
            count.Should().Be(10);
        }
        finally
        {
            Collection.Changed -= OnChanged;
            Collection.Cleared -= OnCleared;
        }
    }

    [Fact]
    public void Replace()
    {
        Collection.Clear();
        Collection.Replace(888, 777).Should().BeFalse();
        Collection.Add(888);
        Collection.Replace(888, 777).Should().BeTrue();
        Collection.Contains(888).Should().BeFalse();
        Collection.Contains(777).Should().BeTrue();
        Assert.Throws<ArgumentException>(() => Collection.Replace(888, 777, true));

        TrackedList<int> list = new();
        list.Dispose();
        Assert.Throws<ObjectDisposedException>(() => list.Replace(888, 777));
    }
}
