using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;
using Open.Disposable;

namespace Open.Collections
{
	public abstract class ConcurrentCollectionBase<T, TCollection> : DisposableBase, ICollection<T>
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

		public int Count
		{
			get
			{
				return Sync.ReadValue(() => InternalSource.Count);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return InternalSource.IsReadOnly;
			}
		}

		public T[] ToArrayDirect()
		{
			var result = Sync.ReadValue(() => InternalSource.ToArray());
			return result;
		}

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

		public TCollection DisposeAndExtract()
		{
			var s = InternalSource;
			InternalSource = null;
			Dispose();
			return s;
		}
		#endregion


		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)this.ToArrayDirect()).GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this.ToArrayDirect()).GetEnumerator();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Sync.Read(() => InternalSource.CopyTo(array, arrayIndex));
		}

		// Allow for multiple modifications at once.
		public void Write(Action action)
		{
			Sync.Write(action);
		}

        public bool IfContains(T value, Action action)
        {
            bool executed = false;
            Sync.ReadWriteConditionalOptimized(lockType => this.Contains(value), () => {
                action();
                executed = true;
            });
            return executed;
        }

        public bool IfNotContains(T value, Action action)
        {
            bool executed = false;
            Sync.ReadWriteConditionalOptimized(lockType => !this.Contains(value), () => {
                action();
                executed = true;
            });
            return executed;
        }

    }
}
