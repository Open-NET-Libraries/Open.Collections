using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncLinkedListTests
    : BasicLinkedListTests<ReadWriteSynchronizedLinkedList<int>>
{
    public ReadWriteSyncLinkedListTests() : base(new())
    {
    }
}
