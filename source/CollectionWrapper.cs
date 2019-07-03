using System.Collections.Generic;

namespace Open.Collections
{
	public class CollectionWrapper<T, TCollection> : ReadOnlyCollectionWrapper<T, TCollection>, ICollection<T>
		where TCollection : class, ICollection<T>
	{
		protected CollectionWrapper(TCollection source) : base(source)
		{
		}

		#region Implementation of ICollection<T>

		/// <inheritdoc />
		public virtual void Add(T item)
			=> InternalSource.Add(item);

		/// <inheritdoc cref="ICollection&lt;T&gt;" />
		/// <param name="item1">First item to add.</param>
		/// <param name="item2">Additional item to add.</param>
		/// <param name="items">Extended param items to add.</param>
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

		/// <inheritdoc />
		public virtual void Clear()
			=> InternalSource.Clear();

		/// <inheritdoc />
		public virtual bool Remove(T item)
			=> InternalSource.Remove(item);

		/// <inheritdoc cref="ReadOnlyCollectionWrapper&lt;T, TCollection&gt;" />
		public override bool IsReadOnly
			=> InternalSource.IsReadOnly;

		/// <inheritdoc cref="ReadOnlyCollectionWrapper&lt;T, TCollection&gt;" />
		public override void CopyTo(T[] array, int arrayIndex)
			=> InternalSource.CopyTo(array, arrayIndex);
		#endregion
	}
}
