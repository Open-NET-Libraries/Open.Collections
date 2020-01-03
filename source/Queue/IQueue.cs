namespace Open.Collections
{
	public interface IQueue<T>
	{
		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;"/>
		void Enqueue(T item);

		/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue&lt;T&gt;"/>
		/// <returns>True if an element was removed and returned from the beginning of the queue successfully; otherwise, false.</returns>
		bool TryDequeue(out T item);

		/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue&lt;T&gt;"/>
		/// <summary>
		/// Tries to return an object from the beginning of the queue without removing it.
		/// </summary>
		/// <param name="item">When this method returns, result contains an object from the beginning of the queue or an unspecified value if the operation failed.</param>
		bool TryPeek(out T item);

		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;"/>
		int Count { get; }

		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;"/>
		void Clear();
	}
}
