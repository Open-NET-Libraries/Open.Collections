using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class LockSyncLinkedListTests : BasicCollectionTests<LockSynchronizedLinkedList<int>>
{
    public LockSyncLinkedListTests() : base(new LockSynchronizedLinkedList<int>())
    {
    }
}
