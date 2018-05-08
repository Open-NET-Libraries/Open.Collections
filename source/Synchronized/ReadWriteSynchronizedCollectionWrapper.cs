using Open.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized
{
	public class ReadWriteSynchronizedCollectionWrapper<T, TCollection> : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
		where TCollection : class, ICollection<T>
	{
		protected ReaderWriterLockSlim Sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.

		protected ReadWriteSynchronizedCollectionWrapper(TCollection source) : base(source)
		{
		}

		#region Implementation of ICollection<T>
		public override void Add(T item)
		{
			Sync.Write(() => InternalSource.Add(item));
		}

		public override void Add(T item1, T item2, params T[] items)
		{
			Sync.Write(() =>
			{
				InternalSource.Add(item1);
				InternalSource.Add(item2);
				foreach (var i in items)
					InternalSource.Add(i);
			});
		}

		public override void Add(T[] items)
		{
			Sync.Write(() =>
			{
				foreach (var i in items)
					InternalSource.Add(i);
			});
		}

		public override void Clear()
		{
			Sync.Write(() => InternalSource.Clear());
		}

		public override bool Contains(T item)
		{
			return Sync.ReadValue(() => InternalSource.Contains(item));
		}

		public override bool Remove(T item)
		{
			bool result = false;
			Sync.ReadWriteConditionalOptimized(
				lockType => result = InternalSource.Contains(item),
				() => result = InternalSource.Remove(item));
			return result;
		}

		public override int Count => Sync.ReadValue(() => InternalSource.Count);


		/// <summary>
		/// Specialized ".ToArray()" thread-safe method.
		/// </summary>
		/// <returns>An array of the contents.</returns>
		public T[] Snapshot()
		{
			return Sync.ReadValue(() => InternalSource.ToArray());
		}

		/// <summary>
		/// Adds all the current items in this collection to the one provided.
		/// </summary>
		/// <param name="to">The collection to add the items to.</param>
		public override void Export(ICollection<T> to)
		{
			Sync.Read(() => to.Add(InternalSource));
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			Sync.Read(() => InternalSource.CopyTo(array, arrayIndex));
		}

		#endregion

		#region Dispose
		protected override void OnDispose(bool calledExplicitly)
		{
			if (calledExplicitly)
			{
				Interlocked.Exchange(ref Sync, null)?.Dispose();
			}

			base.OnDispose(calledExplicitly);
		}
		#endregion

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
				Sync.Read(() =>
				{
					foreach (var item in InternalSource)
						action(item);
				});
			}
		}



		public void Modify(Action<TCollection> action)
		{
			Sync.Write(() => action(InternalSource));
		}

		public TResult Modify<TResult>(Func<TCollection, TResult> action)
		{
			return Sync.WriteValue(() => action(InternalSource));
		}

		public virtual bool IfContains(T item, Action<TCollection> action)
		{
			bool executed = false;
			Sync.ReadWriteConditionalOptimized(lockType => InternalSource.Contains(item), () =>
			{
				action(InternalSource);
				executed = true;
			});
			return executed;
		}

		public virtual bool IfNotContains(T item, Action<TCollection> action)
		{
			bool executed = false;
			Sync.ReadWriteConditionalOptimized(lockType => !InternalSource.Contains(item), () =>
			{
				action(InternalSource);
				executed = true;
			});
			return executed;
		}

	}
}
