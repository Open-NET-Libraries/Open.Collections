using Open.Collections.Synchronized;

namespace Open.Collections.Tests;

public class LockSyncLinkedListTests
	: BasicLinkedListTests<LockSynchronizedLinkedList<int>>
{
	public LockSyncLinkedListTests() : base([])
	{
	}
}
