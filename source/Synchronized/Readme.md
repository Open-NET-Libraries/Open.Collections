# Synchronized Collections

Keeping in mind that without an exclusive lock of some kind, the data can change from one operation to the next.

These classes are provided to help with simple operations that don't rely on lengthy exclusive locking.

Some methods are available that assist with exclusive access:

* `.IfContains(item, action)`
* `.IfNotContains(item, action)`
* `.Snapshot()`
* `.ForEach(action, useSnapshot)`
* `.Export(destination)`
* `.CopyTo(array, arrayIndex)`
* `.Modify(action)`

A "snapshot" is simply a copy of the collection at a given moment that can then be operated on while the underlying collection can continue to mutate.

### Reads

Any operation that the collection cannot be changing (adding/removing) while executing.  
i.e. ```.Contains(item)```

### Writes

Any operation that will change (add/remove an entry in) the collection.

## Lock Synchronized

Most of these classes synchronize access very fast when a simple Monitor (lock) is used properly.

Best to use these classes when expecting a mix of read/write but with a majority of writes.

## Read-Write Synchronized

These classes use a `ReaderWriterLockSlim` to synchronize read-write access and can be faster in some specific cases.

Best to use these classes when expecting a mix of read/write but with a majority of reads and typically of a smaller size.

## Benchmarks

*Important Observations:*

* Be sure to benchmark the 'Release' configuration as the difference in performance versus 'Debug' can be dramatic.
* Observe results on different machines with different core counts.
* Beware as collection sizes increase, performance behavior changes.
