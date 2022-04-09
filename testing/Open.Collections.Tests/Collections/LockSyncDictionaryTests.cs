using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class LockSyncDictionaryTests
    : ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>
{
    public LockSyncDictionaryTests() : base(new())
    {
    }
}

public class LockSyncOrderedDictionaryTests
    : ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>
{
    public LockSyncOrderedDictionaryTests() : base(new())
    {
    }
}