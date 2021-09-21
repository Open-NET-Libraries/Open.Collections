namespace Open.Collections
{
	public interface IQueue<T>
	{
		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;.Enqueue(T)"/>
		void Enqueue(T item);

		/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue&lt;T&gt;.TryDequeue(out T)"/>
		bool TryDequeue(out T item);

		/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue&lt;T&gt;.TryPeek(out T)"/>
		bool TryPeek(out T item);

		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;.Count"/>
		int Count { get; }

		/// <inheritdoc cref="System.Collections.Generic.Queue&lt;T&gt;.Clear()"/>
		void Clear();
	}
}
