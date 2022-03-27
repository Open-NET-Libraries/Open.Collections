using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncLinkedListTests : BasicCollectionTests<LockSynchronizedLinkedList<int>>
{
    public ReadWriteSyncLinkedListTests() : base(new LockSynchronizedLinkedList<int>())
    {
    }
}
