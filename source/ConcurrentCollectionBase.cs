using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;
using Open.Disposable;

namespace Open.Collections
{
	public abstract class ConcurrentCollectionBase<T, TCollection> : DisposableBase, ICollection<T>, ICollection
		where TCollection : class, ICollection<T>
	{
		protected ReaderWriterLockSlim Sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.
		protected TCollection InternalSource;

		protected ConcurrentCollectionBase(TCollection source)
		{
            InternalSource = source ?? throw new ArgumentNullException("source");
		}

		#region Implementation of ICollection<T>
		public void Add(T item)
		{
			Sync.Write(() => InternalSource.Add(item));
		}

		public void Clear()
		{
			Sync.Write(() => InternalSource.Clear());
		}

		public bool Contains(T item)
		{
			return Sync.ReadValue(() => InternalSource.Contains(item));
		}

		public bool Remove(T item)
		{
			bool result = false;
			Sync.ReadWriteConditionalOptimized(
				lockType => result = InternalSource.Contains(item),
				() => result = InternalSource.Remove(item));
			return result;
		}

		public int Count => Sync.ReadValue(() => InternalSource.Count);

		public bool IsReadOnly => InternalSource.IsReadOnly;

		public bool IsSynchronized => true;

		public object SyncRoot => throw new NotImplementedException();

		/// <summary>
		/// Specialized ".ToArray()" thread-safe method.
		/// </summary>
		/// <returns>An array of the contents.</returns>
		public T[] Snapshot()
		{
			var result = Sync.ReadValue(() => InternalSource.ToArray());
			return result;
		}

		/// <summary>
		/// Adds all the current items in this collection to the one provided.
		/// </summary>
		/// <param name="to">The collection to add the items to.</param>
		public void Export(ICollection<T> to)
		{
			Sync.Read(() => to.Add(InternalSource));
		}

		#endregion

		#region Dispose
		protected override void OnDispose(bool calledExplicitly)
		{
			Interlocked.Exchange(ref Sync, null).Dispose();
			Interlocked.Exchange(ref InternalSource, null)?.Dispose();
		}

		/// <summary>
		/// Extracts the underlying collection and returns it before disposing of this synchronized wrapper.
		/// </summary>
		/// <returns>The extracted underlying collection.</returns>
		public TCollection DisposeAndExtract()
		{
			using (this)
			{
				return Nullify(ref InternalSource);
			}
		}
		#endregion

		/// <summary>
		/// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
		/// NOT THREAD SAFE.
		/// 
		/// To enumerate safely, call .Snapshot().
		/// </summary>
		/// <returns>An enumerator from the underlying collection.</returns>
		public virtual IEnumerator<T> GetEnumerator()
		{
			return InternalSource.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// A thread-safe ForEach method.
		/// WARNING: If useSnapshot is false, the collection will be unable to be written to while still processing results and dead-locks can occur.
		/// </summary>
		/// <param name="action">The action to be performed per entry.</param>
		/// <param name="useSnapshot">Indicates if a copy of the contents will be used instead locking the collection.</param>
		public void ForEach(Action<T> action, bool useSnapshot = true)
		{
			if(useSnapshot)
			{
				foreach (var value in Snapshot())
					action(value);
			} else
			{
				Sync.Read(() =>
				{
					foreach (var value in InternalSource)
						action(value);
				});
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Sync.Read(() => InternalSource.CopyTo(array, arrayIndex));
		}

		public void CopyTo(Array array, int arrayIndex)
		{
			Sync.Read(() => ((ICollection)InternalSource).CopyTo(array, arrayIndex));
		}

		// Allow for multiple modifications at once.
		public void Write(Action action)
		{
			Sync.Write(action);
		}

        public bool IfContains(T value, Action action)
        {
            bool executed = false;
            Sync.ReadWriteConditionalOptimized(lockType => Contains(value), () => {
                action();
                executed = true;
            });
            return executed;
        }

        public bool IfNotContains(T value, Action action)
        {
            bool executed = false;
            Sync.ReadWriteConditionalOptimized(lockType => !Contains(value), () => {
                action();
                executed = true;
            });
            return executed;
        }


	}
}
