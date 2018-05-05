using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
	public interface IQueue<T>
	{
		void Enqueue(T item);

		bool TryDequeue(out T item);

		bool TryPeek(out T item);

		int Count { get; }

		void Clear();
	}
}
