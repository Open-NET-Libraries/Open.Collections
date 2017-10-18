using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Open.Threading;

namespace Open.Collections.Synchronized
{
	public class LockSynchronizedCollectionWrapper<T, TCollection> : CollectionWrapper<T, TCollection>, ISynchronizedCollection<T>
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

		public void Modify(Action action)
		{
			lock (Sync) action();
		}

		public T Modify(Func<T> action)
		{
			lock (Sync) return action();
		}


		public bool Contains(T item, Action<bool> action)
		{
			bool contains;
			lock (Sync) action(contains = InternalSource.Contains(item));
			return contains;
		}

		public virtual bool IfContains(T item, Action action)
		{
			bool contains;
			lock(Sync)
			{
				if (contains = InternalSource.Contains(item))
					action();
			}
			return contains;
		}

		public virtual bool IfNotContains(T item, Action action)
		{
			bool notContains;
			lock (Sync)
			{
				if (notContains = !InternalSource.Contains(item))
					action();
			}
			return notContains;
		}

	}
}
