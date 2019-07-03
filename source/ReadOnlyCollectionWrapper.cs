using Open.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Open.Collections
{
	public class ReadOnlyCollectionWrapper<T, TCollection> : DisposableBase, IReadOnlyCollection<T>
		where TCollection : class, ICollection<T>
	{
		protected TCollection InternalSource;

		protected ReadOnlyCollectionWrapper(TCollection source)
		{
			InternalSource = source ?? throw new ArgumentNullException(nameof(source));
		}

		#region Implementation of IReadOnlyCollection<T>
		/// <inheritdoc cref="ICollection&lt;T&gt;" />
		public virtual bool Contains(T item)
			=> InternalSource.Contains(item);

		/// <inheritdoc />
		public virtual int Count
			=> InternalSource.Count;

		/// <inheritdoc cref="ICollection&lt;T&gt;" />
		public virtual bool IsReadOnly
			=> true;

		/// <summary>
		/// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
		/// </summary>
		/// <returns>An enumerator from the underlying collection.</returns>
		// ReSharper disable once InheritdocConsiderUsage
		public IEnumerator<T> GetEnumerator()
			=> InternalSource.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		/// <inheritdoc cref="ICollection&lt;T&gt;" />
		public virtual void CopyTo(T[] array, int arrayIndex)
			=> InternalSource.CopyTo(array, arrayIndex);
		#endregion

		/// <inheritdoc cref="ISynchronizedCollection&lt;T&gt;" />
		public virtual void Export(ICollection<T> to)
			=> to.Add(InternalSource);

		#region Dispose
		protected override void OnDispose()
		{
			Nullify(ref InternalSource)?.Dispose();
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
