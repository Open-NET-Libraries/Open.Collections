using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections;

public static class LinkedList
{
	public sealed class Standard<T> : LinkedList<T>, ILinkedList<T>
	{
		[ExcludeFromCodeCoverage]
		public Standard()
		{
		}

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
