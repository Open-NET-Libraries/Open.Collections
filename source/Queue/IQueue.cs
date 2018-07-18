using System.Diagnostics.CodeAnalysis;

namespace Open.Collections
{
	[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
	public interface IQueue<T>
	{
		void Enqueue(T item);

		bool TryDequeue(out T item);

		bool TryPeek(out T item);

		int Count { get; }

		void Clear();
	}
}
