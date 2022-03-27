using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public class LockSynchronizedList<T> : LockSynchronizedListWrapper<T>
{
    [ExcludeFromCodeCoverage]
    public LockSynchronizedList(int capacity = 0) : base(new List<T>(capacity)) { }
    [ExcludeFromCodeCoverage]
    public LockSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }
}
