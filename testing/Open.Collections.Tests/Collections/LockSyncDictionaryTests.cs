using Open.Collections.Synchronized;

namespace Open.Collections.Tests;
public class LockSyncDictionaryTests
    : ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>
{
}

public class LockSyncIndexedDictionaryTests
    : ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>
{
}