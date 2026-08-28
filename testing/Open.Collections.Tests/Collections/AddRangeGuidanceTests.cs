using FluentAssertions;
using Open.Collections.Synchronized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Characterization tests for the migration paths named in the <see cref="ObsoleteAttribute"/>
/// messages on the span overloads of <see cref="TrackedCollectionWrapper{T, TCollection}"/>.
/// </summary>
/// <remarks>
/// These are characterization tests, not regression guards: they pass both before and after
/// the message corrections. Their purpose is to prove the advice is safe to follow — that the
/// recommended replacement is behaviourally identical to the obsolete method it replaces.
/// If that ever stops being true, the guidance is wrong and these fail.
/// </remarks>
public class AddRangeGuidanceTests
{
	static (int[] Contents, int Changed, int Modified) Capture(Action<TrackedCollectionWrapper<int, List<int>>> act)
	{
		var wrapper = new TrackedCollectionWrapper<int, List<int>>([]);
		int changed = 0, modified = 0;
		wrapper.Changed += (_, _) => changed++;
		wrapper.Modified += (_, _) => modified++;
		act(wrapper);
		return (wrapper.Snapshot(), changed, modified);
	}

	[Fact]
	public void AddRange_ObsoleteSpanOverload_MatchesRecommendedEnumerableOverload()
	{
		int[] payload = [1, 2, 3];

#pragma warning disable CS0618 // Type or member is obsolete
		var viaSpan = Capture(w => w.AddRange(payload.AsSpan()));
#pragma warning restore CS0618 // Type or member is obsolete
		var viaEnumerable = Capture(w => w.AddRange(payload));

		viaSpan.Contents.Should().Equal(1, 2, 3);
		viaSpan.Should().BeEquivalentTo(viaEnumerable,
			"the Obsolete message directs callers to AddRange(IEnumerable<T>), so it must behave identically");
	}

	[Fact]
	public void AddThese_ObsoleteSpanOverload_MatchesRecommendedArrayOverload()
	{
		int[] rest = [3, 4];

#pragma warning disable CS0618 // Type or member is obsolete
		var viaSpan = Capture(w => w.AddThese(1, 2, rest.AsSpan()));
#pragma warning restore CS0618 // Type or member is obsolete
		var viaArray = Capture(w => w.AddThese(1, 2, rest));

		viaSpan.Contents.Should().Equal(1, 2, 3, 4);
		viaSpan.Should().BeEquivalentTo(viaArray,
			"the Obsolete message directs callers to AddThese(T, T, T[]), so it must behave identically");
	}

	[Fact]
	public void AddRange_EmptySpan_MatchesEmptyEnumerable()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		var viaSpan = Capture(w => w.AddRange(ReadOnlySpan<int>.Empty));
#pragma warning restore CS0618 // Type or member is obsolete
		var viaEnumerable = Capture(w => w.AddRange(Array.Empty<int>()));

		viaSpan.Contents.Should().BeEmpty();
		viaSpan.Should().BeEquivalentTo(viaEnumerable);
	}

	/// <summary>
	/// The documented pass-through cases. Each source is held in an <see cref="IEnumerable{T}"/>-typed
	/// reference deliberately: a statically <c>int[]</c>-typed argument binds to
	/// <c>AddRange(ReadOnlySpan&lt;T&gt;)</c> instead, because that overload carries
	/// <c>[OverloadResolutionPriority(1)]</c>, and that overload has its own always-uncopied body that
	/// never reaches the switch these cases are about. An <see cref="IEnumerable{T}"/>-typed reference
	/// has no span conversion, so it forces the intended overload.
	/// </summary>
	[Fact]
	[SuppressMessage("Style", "IDE0300:Simplify collection initialization", Justification = "Required for this test.")]
	[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Required for this test.")]
	public void AddRange_FixedSizeAndImmutableSources_ProduceCorrectContents()
	{
		IEnumerable<int> array = new int[] { 1, 2, 3 };              // hits the T[] arm
		IEnumerable<int> immutable = ImmutableList.Create(1, 2, 3);   // hits the IImmutableList<T> arm
		IEnumerable<int> growable = new List<int> { 1, 2, 3 };        // hits the copying default arm

		var lockList = new LockSynchronizedList<int>();
		lockList.AddRange(array);
		lockList.AddRange(immutable);
		lockList.AddRange(growable);
		lockList.Should().Equal(1, 2, 3, 1, 2, 3, 1, 2, 3);

		var rwList = new ReadWriteSynchronizedList<int>();
		rwList.AddRange(array);
		rwList.AddRange(immutable);
		rwList.AddRange(growable);
		rwList.Should().Equal(1, 2, 3, 1, 2, 3, 1, 2, 3);
	}
}
