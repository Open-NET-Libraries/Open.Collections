using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncListTests
    : ParallelListTests<ReadWriteSynchronizedList<int>>
{
    public ReadWriteSyncListTests() : base(new())
    {
    }
}
