using System.Collections.Generic;

namespace Open.Collections
{
	public interface ISynchronizedCollection<T> : ICollection<T>
	{
		// ReSharper disable once UnusedMemberInSuper.Global
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		T[] Snapshot();

		void Export(ICollection<T> to);
	}
}
