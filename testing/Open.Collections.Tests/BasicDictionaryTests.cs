using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;
public abstract class BasicDictionaryTests<TDictionary>
	where TDictionary : IDictionary<int, int>, new()
{
	protected BasicDictionaryTests(TDictionary dictionary)
		=> Dictionary = dictionary;

	protected BasicDictionaryTests() : this(new()) { }

	protected readonly TDictionary Dictionary;

	[Fact]
	public TDictionary AssertWhenDisposed()
	{
		TDictionary d = new();
		d.Add(5, 10);
		if (d is not IDisposable disposable) return d;
		disposable.Dispose();
		ThrowsDisposed(() => _ = d.Any());
		ThrowsDisposed(() => _ = d.Count);
		ThrowsDisposed(() => d.ContainsKey(5));
		ThrowsDisposed(() => d.TryGetValue(5, out _));
		ThrowsDisposed(() => d[5] = 11);
		ThrowsDisposed(() => _ = d[5]);
		ThrowsDisposed(() => d.Add(5, 5));
		ThrowsDisposed(() => d.Remove(5));
		return d;
	}

	static void ThrowsDisposed(Action action)
		=> Assert.Throws<ObjectDisposedException>(action);

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
		Dictionary.Remove(1).Should().BeTrue();
		Dictionary.TryGetValue(1, out _).Should().BeFalse();

		Dictionary.Add(11, 11);
		int count = Dictionary.Count;
		Dictionary.Keys.Count.Should().Be(count);
		Dictionary.Values.Count.Should().Be(count);
	}
}
