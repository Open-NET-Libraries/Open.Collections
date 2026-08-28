#nullable enable

namespace Open.Collections.Tests.Collections;

public class OrderedDictionaryContainsTests : OrderedDictionaryContainsTests<OrderedDictionary<int, int>>;

public class IndexedDictionaryContainsTests : OrderedDictionaryContainsTests<IndexedDictionary<int, int>>;

public class OrderedDictionaryContainsNullableValueTests
	: OrderedDictionaryContainsNullableValueTests<OrderedDictionary<int, string?>>;

public class IndexedDictionaryContainsNullableValueTests
	: OrderedDictionaryContainsNullableValueTests<IndexedDictionary<int, string?>>;

public class OrderedDictionaryContainsComplexityTests
	: OrderedDictionaryContainsComplexityTests<OrderedDictionary<CountingKey, int>>;

public class IndexedDictionaryContainsComplexityTests
	: OrderedDictionaryContainsComplexityTests<IndexedDictionary<CountingKey, int>>;
