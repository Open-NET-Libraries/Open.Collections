using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;
public class LockSyncDictionaryTests
    : BasicDictionaryTests<LockSynchronizedDictionary<int, int>>
{
    public LockSyncDictionaryTests() : base(new())
    {
    }
}

public class LockSyncOrderedDictionaryTests
    : BasicDictionaryTests<LockSynchronizedDictionary<int, int>>
{
    public LockSyncOrderedDictionaryTests() : base(new())
    {
    }
}