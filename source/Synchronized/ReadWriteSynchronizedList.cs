using System.Collections.Generic;
using Open.Threading;

namespace Open.Collections.Synchronized
{
    public sealed class ReadWriteSynchronizedList<T> : ReadWriteSynchronizedCollectionWrapper<T, List<T>>, IList<T>
	{

		public ReadWriteSynchronizedList() : base(new List<T>()) { }
		public ReadWriteSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }

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
				// Using Sync.Read simply means that "the collection is not changing" so changing a value within the collection is okay.
				Sync.Read(() => InternalSource[index] = value);
			}
		}

		public int IndexOf(T item)
		{
			return Sync.ReadValue(() => InternalSource.IndexOf(item));
		}

		public void Insert(int index, T item)
		{
			Sync.Write(() => InternalSource.Insert(index, item));
		}

		public void RemoveAt(int index)
		{
			Sync.Write(() => InternalSource.RemoveAt(index));
		}

	}
}
