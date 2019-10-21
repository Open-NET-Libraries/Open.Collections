using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public class LockSynchronizedListWrapper<T> : LockSynchronizedCollectionWrapper<T, IList<T>>, IList<T>
	{

		public LockSynchronizedListWrapper(IList<T> list) : base(list) { }

		// This is a simplified version.
		// It could be possible to allow indexed values to change independently of one another.
		// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.

		/// <inheritdoc />
		public T this[int index]
		{
			// ReSharper disable once InconsistentlySynchronizedField
			get => InternalSource[index];
			// ReSharper disable once InconsistentlySynchronizedField
			set => InternalSource[index] = value;
		}

		/// <inheritdoc />
		public int IndexOf(T item)
		{
			lock (Sync) return InternalSource.IndexOf(item);
		}

		/// <inheritdoc />
		public void Insert(int index, T item)
		{
			lock (Sync) InternalSource.Insert(index, item);
		}

		/// <inheritdoc />
		public void RemoveAt(int index)
		{
			lock (Sync) InternalSource.RemoveAt(index);
		}

	}
}
