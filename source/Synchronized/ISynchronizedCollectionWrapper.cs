using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized
{
	[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
	public interface ISynchronizedCollectionWrapper<T, out TCollection> : ISynchronizedCollection<T>
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
		/// <param name="condition">Only executes the action if the condition is true.  The condition may be invoked more than once.</param>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		void Modify(Func<bool> condition, Action<TCollection> action);

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
