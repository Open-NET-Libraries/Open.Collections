using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class TrackedDictionaryTests
    : ParallelDictionaryTests<TrackedDictionary<int, int>>
{
    public TrackedDictionaryTests()
        : base(new (new Threading.ReadWriteModificationSynchronizer()))
    {
    }
}

public class TrackedOrderedDictionaryTests
    : ParallelDictionaryTests<TrackedOrderedDictionary<int, int>>
{
    public TrackedOrderedDictionaryTests()
        : base(new(new Threading.SimpleLockingModificationSynchronizer()))
    {
    }
}