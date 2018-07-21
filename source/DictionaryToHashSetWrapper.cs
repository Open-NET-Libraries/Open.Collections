using System.Collections;
using System.Collections.Generic;

namespace Open.Collections
{
	public class DictionaryToHashSetWrapper<T> : ISet<T>
	{
		protected readonly IDictionary<T, bool> InternalSource;

		// ReSharper disable once MemberCanBeProtected.Global
		public DictionaryToHashSetWrapper(IDictionary<T, bool> source)
		{
			InternalSource = source;
		}

		public int Count
			=> InternalSource.Count;

		public bool IsReadOnly
			=> InternalSource.IsReadOnly;

		public virtual bool Add(T item)
		{
			if (InternalSource.ContainsKey(item))
				return false;

			try
			{
				InternalSource.Add(item, true);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public bool Remove(T item)
			// ReSharper disable once AssignNullToNotNullAttribute
			=> InternalSource.Remove(item);

		public void Clear()
			=> InternalSource.Clear();

		public bool Contains(T item)
			// ReSharper disable once AssignNullToNotNullAttribute
			=> InternalSource.ContainsKey(item);

		public void CopyTo(T[] array, int arrayIndex)
			=> InternalSource.Keys.CopyTo(array, arrayIndex);

		public HashSet<T> ToHashSet()
			=> new HashSet<T>(InternalSource.Keys);

		public IEnumerator<T> GetEnumerator()
			=> InternalSource.Keys.GetEnumerator();

		public void ExceptWith(IEnumerable<T> other)
		{
			foreach (var e in other) Remove(e);
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			foreach (var e in other)
			{
				if (!InternalSource.ContainsKey(e))
					Remove(e);
			}
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
			=> ToHashSet().IsProperSubsetOf(other);

		public bool IsProperSupersetOf(IEnumerable<T> other)
			=> ToHashSet().IsProperSupersetOf(other);

		public bool IsSubsetOf(IEnumerable<T> other)
			=> ToHashSet().IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<T> other)
			=> ToHashSet().IsSupersetOf(other);

		public bool Overlaps(IEnumerable<T> other)
			=> ToHashSet().Overlaps(other);

		public bool SetEquals(IEnumerable<T> other)
			=> ToHashSet().SetEquals(other);

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			foreach (var e in other)
			{
				if (InternalSource.ContainsKey(e))
					Add(e);
				else
					Remove(e);
			}
		}

		public void UnionWith(IEnumerable<T> other)
		{
			foreach (var e in other) Add(e);
		}

		void ICollection<T>.Add(T item)
			// ReSharper disable once AssignNullToNotNullAttribute
			=> Add(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
