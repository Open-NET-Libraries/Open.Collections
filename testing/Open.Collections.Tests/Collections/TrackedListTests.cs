using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using Xunit;

namespace Open.Collections.Tests.Collections;
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
            switch(args.Change)
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
            for(int i = 0; i < 20; i++)
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
}
