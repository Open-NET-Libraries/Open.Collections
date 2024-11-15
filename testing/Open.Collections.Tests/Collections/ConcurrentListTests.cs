using Open.Collections.Synchronized;

namespace Open.Collections.Tests;

public class ConcurrentListTests : BasicListTests<ConcurrentList<int>>
{
	public ConcurrentListTests() : base([])
	{
	}
}
