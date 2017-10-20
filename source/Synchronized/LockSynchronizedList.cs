using System.Collections.Generic;


namespace Open.Collections.Synchronized
{
	public sealed class LockSynchronizedList<T> : LockSynchronizedCollectionWrapper<T, List<T>>, IList<T>
	{

		public LockSynchronizedList() : base(new List<T>()) { }
		public LockSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }

		// This is a simplified version.
		// It could be possible to allow indexed values to change independently of one another.
		// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.

		public T this[int index]
		{
			get
			{
				return InternalSource[index];
			}

			set
			{
				InternalSource[index] = value;
			}
		}

		public int IndexOf(T item)
		{
			lock (Sync) return InternalSource.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			lock (Sync) InternalSource.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			lock (Sync) InternalSource.RemoveAt(index);
		}

	}
}
