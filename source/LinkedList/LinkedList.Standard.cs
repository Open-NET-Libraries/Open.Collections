namespace Open.Collections;

/// <summary>
/// The container for <see cref="Standard{T}"/>.
/// </summary>
public static class LinkedList
{
	/// <summary>
	/// A standard <see cref="LinkedList{T}"/> implementation that allows for the <see cref="ILinkedList{T}"/> interface.
	/// </summary>
	/// <remarks>This allows for other linked lists to be used interchangeably.</remarks>
	public sealed class Standard<T> : LinkedList<T>, ILinkedList<T>
	{
		///<inheritdoc />
		[ExcludeFromCodeCoverage]
		public Standard() : base()
		{
		}

		///<inheritdoc />
		[ExcludeFromCodeCoverage]
		public Standard(IEnumerable<T> initial) : base(initial)
		{
		}

		/// <inheritdoc />
		public bool TryTakeFirst(out T item)
		{
			LinkedListNode<T>? node = First;
			if (node is null)
			{
				item = default!;
				return false;
			}

			item = node.Value;
			RemoveFirst();
			return true;
		}

		/// <inheritdoc />
		public bool TryTakeLast(out T item)
		{
			LinkedListNode<T>? node = Last;
			if (node is null)
			{
				item = default!;
				return false;
			}

			item = node.Value;
			RemoveLast();
			return true;
		}
	}
}
