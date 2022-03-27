using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

public sealed class ReadWriteSynchronizedList<T> : ReadWriteSynchronizedListWrapper<T>
{
    [ExcludeFromCodeCoverage]
    public ReadWriteSynchronizedList(int capacity = 0)
        : base(new List<T>(capacity)) { }

    [ExcludeFromCodeCoverage]
    public ReadWriteSynchronizedList(IEnumerable<T> collection)
        : base(new List<T>(collection)) { }
}
