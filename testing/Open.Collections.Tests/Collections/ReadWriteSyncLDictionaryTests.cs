using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class ReadWriteSyncDictionaryTests : BasicDictionaryTests<ReadWriteSynchronizedDictionaryWrapper<int, int>>
{
    public ReadWriteSyncDictionaryTests() : base(new())
    {
    }
}

public class ReadWriteSyncOrderedDictionaryTests : BasicDictionaryTests<ReadWriteSynchronizedDictionaryWrapper<int, int>>
{
    public ReadWriteSyncOrderedDictionaryTests() : base(new(new OrderedDictionary<int, int>()))
    {
    }
}