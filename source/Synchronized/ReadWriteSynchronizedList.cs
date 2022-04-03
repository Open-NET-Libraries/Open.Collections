using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

[ExcludeFromCodeCoverage]
public sealed class ReadWriteSynchronizedList<T> : ReadWriteSynchronizedListWrapper<T>
{
    public ReadWriteSynchronizedList(int capacity = 0)
        : base(new List<T>(capacity)) { }

    public ReadWriteSynchronizedList(IEnumerable<T> collection)
        : base(new List<T>(collection)) { }
}
