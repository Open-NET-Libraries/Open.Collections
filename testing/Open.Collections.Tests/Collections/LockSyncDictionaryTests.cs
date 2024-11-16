using Open.Collections.Synchronized;

namespace Open.Collections.Tests.Collections;

public class LockSyncDictionaryTests
	: ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>;

public class LockSyncIndexedDictionaryTests
	: ParallelDictionaryTests<LockSynchronizedDictionary<int, int>>;