using System.Collections.Generic;
using Open.Threading;

namespace Open.Collections
{
    public class ConcurrentHashSet<T> : ConcurrentCollectionBase<T, HashSet<T>>, ISet<T>
	{

		public ConcurrentHashSet() : base(new HashSet<T>()) { }
		public ConcurrentHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

		public new bool Add(T item)
		{
			return Sync.WriteValue(() => InternalSource.Add(item));
		}

        public void ExceptWith(IEnumerable<T> other)
        {
		   Sync.Write(() => InternalSource.ExceptWith(other));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
			Sync.Write(() =>InternalSource.IntersectWith(other));
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

    }
}
