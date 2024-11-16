using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;

public class LockSyncLinkedListTests
	: BasicLinkedListTests<LockSynchronizedLinkedList<int>>
{
	public LockSyncLinkedListTests() : base([])
	{
	}
}
