using Open.Collections.Synchronized;

namespace Open.Collections.Tests;
public class ReadWriteSyncDictionaryTests
    : ParallelDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
}

public class ReadWriteSyncIndexedDictionaryTests
    : ParallelDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
}