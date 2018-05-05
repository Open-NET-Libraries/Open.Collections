using System.Collections.Generic;

namespace Open.Collections
{
	public interface ISynchronizedCollection<T> : ICollection<T>
	{
		T[] Snapshot();

		void Export(ICollection<T> to);
	}
}
