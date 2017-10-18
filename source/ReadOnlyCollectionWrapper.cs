using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;
using Open.Disposable;

namespace Open.Collections
{
	public class ReadOnlyCollectionWrapper<T, TCollection> : DisposableBase, IReadOnlyCollection<T>
		where TCollection : class, ICollection<T>
	{
		protected TCollection InternalSource;

		protected ReadOnlyCollectionWrapper(TCollection source)
		{
			InternalSource = source ?? throw new ArgumentNullException("source");
		}

		#region Implementation of IReadOnlyCollection<T>
		public virtual bool Contains(T item)
		{
			return InternalSource.Contains(item);
		}

		public virtual int Count => InternalSource.Count;

		public virtual bool IsReadOnly => true;

		/// <summary>
		/// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
		/// </summary>
		/// <returns>An enumerator from the underlying collection.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return InternalSource.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			InternalSource.CopyTo(array, arrayIndex);
		}
		#endregion

		/// <summary>
		/// Adds all the current items in this collection to the one provided.
		/// </summary>
		/// <param name="to">The collection to add the items to.</param>
		public virtual void Export(ICollection<T> to)
		{
			to.Add(InternalSource);
		}

		#region Dispose
		protected override void OnDispose(bool calledExplicitly)
		{
			if (calledExplicitly)
			{
				Interlocked.Exchange(ref InternalSource, null)?.Dispose();
			}
		}
		/// <summary>
		/// Extracts the underlying collection and returns it before disposing of this synchronized wrapper.
		/// </summary>
		/// <returns>The extracted underlying collection.</returns>
		public TCollection ExtractAndDispose()
		{
			using (this)
			{
				return Nullify(ref InternalSource);
			}
		}
		#endregion

	}
}
