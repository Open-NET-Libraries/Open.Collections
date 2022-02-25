using System.Collections.Generic;

namespace Open.Collections.Synchronized;

public sealed class LockSynchronizedList<T> : LockSynchronizedListWrapper<T>
{
	public LockSynchronizedList() : base(new List<T>()) { }
	public LockSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }
}
