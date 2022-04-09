using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

[ExcludeFromCodeCoverage]
public sealed class LockSynchronizedList<T>
    : LockSynchronizedListWrapper<T>
{
    public LockSynchronizedList(int capacity = 0) : base(new List<T>(capacity)) { }
    public LockSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }
}
