using Open.Collections.Synchronized;

namespace Open.Collections.Tests;
public class ReadWriteSyncLinkedListTests
	: BasicLinkedListTests<ReadWriteSynchronizedLinkedList<int>>
{
	public ReadWriteSyncLinkedListTests() : base([])
	{
	}
}
