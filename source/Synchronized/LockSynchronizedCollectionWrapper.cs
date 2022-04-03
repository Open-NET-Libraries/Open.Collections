using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Open.Collections.Synchronized;

public class LockSynchronizedCollectionWrapper<T, TCollection> : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
		where TCollection : class, ICollection<T>
{
    protected LockSynchronizedCollectionWrapper(TCollection source, bool owner = false)
        : base(source, owner) { }

    #region Implementation of ICollection<T>

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override void Add(T item)
	{
		lock (Sync) base.Add(item);
	}

    /// <inheritdoc />
    [SuppressMessage("Roslynator", "RCS1235:Optimize method call.")]
    public override void AddThese(T item1, T item2, params T[] items)
    {
        lock (Sync)
        {
            base.Add(item1);
            base.Add(item2);
            foreach (T? i in items)
                base.Add(i);
        }
    }

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

        if (enumerable.Count == 0)
            return;

        lock (Sync) base.AddRange(items);
	}

	/// <inheritdoc />
	public override void Clear()
	{
		lock (Sync) base.Clear();
	}

	/// <inheritdoc  />
	public override bool Contains(T item)
	{
		lock (Sync) return base.Contains(item);
	}

	/// <inheritdoc />
	public override bool Remove(T item)
	{
		lock (Sync) return base.Remove(item);
	}

    #endregion

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public T[] Snapshot()
	{
		lock (Sync) return this.ToArray();
	}

    /// <inheritdoc cref="CollectionWrapper&lt;T, TCollection&gt;" />
    [ExcludeFromCodeCoverage]
    public override void Export(ICollection<T> to)
	{
		lock (Sync) to.AddRange(this);
	}

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
			foreach (var item in Snapshot())
				action(item);
		}
		else
		{
			lock (Sync)
			{
				foreach (var item in InternalSource)
					action(item);
			}
		}
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override void CopyTo(T[] array, int arrayIndex)
	{
		lock (Sync) base.CopyTo(array, arrayIndex);
	}

    /// <summary>
    /// Copies the results to the provided span up to its length or until the end of the results.
    /// </summary>
    /// <returns>
    /// A span representing the results.
    /// If the count was less than the target length, a new span representing the results.
    /// Otherwise the target is returned.
    /// </returns>
    [ExcludeFromCodeCoverage]
    public override Span<T> CopyTo(Span<T> span)
	{
		lock (Sync) return base.CopyTo(span);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Modify(Action<TCollection> action)
	{
		lock (Sync) action(InternalSource);
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Modify(Func<bool> condition, Action<TCollection> action)
	{
		if (!condition()) return;
		lock (Sync)
		{
			if (!condition()) return;
			action(InternalSource);
		}
	}

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public TResult Modify<TResult>(Func<TCollection, TResult> action)
	{
		lock (Sync) return action(InternalSource);
	}

	/// <inheritdoc />
	public virtual bool IfContains(T item, Action<TCollection> action)
	{
		lock (Sync)
		{
            if (!InternalSource.Contains(item)) return false;
            action(InternalSource);
            return true;
		}
	}

	/// <inheritdoc />
	public virtual bool IfNotContains(T item, Action<TCollection> action)
	{
		lock (Sync)
		{
            if (InternalSource.Contains(item)) return false;
            action(InternalSource);
            return true;
        }
    }
}
