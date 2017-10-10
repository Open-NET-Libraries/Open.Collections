using System.Collections.Generic;
using Open.Threading;

namespace Open.Collections
{
    public class ConcurrentList<T> : ConcurrentCollectionBase<T, List<T>>, IList<T>
	{

		public ConcurrentList() : base(new List<T>()) { }
		public ConcurrentList(IEnumerable<T> collection) : base(new List<T>(collection)) { }

		// This is a simplified version.
		// It could be possible to allow indexed values to change independently of one another.
		// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.
		public T this[int index]
		{
			get
			{
				return Sync.ReadValue(() => InternalSource[index]);
			}

			set
			{
				Sync.Write(() => InternalSource[index] = value);
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
