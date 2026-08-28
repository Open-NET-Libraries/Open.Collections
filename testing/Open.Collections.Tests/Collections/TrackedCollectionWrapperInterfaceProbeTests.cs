using System;
using System.Collections.Generic;
using FluentAssertions;
using Open.Collections.Synchronized;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Regression coverage for the AddRange(ReadOnlySpan&lt;T&gt;) fix when called
/// through the <see cref="IAddMultiple{T}"/> interface rather than the
/// concrete class. This path matters because <see cref="IAddMultiple{T}"/>
/// declares its span overload with <c>[OverloadResolutionPriority(1)]</c>
/// (preferred), while <see cref="TrackedCollectionWrapper{T, TCollection}"/>'s
/// own implementation declares it with <c>[OverloadResolutionPriority(-1)]</c>
/// (deprioritized, since it's <see cref="ObsoleteAttribute">obsolete</see>).
/// A call through an interface-typed reference with an array argument can
/// therefore resolve to a different overload than an equivalent call through
/// a class-typed reference -- verified empirically (via temporary call
/// counters, since removed) to invoke the span overload exactly once, which
/// then forwards to the enumerable overload exactly once with no recursion.
/// </summary>
public class TrackedCollectionWrapperInterfaceProbeTests
{
	[Fact]
	public void InterfaceTyped_AddRange_WithArrayArgument_DoesNotRecurse_AndAddsCorrectly()
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		IAddMultiple<int> iface = wrapper;

		// This mirrors BasicCollectionTests<T>.AddRange()'s
		// `c.AddRange(Array.Empty<int>())` / `c.AddRange(e)` pattern where `c`
		// is statically typed as IAddMultiple<int>. An int[] argument is
		// implicitly convertible to BOTH IEnumerable<int> and ReadOnlySpan<int>,
		// so which overload wins is exactly the ambiguity the priority
		// attributes are meant to resolve. If this recursed, the test would
		// hang/stack-overflow rather than fail an assertion.
		int[] arr = [10, 20, 30];
		iface.AddRange(arr);

		wrapper.Snapshot().Should().Equal(10, 20, 30);
		wrapper.Count.Should().Be(3);
	}

	[Fact]
	public void InterfaceTyped_AddRange_WithEmptyArray_AddsNothing()
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		IAddMultiple<int> iface = wrapper;

		int changed = 0;
		wrapper.Changed += (_, _) => changed++;

		iface.AddRange(Array.Empty<int>());

		wrapper.Count.Should().Be(0);
		changed.Should().Be(0);
	}

#if NET9_0_OR_GREATER
	[Fact]
	public void InterfaceTyped_AddRange_WithRealSpanLiteral_DoesNotRecurse()
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		IAddMultiple<int> iface = wrapper;

		// A genuine ReadOnlySpan<int> argument at an interface-typed call site
		// has only one applicable candidate (ReadOnlySpan<T> can't convert to
		// IEnumerable<T>), so this MUST invoke AddRange(ReadOnlySpan<T>) on the
		// concrete type. If that method's internal call back to `AddRange(...)`
		// resolved back to itself (ignoring the class's own priority(-1)
		// attribute), this would stack-overflow.
		iface.AddRange((ReadOnlySpan<int>)[7, 8, 9]);

		wrapper.Snapshot().Should().Equal(7, 8, 9);
	}
#endif

	[Fact]
	public void ClassTyped_AddRange_WithArrayArgument_ResolvesToEnumerableOverload()
	{
		// Sanity check of the fix's premise: calling AddRange(T[]) directly
		// through the concrete class type (not the interface) must not pick
		// the class's own deprioritized span overload, or `AddRange(items.ToArray())`
		// inside the span overload would recurse infinitely.
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		int[] arr = [1, 2, 3];
		wrapper.AddRange(arr);
		wrapper.Snapshot().Should().Equal(1, 2, 3);
	}
}
