using System;
using System.Collections.Generic;

namespace Open.Collections;

public interface ISynchronizedCollection<T> : ICollection<T>
{
    /// <summary>
    /// Provides temporary exclusive read access while the action is being executed.
    /// </summary>
    /// <param name="action">The action to execute safely on the underlying collection safely.</param>
    void Read(Action action);

    /// <returns>The result of the action.</returns>
    /// <inheritdoc cref="Read(Action)"/>
    TResult Read<TResult>(Func<TResult> action);

    /// <summary>
    /// Specialized ".ToArray()" thread-safe method.
    /// </summary>
    /// <returns>An array of the contents.</returns>
    T[] Snapshot();

	/// <summary>
	/// Adds all the current items in this collection to the one provided.
	/// </summary>
	/// <param name="to">The collection to add the items to.</param>
	void Export(ICollection<T> to);
}
