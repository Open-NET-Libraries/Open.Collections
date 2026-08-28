#nullable enable

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Open.Collections.Tests;

/// <summary>
/// Correctness tests for <see cref="ICollection{T}.Contains(T)"/>
/// (i.e. <c>IDictionary&lt;TKey,TValue&gt;.Contains(KeyValuePair&lt;TKey,TValue&gt;)</c>)
/// on dictionaries with <see langword="int"/> values, including the default(int) case.
/// </summary>
/// <remarks>
/// Regression coverage for a bug where <see cref="OrderedDictionary{TKey,TValue}"/>
/// never overrode <c>Contains(KeyValuePair{TKey,TValue})</c> and therefore fell through
/// to an O(n) <see cref="System.Collections.Generic.LinkedList{T}.Contains(T)"/> scan.
/// This class is instantiated for both <see cref="OrderedDictionary{TKey,TValue}"/> and
/// <see cref="IndexedDictionary{TKey,TValue}"/> so the two are asserted to behave identically.
/// </remarks>
public abstract class OrderedDictionaryContainsTests<TDictionary>(TDictionary dictionary)
	where TDictionary : IDictionary<int, int>, new()
{
	protected OrderedDictionaryContainsTests()
		: this(new()) { }

	protected readonly TDictionary Dictionary = dictionary;

	[Fact]
	public void Contains_KeyPresent_ValueMatches_ReturnsTrue()
	{
		Dictionary.Clear();
		Dictionary.Add(1, 100);
		Dictionary.Add(2, 200);

		Dictionary.Contains(new KeyValuePair<int, int>(2, 200)).Should().BeTrue();
	}

	[Fact]
	public void Contains_KeyPresent_ValueDiffers_ReturnsFalse()
	{
		Dictionary.Clear();
		Dictionary.Add(1, 100);

		Dictionary.Contains(new KeyValuePair<int, int>(1, 999)).Should().BeFalse();
	}

	[Fact]
	public void Contains_KeyAbsent_ReturnsFalse()
	{
		Dictionary.Clear();
		Dictionary.Add(1, 100);

		Dictionary.Contains(new KeyValuePair<int, int>(42, 100)).Should().BeFalse();
	}

	[Fact]
	public void Contains_EmptyDictionary_ReturnsFalse()
	{
		Dictionary.Clear();

		Dictionary.Contains(new KeyValuePair<int, int>(1, 1)).Should().BeFalse();
	}

	[Fact]
	public void Contains_DefaultValue_OnlyMatchesWhenStoredValueIsDefault()
	{
		Dictionary.Clear();
		Dictionary.Add(1, default);
		Dictionary.Add(2, 5);

		Dictionary.Contains(new KeyValuePair<int, int>(1, default)).Should().BeTrue();
		Dictionary.Contains(new KeyValuePair<int, int>(2, default)).Should().BeFalse();
	}

	[Fact]
	public void Contains_AfterRemove_ReturnsFalse()
	{
		Dictionary.Clear();
		Dictionary.Add(1, 100);
		Dictionary.Remove(1).Should().BeTrue();

		Dictionary.Contains(new KeyValuePair<int, int>(1, 100)).Should().BeFalse();
	}
}

/// <summary>
/// Correctness tests for <c>Contains(KeyValuePair&lt;TKey,TValue&gt;)</c> with nullable
/// reference-type values, covering the both-null / one-null comparison cases called out
/// in the bug report for the <c>node.Value.Value?.Equals(item.Value) ?? (item.Value is null)</c>
/// vs. <see cref="EqualityComparer{T}.Default"/> equivalence.
/// </summary>
public abstract class OrderedDictionaryContainsNullableValueTests<TDictionary>(TDictionary dictionary)
	where TDictionary : IDictionary<int, string?>, new()
{
	protected OrderedDictionaryContainsNullableValueTests()
		: this(new()) { }

	protected readonly TDictionary Dictionary = dictionary;

	[Fact]
	public void Contains_NullStoredValue_NullQuery_ReturnsTrue()
	{
		Dictionary.Clear();
		Dictionary.Add(1, null);

		Dictionary.Contains(new KeyValuePair<int, string?>(1, null)).Should().BeTrue();
	}

	[Fact]
	public void Contains_NullStoredValue_NonNullQuery_ReturnsFalse()
	{
		Dictionary.Clear();
		Dictionary.Add(1, null);

		Dictionary.Contains(new KeyValuePair<int, string?>(1, "x")).Should().BeFalse();
	}

	[Fact]
	public void Contains_NonNullStoredValue_NullQuery_ReturnsFalse()
	{
		Dictionary.Clear();
		Dictionary.Add(1, "x");

		Dictionary.Contains(new KeyValuePair<int, string?>(1, null)).Should().BeFalse();
	}

	[Fact]
	public void Contains_NonNullStoredValue_MatchingQuery_ReturnsTrue()
	{
		Dictionary.Clear();
		Dictionary.Add(1, "x");

		Dictionary.Contains(new KeyValuePair<int, string?>(1, "x")).Should().BeTrue();
	}
}

