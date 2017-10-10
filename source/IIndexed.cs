using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{

    public interface IIndexed<TKey>
    {
        TKey Key { get; }
    }

    public interface IIndexedValue<TKey, TValue> : IIndexed<TKey>
    {
        TValue Value { get; }
    }


}
