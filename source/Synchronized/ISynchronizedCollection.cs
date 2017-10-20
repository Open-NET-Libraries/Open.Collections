using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
	public interface ISynchronizedCollection<T> : ICollection<T>
	{
		T[] Snapshot();

		void Export(ICollection<T> to);
	}
}