/// <summary>
/// Counts how many times <see cref="CountingKey.Equals(CountingKey)"/> is invoked.
/// Used to detect an O(n) scan hiding behind what should be an O(1) lookup, without
/// relying on flaky wall-clock timing.
/// </summary>
public sealed class ComparisonCounter
{
	private long _count;

	/// <summary>The number of equality comparisons observed so far.</summary>
	public long Count => Interlocked.Read(ref _count);

	/// <summary>Records one equality comparison.</summary>
	public void Increment() => Interlocked.Increment(ref _count);

	/// <summary>Resets the count to zero.</summary>
	public void Reset() => Interlocked.Exchange(ref _count, 0);
}

/// <summary>
/// A key type that records every equality comparison performed against it via a shared
/// <see cref="ComparisonCounter"/>, so tests can assert on the NUMBER of comparisons
/// performed rather than on wall-clock time (which is fragile under CI load).
/// </summary>
public readonly struct CountingKey(int value, ComparisonCounter counter) : IEquatable<CountingKey>
{
	/// <summary>The logical value this key represents.</summary>
	public int Value { get; } = value;

	private readonly ComparisonCounter _counter = counter;

	/// <inheritdoc />
	public bool Equals(CountingKey other)
	{
		_counter?.Increment();
		return Value == other.Value;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is CountingKey other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// Regression test that would have caught the O(n) <c>Contains(KeyValuePair{TKey,TValue})</c>
/// bug: it counts key-equality comparisons instead of measuring wall-clock time, which is
/// robust and not flaky under CI load.
/// </summary>
/// <remarks>
/// A correct O(1) lookup via the internal key-to-node dictionary needs only a small, N-independent
/// number of key comparisons (typically 1, occasionally a few more on hash collisions).
/// The buggy O(n) <see cref="System.Collections.Generic.LinkedList{T}.Contains(T)"/> fallback needs
/// one key comparison per node scanned - i.e. ~N comparisons when searching for the last element.
/// </remarks>
public abstract class OrderedDictionaryContainsComplexityTests<TDictionary>(TDictionary dictionary)
	where TDictionary : IDictionary<CountingKey, int>, new()
{
	protected OrderedDictionaryContainsComplexityTests()
		: this(new()) { }

	protected readonly TDictionary Dictionary = dictionary;

	private const int Count = 5000;

	// A generous, N-independent bound. An O(1) hash lookup needs a handful of comparisons;
	// the O(n) bug would need ~Count (5000) comparisons for these test cases.
	private const long MaxExpectedComparisons = 50;

	[Fact]
	public void Contains_PresentLastElement_DoesNotScanLinearlyWithCount()
	{
		var counter = new ComparisonCounter();
		Dictionary.Clear();
		for (int i = 0; i < Count; i++)
			Dictionary.Add(new CountingKey(i, counter), i);

		// Only measure the comparisons performed by the Contains call itself.
		counter.Reset();

		// Worst case for a linear scan: the last-added element.
		var target = new KeyValuePair<CountingKey, int>(new CountingKey(Count - 1, counter), Count - 1);
		bool found = Dictionary.Contains(target);

		found.Should().BeTrue();
		counter.Count.Should().BeLessThan(MaxExpectedComparisons,
			$"Contains should use an O(1) key lookup, not a linear scan "
			+ $"(observed {counter.Count} comparisons over {Count} entries)");
	}

	[Fact]
	public void Contains_MissingKey_DoesNotScanLinearlyWithCount()
	{
		var counter = new ComparisonCounter();
		Dictionary.Clear();
		for (int i = 0; i < Count; i++)
			Dictionary.Add(new CountingKey(i, counter), i);

		counter.Reset();

		var target = new KeyValuePair<CountingKey, int>(new CountingKey(Count + 1, counter), 0);
		bool found = Dictionary.Contains(target);

		found.Should().BeFalse();
		counter.Count.Should().BeLessThan(MaxExpectedComparisons,
			$"Contains should use an O(1) key lookup, not a linear scan "
			+ $"(observed {counter.Count} comparisons over {Count} entries)");
	}
}
