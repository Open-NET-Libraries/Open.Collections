using System.Collections.Generic;

namespace Open.Collections;

public interface ISynchronizedCollection<T> : ICollection<T>
{
	/// <summary>
	/// Specialized ".ToArray()" thread-safe method.
	/// </summary>
	/// <returns>An array of the contents.</returns>
	// ReSharper disable once ReturnTypeCanBeEnumerable.Global
	// ReSharper disable once UnusedMemberInSuper.Global
	T[] Snapshot();

	/// <summary>
	/// Adds all the current items in this collection to the one provided.
	/// </summary>
	/// <param name="to">The collection to add the items to.</param>
	void Export(ICollection<T> to);
}
