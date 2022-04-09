using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class TrackedDictionaryTests
    : BasicDictionaryTests<TrackedDictionary<int, int>>
{
    public TrackedDictionaryTests() : base(new())
    {
    }
}

public class TrackedOrderedDictionaryTests
    : BasicDictionaryTests<TrackedOrderedDictionary<int, int>>
{
    public TrackedOrderedDictionaryTests() : base(new())
    {
    }
}