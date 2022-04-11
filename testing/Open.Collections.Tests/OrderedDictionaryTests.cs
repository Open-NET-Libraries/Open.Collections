using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests.Collections;
public abstract class OrderedDictionaryTests<TDictionary> : BasicDictionaryTests<TDictionary>
    where TDictionary : IDictionary<int, int>, new()
{
    protected OrderedDictionaryTests(TDictionary dictionary)
        : base(dictionary) { }

    protected OrderedDictionaryTests()
        : this(new()) { }

    [Fact]
    public virtual void EnsureOrdered()
    {
        const int count = 1000000;
        for(int i = count; i>0; i--)
        {
            Dictionary.Add(i, i);
        }

        Dictionary.Where((e, i) =>
        {
            Assert.Equal(e.Key, e.Value);
            Assert.Equal(count - i, e.Key);
            return true;
        }).Count().Should().Be(count);

        Dictionary.Clear();
        Dictionary.Add(20, 10);
        Dictionary.Add(19, 9);
        Dictionary.Add(18, 8);
        Dictionary.Add(17, 7);
        Dictionary.Remove(19).Should().BeTrue();
        Dictionary.Add(19, 1);

        using var e = Dictionary.GetEnumerator();
        e.MoveNext();
        e.Current.Value.Should().Be(10);
        e.MoveNext();
        e.Current.Value.Should().Be(8);
        e.MoveNext();
        e.Current.Value.Should().Be(7);
        e.MoveNext();
        e.Current.Value.Should().Be(1);
        e.MoveNext().Should().BeFalse();
    }
}
