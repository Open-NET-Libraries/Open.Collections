using Open.Collections.Synchronized;

namespace Open.Collections.Tests;
public class TrackedDictionaryTests
    : ParallelDictionaryTests<TrackedDictionary<int, int>>
{
}

public class TrackedIndexedDictionaryTests
    : ParallelDictionaryTests<TrackedIndexedDictionary<int, int>>
{
}