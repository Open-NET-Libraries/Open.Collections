using Open.Threading;
using System.Collections.Generic;

namespace Open.Collections.Synchronized
{
	public sealed class ReadWriteSynchronizedList<T> : ReadWriteSynchronizedCollectionWrapper<T, List<T>>, IList<T>
	{

		public ReadWriteSynchronizedList() : base(new List<T>()) { }
		public ReadWriteSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }

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
