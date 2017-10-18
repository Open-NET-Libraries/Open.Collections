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
