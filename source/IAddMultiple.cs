using System.Collections.Generic;

namespace Open.Collections;
public interface IAddMultiple<T>
{
    /// <summary>Adds more than one item.</summary>
    /// <param name="item1">First item to add.</param>
    /// <param name="item2">Additional item to add.</param>
    /// <param name="items">Extended param items to add.</param>
    void AddThese(T item1, T item2, params T[] items);

    /// <summary>
    /// Adds all the items in <paramref name="items"/> to this collection.
    /// </summary>
    /// <param name="items">The items to add.</param>
    void AddRange(IEnumerable<T> items);
}
