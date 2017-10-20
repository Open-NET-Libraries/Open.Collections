using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;
using Open.Disposable;

namespace Open.Collections
{
	public class CollectionWrapper<T, TCollection> : ReadOnlyCollectionWrapper<T, TCollection>, ICollection<T>
		where TCollection : class, ICollection<T>
	{
		protected CollectionWrapper(TCollection source):base(source)
		{
		}

		#region Implementation of ICollection<T>
		public virtual void Add(T item)
		{
			InternalSource.Add(item);
		}

		public virtual void Add(T item1, T item2, params T[] items)
		{
			InternalSource.Add(item1);
			InternalSource.Add(item2);
			foreach (var i in items)
				InternalSource.Add(i);
		}


		/// <summary>
		/// Adds mutliple items to the collection.
		/// It's important to avoid locking for too long so an array is used to add multiple items.
		/// An enumerable is potentially slow as it may be yielding to a process.
		/// </summary>
		/// <param name="items">The items to add.</param>
		public virtual void Add(T[] items)
		{
			foreach (var i in items)
				InternalSource.Add(i);
		}

		public virtual void Clear()
		{
			InternalSource.Clear();
		}

		public virtual bool Remove(T item)
		{
			return InternalSource.Remove(item);
		}

		public override bool IsReadOnly => InternalSource.IsReadOnly;

		public override void CopyTo(T[] array, int arrayIndex)
		{
			InternalSource.CopyTo(array, arrayIndex);
		}
		#endregion
	}
}
