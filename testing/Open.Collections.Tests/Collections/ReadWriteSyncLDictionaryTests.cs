using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncDictionaryTests
    : BasicDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
    public ReadWriteSyncDictionaryTests() : base(new())
    {
    }
}

public class ReadWriteSyncOrderedDictionaryTests
    : BasicDictionaryTests<ReadWriteSynchronizedDictionary<int, int>>
{
    public ReadWriteSyncOrderedDictionaryTests() : base(new())
    {
    }
}