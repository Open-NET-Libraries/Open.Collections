#nullable enable

using FluentAssertions;
using Open.Collections.Synchronized;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Covers the indexer setter on the synchronized dictionary wrappers, which now
/// routes through the overridable <c>SetValueInternal</c> seam.
/// </summary>
/// <remarks>
/// <see cref="ReadWriteSynchronizedDictionary{TKey, TValue}"/> overrides that seam
/// with a single-probe ref lookup; the base takes the ContainsKey-then-store path.
/// Both must be observationally identical, which is what these assert.
/// </remarks>
public class SynchronizedDictionarySetterTests
{
	[Fact]
	public void ReadWriteDictionary_SetsNewKey()
	{
		using var d = new ReadWriteSynchronizedDictionary<string, int>();
		d["a"] = 1;
		d["a"].Should().Be(1);
		d.Count.Should().Be(1);
	}

	[Fact]
	public void ReadWriteDictionary_OverwritesExistingKey()
	{
		using var d = new ReadWriteSynchronizedDictionary<string, int>();
		d["a"] = 1;
		d["a"] = 2;
		d["a"].Should().Be(2);
		d.Count.Should().Be(1);
	}

	/// <summary>
	/// Guards against an accidental "skip the store when the value is equal"
	/// regression, which would change reference-identity semantics.
	/// </summary>
	[Fact]
	public void ReadWriteDictionary_StoresEvenWhenValueIsEqual()
	{
		using var d = new ReadWriteSynchronizedDictionary<string, string>();
		string first = new(['x', 'y']);
		string second = new(['x', 'y']);
		first.Should().Be(second);
		ReferenceEquals(first, second).Should().BeFalse("the test needs two distinct instances");

		d["k"] = first;
		d["k"] = second;

		ReferenceEquals(d["k"], second).Should().BeTrue("the setter must store the value it was given");
	}

	/// <summary>
	/// Exercises the base seam rather than the override, by backing the wrapper with
	/// a dictionary that is not a <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	[Fact]
	public void ReadWriteWrapper_OverNonDictionary_SetsAndOverwrites()
	{
		using var d = new ReadWriteSynchronizedDictionaryWrapper<string, int>(
			new SortedDictionary<string, int>());

		d["a"] = 1;
		d["a"].Should().Be(1);
		d["a"] = 2;
		d["a"].Should().Be(2);
		d.Count.Should().Be(1);
	}

	[Fact]
	public void LockDictionary_SetsAndOverwrites()
	{
		using var d = new LockSynchronizedDictionary<string, int>();
		d["a"] = 1;
		d["a"].Should().Be(1);
		d["a"] = 2;
		d["a"].Should().Be(2);
		d.Count.Should().Be(1);
	}
}
