using System.Collections.Generic;


namespace Open.Collections.Synchronized
{
	public sealed class LockSynchronizedHashSet<T> : LockSynchronizedCollectionWrapper<T,HashSet<T>>, ISet<T>, ISynchronizedCollection<T>
	{
		public LockSynchronizedHashSet() : base(new HashSet<T>()) { }
		public LockSynchronizedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
		public LockSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

		// Asumes that .Contains is a thread-safe read-only operation.
		// But any potentially iterative operation will be locked.

		public override bool Contains(T item)
		{
			return InternalSource.Contains(item);
		}

		public new bool Add(T item)
		{
			return IfNotContains(item, c => c.Add(item));
		}

		public override bool Remove(T item)
		{
			if (!Contains(item)) return false;
			return base.Remove(item);
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			lock (Sync) InternalSource.ExceptWith(other);
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			lock (Sync) InternalSource.IntersectWith(other);
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			lock (Sync) InternalSource.SymmetricExceptWith(other);
		}

		public void UnionWith(IEnumerable<T> other)
		{
			lock (Sync) InternalSource.UnionWith(other);
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.IsProperSubsetOf(other);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.IsProperSupersetOf(other);
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.IsSubsetOf(other);
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.IsSupersetOf(other);
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.Overlaps(other);
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			lock (Sync) return InternalSource.SetEquals(other);
		}

	}
}
