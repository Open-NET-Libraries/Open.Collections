using Open.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized
{
	public sealed class ReadWriteSynchronizedHashSet<T> : ReadWriteSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
	{
		public ReadWriteSynchronizedHashSet() : base(new HashSet<T>()) { }
		public ReadWriteSynchronizedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
		public ReadWriteSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

		// Asumes that .Contains is a thread-safe read-only operation.
		// But any potentially iterative operation will be locked.

		public override bool Contains(T item)
		{
			return InternalSource.Contains(item);
		}

		public new bool Add(T item)
		{
			return IfNotContains(item, () => base.Add(item));
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			Sync.Write(() => InternalSource.ExceptWith(other));
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			Sync.Write(() => InternalSource.IntersectWith(other));
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.IsProperSubsetOf(other));
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.IsProperSupersetOf(other));
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.IsSubsetOf(other));
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.IsSupersetOf(other));
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.Overlaps(other));
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			return Sync.ReadValue(() => InternalSource.SetEquals(other));
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			Sync.Write(() => InternalSource.SymmetricExceptWith(other));
		}

		public void UnionWith(IEnumerable<T> other)
		{
			Sync.Write(() => InternalSource.UnionWith(other));
		}

		public override bool IfContains(T item, Action action)
		{
			if (!InternalSource.Contains(item)) return false;
			return base.IfContains(item, action);
		}

		public override bool IfNotContains(T item, Action action)
		{
			if (InternalSource.Contains(item)) return false;
			return base.IfNotContains(item, action);
		}
	}

}
