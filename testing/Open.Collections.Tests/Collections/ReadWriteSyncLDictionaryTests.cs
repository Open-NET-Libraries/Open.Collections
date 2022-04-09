using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncDictionaryTests
    : ParallelDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
    public ReadWriteSyncDictionaryTests() : base(new())
    {
    }
}

public class ReadWriteSyncOrderedDictionaryTests
    : ParallelDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
    public ReadWriteSyncOrderedDictionaryTests() : base(new())
    {
    }
}