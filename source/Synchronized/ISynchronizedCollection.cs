using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
    interface ISynchronizedCollection<T> : ICollection<T>
    {
		T[] Snapshot();

		void Export(ICollection<T> to);

		/// <summary>
		/// Allows for multiple modifications at once.
		/// </summary>
		/// <param name="action">The action to execute safely.</param>
		void Modify(Action action);

		/// <summary>
		/// Allows for multiple modifications at once.
		/// </summary>
		/// <param name="action">The action to execute safely.</param>
		/// <returns>The result of the action.</returns>
		T Modify(Func<T> action);

		bool Contains(T item, Action<bool> action);

		bool IfContains(T item, Action action);

		bool IfNotContains(T item, Action action);

	}
}
