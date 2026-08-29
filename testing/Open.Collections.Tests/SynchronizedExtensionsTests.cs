#nullable enable

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Open.Collections.Tests;

/// <summary>
/// Coverage for the free-standing <c>*Synchronized</c> extension family in
/// <see cref="Extensions"/>.
/// </summary>
/// <remarks>
/// These are the only members of this library that reach
/// <c>ThreadSafety.GetReadWriteHelper</c> in Open.Threading; the synchronized
/// collection classes use the monitor-based helpers instead.
/// <para>
/// Regression coverage for Open.Threading 3.0.4, where that method built its
/// helper through <c>ConditionalWeakTable.GetOrCreateValue</c>. That overload
/// constructs via <c>Activator.CreateInstance</c>, which requires a genuine
/// parameterless constructor, and <c>ReadWriteHelper</c> declares only
/// <c>ReadWriteHelper(bool supportRecursion = false)</c>. An all-optional
/// constructor is a call-site convenience, not a parameterless constructor at
/// the IL level, so every method below threw <see cref="MissingMethodException"/>
/// rather than returning. Fixed in 3.0.5.
/// </para>
/// </remarks>
public class SynchronizedExtensionsTests
{
	[Fact]
	public void TryGetValueSynchronized_ExistingKey()
	{
		var d = new Dictionary<string, int> { ["a"] = 1 };
		d.TryGetValueSynchronized("a", out int value).Should().BeTrue();
		value.Should().Be(1);
	}

	[Fact]
	public void TryGetValueSynchronized_MissingKey()
	{
		var d = new Dictionary<string, int>();
		d.TryGetValueSynchronized("nope", out int value).Should().BeFalse();
		value.Should().Be(0);
	}

	[Fact]
	public void GetValueSynchronized_ExistingKey()
	{
		var d = new Dictionary<string, int> { ["a"] = 42 };
		d.GetValueSynchronized("a").Should().Be(42);
	}

	[Fact]
	public void GetValueSynchronized_MissingKey_Throws()
	{
		var d = new Dictionary<string, int>();
		Assert.Throws<KeyNotFoundException>(() => d.GetValueSynchronized("nope"));
	}

	[Fact]
	public void GetValueSynchronized_MissingKey_ReturnsDefault()
	{
		var d = new Dictionary<string, int>();
		d.GetValueSynchronized("nope", -1).Should().Be(-1);
	}

	[Fact]
	public void RegisterSynchronized_AddsOnlyOnce()
	{
		var list = new List<int>();
		list.RegisterSynchronized(5);
		list.RegisterSynchronized(5);
		list.Should().ContainSingle().Which.Should().Be(5);
	}

	[Fact]
	public void AddSynchronized_Adds()
	{
		var list = new List<int>();
		list.AddSynchronized(1);
		list.AddSynchronized(1);
		list.Should().Equal(1, 1);
	}


	[Fact]
	public void AddOrUpdateSynchronized_UpdatesWhenPresent()
	{
		var d = new Dictionary<string, int> { ["a"] = 1 };
		d.AddOrUpdateSynchronized("a", 99, (_, existing) => existing + 10).Should().Be(11);
		d["a"].Should().Be(11);
	}

	[Fact]
	public void EnsureDefaultSynchronized_SetsOnlyWhenMissing()
	{
		var d = new Dictionary<string, int>();
		d.EnsureDefaultSynchronized("a", 5);
		d.EnsureDefaultSynchronized("a", 9);
		d["a"].Should().Be(5);
	}

	[Fact]
	public void TryAddSynchronized_AddsOnlyWhenMissing()
	{
		var d = new Dictionary<string, int>();
		d.TryAddSynchronized("a", 1).Should().BeTrue();
		d.TryAddSynchronized("a", 2).Should().BeFalse();
		d["a"].Should().Be(1);
	}

	[Fact]
	public void TryAddSynchronized_FactoryOverload()
	{
		var d = new Dictionary<string, int>();
		d.TryAddSynchronized("a", () => 1).Should().BeTrue();
		d.TryAddSynchronized("a", () => 2).Should().BeFalse();
		d["a"].Should().Be(1);
	}

	[Fact]
	public void TryRemoveSynchronized_RemovesExisting()
	{
		var d = new Dictionary<string, int> { ["a"] = 1 };
		d.TryRemoveSynchronized("a").Should().BeTrue();
		d.TryRemoveSynchronized("a").Should().BeFalse();
		d.Should().BeEmpty();
	}

