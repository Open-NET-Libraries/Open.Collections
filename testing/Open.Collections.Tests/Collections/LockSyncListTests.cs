using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class LockSyncListTests
    : BasicListTests<LockSynchronizedList<int>>
{
    public LockSyncListTests() : base(new())
    {
    }
}
