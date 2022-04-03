using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicDictionaryTests<TDictionary>
    where TDictionary : IDictionary<int, int>
{
    protected BasicDictionaryTests(TDictionary dictionary)
        => Dictionary = dictionary;

    protected readonly TDictionary Dictionary;

    [Fact]
    public void SetAndCheck()
    {
        if (Dictionary.IsReadOnly) return;

        Dictionary.Clear();
        Dictionary.Add(10, 11);
        Dictionary.ContainsKey(10).Should().BeTrue();
        int value = Dictionary[10];
        value.Should().Be(11);
        Dictionary.ContainsKey(int.MaxValue).Should().BeFalse();
        Dictionary[0] = int.MaxValue;
        Dictionary[0].Should().Be(int.MaxValue);
        Dictionary.ContainsKey(0).Should().BeTrue();
        Dictionary.ContainsKey(int.MaxValue).Should().BeFalse();
        Dictionary[0] = value;
        Dictionary[0].Should().Be(value);
    }

    [Fact]
    public void AddAndRemove()
    {
        if (Dictionary.IsReadOnly)
        {
            Assert.Throws<Exception>(() => Dictionary.Add(0, 1));
            Assert.Throws<Exception>(() => Dictionary.Remove(0));
            return;
        }

        Dictionary.Clear();
        Dictionary.Add(0, 1);
        Assert.Throws<ArgumentException>(() => Dictionary.Add(0, 2));
        Dictionary.Add(1, 3);
        Dictionary[0].Should().Be(1);
        Dictionary[1].Should().Be(3);
        Dictionary.Remove(1);
        Dictionary.TryGetValue(1, out _).Should().BeFalse();
    }
}
