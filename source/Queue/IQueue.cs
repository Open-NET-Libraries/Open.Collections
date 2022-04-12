namespace Open.Collections;

public interface IQueue<T>
{
	/// <inheritdoc cref="System.Collections.Generic.Queue{T}.Enqueue(T)"/>
	void Enqueue(T item);

	/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue{T}.TryDequeue(out T)"/>
	bool TryDequeue(out T item);

	/// <inheritdoc cref="System.Collections.Concurrent.ConcurrentQueue{T}.TryPeek(out T)"/>
	bool TryPeek(out T item);

	/// <inheritdoc cref="System.Collections.Generic.Queue{T}.Count"/>
	int Count { get; }

	/// <inheritdoc cref="System.Collections.Generic.Queue{T}.Clear()"/>
	void Clear();
}
