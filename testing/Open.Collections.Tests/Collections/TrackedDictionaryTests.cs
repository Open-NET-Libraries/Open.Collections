using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class TrackedDictionaryTests
    : ParallelDictionaryTests<TrackedDictionary<int, int>>
{
}

public class TrackedIndexedDictionaryTests
    : ParallelDictionaryTests<TrackedIndexedDictionary<int, int>>
{
}