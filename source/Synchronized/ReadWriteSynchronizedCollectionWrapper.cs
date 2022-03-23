using Open.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized;

public class ReadWriteSynchronizedCollectionWrapper<T, TCollection>
    : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
		where TCollection : class, ICollection<T>
{
	protected ReaderWriterLockSlim Sync = new(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.

	protected ReadWriteSynchronizedCollectionWrapper(TCollection source) : base(source)
	{
	}

	#region Implementation of ICollection<T>

	/// <inheritdoc />
	public override void Add(T item)
        => Sync.Write(() => InternalSource.Add(item));

	/// <inheritdoc />
	public override void Add(T item1, T item2, params T[] items)
        => Sync.Write(() =>
		{
			InternalSource.Add(item1);
			InternalSource.Add(item2);
			foreach (T? i in items)
				InternalSource.Add(i);
		});

	/// <inheritdoc />
	public override void AddRange(IEnumerable<T> items)
    {
        if (items is null) return;
        IReadOnlyList<T> enumerable = items switch
        {
            IImmutableList<T> i => i,
            T[] a => a,
            _ => items.ToArray(),
        };

        Sync.Write(() =>
        {
            foreach (var item in enumerable)
                InternalSource.Add(item);
        });
    }

	/// <inheritdoc />
	public override void Clear()
        => Sync.Write(() => InternalSource.Clear());

	/// <inheritdoc />
	public override bool Contains(T item)
        => Sync.Read(() => InternalSource.Contains(item));

    /// <inheritdoc />
    public override bool Remove(T item)
        => Sync.Write(() => InternalSource.Remove(item));

    /// <inheritdoc />
    public override int Count
		=> Sync.Read(() => InternalSource.Count);

	/// <inheritdoc />
	public T[] Snapshot()
        => Sync.Read(() => InternalSource.ToArray());

	/// <inheritdoc />
	public override void Export(ICollection<T> to)
        => Sync.Read(() => to.Add(InternalSource));

	/// <inheritdoc />
	public override void CopyTo(T[] array, int arrayIndex)
        => Sync.Read(() => InternalSource.CopyTo(array, arrayIndex));

	/// <inheritdoc />
	public override Span<T> CopyTo(Span<T> span)
	{
		using ReadLock? read = Sync.ReadLock();
		return InternalSource.CopyToSpan(span);
	}

	#endregion

	#region Dispose
	protected override void OnDispose()
	{
		Nullify(ref Sync)?.Dispose();

		base.OnDispose();
	}
	#endregion

	/// <summary>
	/// A thread-safe ForEach method.
	/// WARNING: If useSnapshot is false, the collection will be unable to be written to while still processing results and dead-locks can occur.
	/// </summary>
	/// <param name="action">The action to be performed per entry.</param>
	/// <param name="useSnapshot">Indicates if a copy of the contents will be used instead locking the collection.</param>
	public void ForEach(Action<T> action, bool useSnapshot = true)
	{
		if (useSnapshot)
		{
			foreach (T? item in Snapshot())
				action(item);
		}
		else
		{
			Sync.Read(() =>
			{
				foreach (T? item in InternalSource)
					action(item);
			});
		}
	}

	/// <inheritdoc />
	public void Modify(Func<bool> condition, Action<TCollection> action)
		=> Sync.ReadWriteConditional(_ => condition(), () => action(InternalSource));

	/// <summary>
	/// Allows for multiple modifications at once.
	/// </summary>
	/// <param name="condition">Only executes the action if the condition is true.  The condition may be invoked more than once.</param>
	/// <param name="action">The action to execute safely on the underlying collection safely.</param>
	public void Modify(Func<bool, bool> condition, Action<TCollection> action)
		=> Sync.ReadWriteConditional(condition, () => action(InternalSource));

	/// <inheritdoc />
	public void Modify(Action<TCollection> action) => Sync.Write(()
		=> action(InternalSource));

	/// <inheritdoc />
	public TResult Modify<TResult>(Func<TCollection, TResult> action)
		=> Sync.Write(() => action(InternalSource));

	/// <inheritdoc />
	public virtual bool IfContains(T item, Action<TCollection> action)
	{
        using var uLock = Sync.UpgradableReadLock();
        if (!InternalSource.Contains(item)) return false;
        using var wLock = Sync.WriteLock();
        action(InternalSource);
        return true;
	}

	/// <inheritdoc />
	public virtual bool IfNotContains(T item, Action<TCollection> action)
	{
        using var uLock = Sync.UpgradableReadLock();
        if (!InternalSource.Contains(item)) return false;
        using var wLock = Sync.WriteLock();
        action(InternalSource);
        return true;
	}
}
