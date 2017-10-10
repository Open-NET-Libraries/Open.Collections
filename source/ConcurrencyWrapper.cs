using System.Collections.Generic;

namespace Open.Collections
{
    public sealed class ConcurrencyWrapper<T, TCollection> : ConcurrentCollectionBase<T, TCollection>
		where TCollection : class, ICollection<T>
	{

		public ConcurrencyWrapper(TCollection source) : base(source) { }

		public TCollection Source
		{
			get {
				return InternalSource;
			} 
		}

	}
}