	[Fact]
	public void TryRemoveSynchronized_OutValue()
	{
		var d = new Dictionary<string, int> { ["a"] = 7 };
		d.TryRemoveSynchronized("a", out int value).Should().BeTrue();
		value.Should().Be(7);
	}

	// ---------------------------------------------------------------
	// Characterization tests.
	//
	// These record what the library does TODAY. They are not an
	// endorsement of the behavior; several of these cases are open
	// questions. They exist so the branch is honest about its state
	// and so any future change to these paths is deliberate rather
	// than silent.
	// ---------------------------------------------------------------

	/// <summary>
	/// On .NET 10 against a concrete <see cref="Dictionary{TKey, TValue}"/>,
	/// the <c>CollectionsMarshal</c> fast path writes through a null ref when
	/// the key is absent. The same call against a non-Dictionary target adds
	/// normally, so the two paths currently disagree.
	/// </summary>
	[Fact]
	public void AddOrUpdateSynchronized_MissingKey_CurrentlyThrows()
	{
		var d = new Dictionary<string, int>();
		Assert.Throws<NullReferenceException>(
			() => d.AddOrUpdateSynchronized("a", 1, (_, existing) => existing + 10));
	}

	/// <inheritdoc cref="AddOrUpdateSynchronized_MissingKey_CurrentlyThrows"/>
	[Fact]
	public void AddOrUpdateSynchronized_ValueFactory_MissingKey_CurrentlyThrows()
	{
		var d = new Dictionary<string, int>();
		Assert.Throws<NullReferenceException>(
			() => d.AddOrUpdateSynchronized("a", _ => 7, (_, existing) => existing + 1));
	}

	/// <summary>
	/// Currently returns the incoming value rather than the stored one, which
	/// differs from the non-synchronized <c>GetOrAdd</c>, which returns the
	/// existing value.
	/// </summary>
	[Fact]
	public void GetOrAddSynchronized_ExistingKey_CurrentlyReturnsIncomingValue()
	{
		var d = new Dictionary<string, int>();
		d.GetOrAddSynchronized("a", 3).Should().Be(3);
		d.GetOrAddSynchronized("a", 99).Should().Be(99);
	}

	/// <inheritdoc cref="GetOrAddSynchronized_ExistingKey_CurrentlyReturnsIncomingValue"/>
	[Fact]
	public void GetOrAddSynchronized_Factory_ExistingKey_CurrentlyReturnsIncomingValue()
	{
		var d = new Dictionary<string, int>();
		d.GetOrAddSynchronized("a", _ => 3).Should().Be(3);
		d.GetOrAddSynchronized("a", _ => 99).Should().Be(99);
	}

	/// <summary>
	/// The second append is currently lost; the list ends with one entry.
	/// Likely downstream of
	/// <see cref="GetOrAddSynchronized_ExistingKey_CurrentlyReturnsIncomingValue"/>,
	/// which hands back a value that is not the one held in the dictionary.
	/// </summary>
	[Fact]
	public void AddToSynchronized_SecondAppend_CurrentlyLost()
	{
		var d = new Dictionary<string, IList<int>>();
		d.AddToSynchronized("a", 1);
		d.AddToSynchronized("a", 2);
		d["a"].Should().Equal(1);
	}

	/// <summary>
	/// Takes a read lock while the write lock is held, which
	/// <see cref="System.Threading.ReaderWriterLockSlim"/> rejects under every
	/// policy. Previously unreachable, because the whole family threw before
	/// getting this far.
	/// </summary>
	[Fact]
	public void EnsureDefaultSynchronized_Factory_CurrentlyThrowsLockRecursion()
	{
		var d = new Dictionary<string, int>();
		Assert.Throws<LockRecursionException>(() => d.EnsureDefaultSynchronized("a", _ => 5));
	}

	[Fact]
	public async Task GetOrAddSynchronized_ConcurrentCallersDoNotCorrupt()
	{
		var d = new Dictionary<string, int>();
		var tasks = new Task[32];

		for (int i = 0; i < tasks.Length; i++)
			tasks[i] = Task.Run(() => d.GetOrAddSynchronized("k", _ => 123));

		await Task.WhenAll(tasks);

		d["k"].Should().Be(123);
	}
}
