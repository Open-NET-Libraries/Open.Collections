using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Open.Collections;
internal sealed class ConcurrentHashSetInternal<TKey>
    : ConcurrentDictionary<TKey, bool>, ISet<TKey>
{
    int ICollection<TKey>.Count => Count;
    bool ICollection<TKey>.IsReadOnly => false;
    bool ISet<TKey>.Add(TKey item) => TryAdd(item, true);
    void ICollection<TKey>.Add(TKey item) => throw new NotImplementedException();
    void ICollection<TKey>.Clear() => Clear();
    bool ICollection<TKey>.Contains(TKey item) => ContainsKey(item);
    void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex) => throw new NotImplementedException();
    void ISet<TKey>.ExceptWith(IEnumerable<TKey> other) => throw new NotImplementedException();
    IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => throw new NotImplementedException();
    void ISet<TKey>.IntersectWith(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ISet<TKey>.IsProperSubsetOf(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ISet<TKey>.IsProperSupersetOf(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ISet<TKey>.IsSubsetOf(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ISet<TKey>.IsSupersetOf(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ISet<TKey>.Overlaps(IEnumerable<TKey> other) => throw new NotImplementedException();
    bool ICollection<TKey>.Remove(TKey item) => TryRemove(item, out _);
    bool ISet<TKey>.SetEquals(IEnumerable<TKey> other) => throw new NotImplementedException();
    void ISet<TKey>.SymmetricExceptWith(IEnumerable<TKey> other) => throw new NotImplementedException();
    void ISet<TKey>.UnionWith(IEnumerable<TKey> other) => throw new NotImplementedException();
}
