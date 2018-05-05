using System;
using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public interface ISynchronizedCollectionWrapper<T, TCollection> : ISynchronizedCollection<T>
		where TCollection : ICollection<T>
	{
		/// <summary>
		/// Allows for multiple modifications at once.
		/// </summary>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		void Modify(Action<TCollection> action);

		/// <summary>
		/// Allows for multiple modifications at once.
		/// </summary>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		/// <returns>The result of the action.</returns>
		TResult Modify<TResult>(Func<TCollection, TResult> action);

		/// <summary>
		/// If the item is within, allows locks the collection before executing the action.
		/// </summary>
		/// <param name="item">The item to look for.</param>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		/// <returns>True if the action was executed.</returns>
		bool IfContains(T item, Action<TCollection> action);

		/// <summary>
		/// If the item is not within, allows locks the collection before executing the action.
		/// </summary>
		/// <param name="item">The item to look for.</param>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		/// <returns>True if the action was executed.</returns>
		bool IfNotContains(T item, Action<TCollection> action);
	}
}
