using System.Collections.Generic;

namespace Open.Collections.Synchronized;

public sealed class ReadWriteSynchronizedList<T> : ReadWriteSynchronizedListWrapper<T>
{
	public ReadWriteSynchronizedList()
        : base(new List<T>()) { }
	public ReadWriteSynchronizedList(IEnumerable<T> collection)
        : base(new List<T>(collection)) { }
}
