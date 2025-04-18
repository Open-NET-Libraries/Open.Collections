namespace Open.Collections;

/// <summary>
/// Represents a collection that can add multiple items.
/// </summary>
public interface IAddMultiple<T>
{
	/// <summary>
	/// Adds all the items in <paramref name="items"/> to this collection.
	/// </summary>
	/// <param name="items">The items to add.</param>
	void AddRange(IEnumerable<T> items);

	// Note: "AddThese" is the name because Add can have multiple signatures.

	/// <summary>Adds more than one item.</summary>
	/// <param name="item1">First item to add.</param>
	/// <param name="item2">Additional item to add.</param>
	/// <param name="items">Extended param items to add.</param>
#if NET9_0_OR_GREATER
	void AddThese(T item1, T item2, params System.ReadOnlySpan<T> items);

	/// <inheritdoc cref="AddRange(IEnumerable{T})"/>/>
	[OverloadResolutionPriority(1)]
	void AddRange(ReadOnlySpan<T> items);
#else
	void AddThese(T item1, T item2, params T[] items);
#endif
}
