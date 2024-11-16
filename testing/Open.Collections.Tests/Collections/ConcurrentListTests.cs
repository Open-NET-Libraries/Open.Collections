using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;

public class ConcurrentListTests : BasicListTests<ConcurrentList<int>>
{
	public ConcurrentListTests() : base([])
	{
	}
}
