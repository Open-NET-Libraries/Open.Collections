using FluentAssertions;
using Xunit;

namespace Open.Collections.Tests;

public class OrderedDictionaryTests : OrderedDictionaryTests<OrderedDictionary<int, int>>;

public class IndexedDictionaryTests : OrderedDictionaryTests<IndexedDictionary<int, int>>
{
	[Fact]
	public override void EnsureOrdered()
	{
		base.EnsureOrdered();

		Dictionary.Clear();
		Dictionary.Add(20, 10);
		Dictionary.Add(19, 9);
		Dictionary.Add(18, 8);
		Dictionary.Add(17, 7);
		Dictionary.Remove(19).Should().BeTrue();
		Dictionary.Add(19, 1);
		Dictionary.Insert(1, 15, 100);

		using var e = Dictionary.GetEnumerator();
		e.MoveNext();
		e.Current.Value.Should().Be(10);
		e.MoveNext();
		e.Current.Key.Should().Be(15);
		e.Current.Value.Should().Be(100);
		e.MoveNext();
		e.Current.Value.Should().Be(8);
		e.MoveNext();
		e.Current.Value.Should().Be(7);
		e.MoveNext();
		e.Current.Value.Should().Be(1);
		e.MoveNext().Should().BeFalse();
	}
}
