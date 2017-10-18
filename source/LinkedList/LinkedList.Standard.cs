using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
    public static partial class LinkedList
    {
		public class Standard<T> : System.Collections.Generic.LinkedList<T>, ILinkedList<T>
		{
			public Standard() : base()
			{

			}

			public Standard(IEnumerable<T> initial) : base(initial)
			{

			}
		}
	}
}

