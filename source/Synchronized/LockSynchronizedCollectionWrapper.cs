using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Open.Threading;

namespace Open.Collections.Synchronized
{
	public class LockSynchronizedCollectionWrapper<T, TCollection> : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
		where TCollection : class, ICollection<T>
	{
		protected LockSynchronizedCollectionWrapper(TCollection source) : base(source)
		{
			Sync = source;
		}

		protected object Sync; // Could possibly override..
		public object SyncRoot => Sync;

		#region Implementation of ICollection<T>
		public override void Add(T item)
		{
			lock (Sync) InternalSource.Add(item);
		}

		public override void Add(T item1, T item2, params T[] items)
		{
			lock(items)
			{
				InternalSource.Add(item1);
				InternalSource.Add(item2);
				foreach (var i in items)
					InternalSource.Add(i);
			}
		}

		public override void Add(T[] items)
		{
			lock (items)
			{
				foreach (var i in items)
					InternalSource.Add(i);
			}
		}


		public override void Clear()
		{
			lock (Sync) InternalSource.Clear();
		}

		public override bool Contains(T item)
		{
			lock (Sync) return InternalSource.Contains(item);
		}

		public override bool Remove(T item)
		{
			lock (Sync) return InternalSource.Remove(item);
		}

		#endregion

		/// <summary>
		/// Specialized ".ToArray()" thread-safe method.
		/// </summary>
		/// <returns>An array of the contents.</returns>
		public T[] Snapshot()
		{
			lock (Sync) return this.ToArray();
		}

		/// <summary>
		/// Adds all the current items in this collection to the one provided.
		/// </summary>
		/// <param name="to">The collection to add the items to.</param>
		public override void Export(ICollection<T> to)
		{
			lock (Sync) to.Add(this);
		}

		/// <summary>
		/// A thread-safe ForEach method.
		/// WARNING: If useSnapshot is false, the collection will be unable to be written to while still processing results and dead-locks can occur.
		/// </summary>
		/// <param name="action">The action to be performed per entry.</param>
		/// <param name="useSnapshot">Indicates if a copy of the contents will be used instead locking the collection.</param>
		public void ForEach(Action<T> action, bool useSnapshot = true)
		{
			if (useSnapshot)
			{
				foreach (var item in Snapshot())
					action(item);
			}
			else
			{
				lock (Sync)
				{
					foreach (var item in InternalSource)
						action(item);
				};
			}
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			lock (Sync) InternalSource.CopyTo(array, arrayIndex);
		}

		public void Modify(Action<TCollection> action)
		{
			lock (Sync) action(InternalSource);
		}

		public TResult Modify<TResult>(Func<TCollection, TResult> action)
		{
			lock (Sync) return action(InternalSource);
		}

		public bool IfContains(T item, Action<TCollection> action)
		{
			bool contains = InternalSource.Contains(item);
			if (contains)
			{
				lock (Sync)
				{
					if (contains = InternalSource.Contains(item))
						action(InternalSource);
				}
			}
			return contains;
		}

		public bool IfNotContains(T item, Action<TCollection> action)
		{
			bool notContains = !InternalSource.Contains(item);
			if (notContains)
			{
				lock (Sync)
				{
					if (notContains = !InternalSource.Contains(item))
						action(InternalSource);
				}
			}
			return notContains;
		}

	}
}
