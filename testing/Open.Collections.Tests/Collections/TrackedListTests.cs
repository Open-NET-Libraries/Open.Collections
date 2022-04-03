using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class TrackedListTests : BasicListTests<TrackedList<int>>
{
    public TrackedListTests() : base(new())
    {
    }
}
