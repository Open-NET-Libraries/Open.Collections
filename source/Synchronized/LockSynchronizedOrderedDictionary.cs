namespace Open.Collections.Synchronized;

/// <inheritdoc />
public class LockSynchronizedOrderedDictionary<TKey, TValue>
    : LockSynchronizedDictionaryWrapper<TKey, TValue, OrderedDictionary<TKey, TValue>>, IOrderedDictionary<TKey, TValue>    
{
    /// <inheritdoc />
    public LockSynchronizedOrderedDictionary(int capacity = 0) : base(new OrderedDictionary<TKey, TValue>(capacity)) { }

    /// <inheritdoc />
    public TKey GetKeyAt(int index) => InternalSource.GetKeyAt(index);

    /// <inheritdoc />
    public TValue GetValueAt(int index) => InternalSource.GetValueAt(index);

    /// <inheritdoc />
    public void Insert(int index, TKey key, TValue value)
    {
        lock(Sync) InternalSource.Insert(index, key, value);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        lock (Sync) InternalSource.RemoveAt(index);
    }

    /// <inheritdoc />
    public int SetValue(TKey key, TValue value)
    {
        lock (Sync) return InternalSource.SetValue(key, value);
    }

    /// <inheritdoc />
    public bool SetValueAt(int index, TValue value)
    {
        lock (Sync) return InternalSource.SetValueAt(index, value);
    }

    /// <inheritdoc />
    public new int Add(TKey key, TValue value)
    {
        lock (Sync) return InternalSource.Add(key, value);
    }
}
