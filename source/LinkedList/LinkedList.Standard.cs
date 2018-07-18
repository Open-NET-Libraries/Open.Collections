using System.Collections.Generic;

namespace Open.Collections
{
	public static class LinkedList
	{
		public class Standard<T> : LinkedList<T>, ILinkedList<T>
		{
			public Standard()
			{

			}

			public Standard(IEnumerable<T> initial) : base(initial)
			{

			}
		}
	}
}
