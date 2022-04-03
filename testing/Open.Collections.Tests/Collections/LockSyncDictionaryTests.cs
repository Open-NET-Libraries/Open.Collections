using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class LockSyncDictionaryTests : BasicDictionaryTests<LockSynchronizedDictionaryWrapper<int, int>>
{
    public LockSyncDictionaryTests() : base(new())
    {
    }
}

public class LockSyncOrderedDictionaryTests : BasicDictionaryTests<LockSynchronizedDictionaryWrapper<int, int>>
{
    public LockSyncOrderedDictionaryTests() : base(new(new OrderedDictionary<int, int>()))
    {
    }
}