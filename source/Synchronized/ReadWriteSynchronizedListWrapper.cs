using Open.Threading;
using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public class ReadWriteSynchronizedListWrapper<T> : ReadWriteSynchronizedCollectionWrapper<T, IList<T>>, IList<T>
	{
		public ReadWriteSynchronizedListWrapper(IList<T> list) : base(list) { }

		// This is a simplified version.
		// It could be possible to allow indexed values to change independently of one another.
		// If that fine grained of read-write control is necessary, then use the ThreadSafety utility and extensions.

		/// <inheritdoc />
		public T this[int index]
		{
			get => InternalSource[index];
			set => InternalSource[index] = value;
		}

		/// <inheritdoc />
		public int IndexOf(T item)
			=> Sync.ReadValue(() => InternalSource.IndexOf(item));

		/// <inheritdoc />
		public void Insert(int index, T item)
			=> Sync.Write(() => InternalSource.Insert(index, item));

		/// <inheritdoc />
		public void RemoveAt(int index)
			=> Sync.Write(() => InternalSource.RemoveAt(index));

	}
}
