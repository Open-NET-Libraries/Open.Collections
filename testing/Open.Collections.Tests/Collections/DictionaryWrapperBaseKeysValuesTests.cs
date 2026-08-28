using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Open.Collections.Tests.Collections;

/// <summary>
/// Characterization tests asserting each shipped subclass overrides <c>Keys</c>/<c>Values</c> in a
/// shape that cannot recurse into <c>KeyCollection</c>/<c>ValueCollection</c>. See the remarks on
/// <see cref="DictionaryWrapperBase{TKey, TValue, TCollection}.Keys"/>.
/// </summary>
public class DictionaryWrapperBaseKeysValuesTests
{
	[Fact]
	public void DictionaryWrapper_KeysAndValues_ResolveFromBothPaths()
	{
		// DictionaryWrapper overrides both Keys and KeyCollection (and the Values equivalent), so
		// neither the public property nor the explicit IDictionary<>.Keys/.Values path -- which goes
		// through KeyCollection/ValueCollection -- may recurse.
		var wrapper = new DictionaryWrapper<int, int> { { 1, 100 } };
		IDictionary<int, int> asIDictionary = wrapper;

		wrapper.Keys.Should().Contain(1);
		wrapper.Values.Should().Contain(100);
		asIDictionary.Keys.Should().Contain(1);
		asIDictionary.Values.Should().Contain(100);
	}

	[Fact]
	public void IndexedDictionary_ExplicitIDictionaryKeysAndValues_DoNotRecurse()
	{
		// Keys/Values here don't call KeyCollection/ValueCollection at all, so the base default
		// KeyCollection -- reached only via the explicit IDictionary<> accessors -- calls Keys exactly
		// once and terminates. Not overriding KeyCollection is safe precisely because Keys does not
		// depend on it, which is the opposite direction of the hazard.
		var dictionary = new IndexedDictionary<int, int> { { 1, 100 } };
		IDictionary<int, int> asIDictionary = dictionary;

		asIDictionary.Keys.Should().Contain(1);
		asIDictionary.Values.Should().Contain(100);
	}

	[Fact]
	public void OrderedDictionary_ExplicitIDictionaryKeysAndValues_DoNotRecurse()
	{
		var dictionary = new OrderedDictionary<int, int> { { 1, 100 } };
		IDictionary<int, int> asIDictionary = dictionary;

		asIDictionary.Keys.Should().Contain(1);
		asIDictionary.Values.Should().Contain(100);
	}
}
