using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncListTests : BasicListTests<ReadWriteSynchronizedList<int>>
{
    public ReadWriteSyncListTests() : base(new())
    {
    }
}
