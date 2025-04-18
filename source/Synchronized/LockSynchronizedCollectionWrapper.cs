﻿using Open.Threading;
using System.Collections.Immutable;

namespace Open.Collections.Synchronized;

/// <summary>
/// A disposable synchronized wrapper for a collection.
/// </summary>
public class LockSynchronizedCollectionWrapper<T, TCollection>(
	TCollection source, bool owner = false)
	: CollectionWrapper<T, TCollection>(source, owner), ISynchronizedCollectionWrapper<T, TCollection>
	where TCollection : class, ICollection<T>
{
	/// <inheritdoc />
	protected override void OnBeforeDispose()
	{
		ThreadSafety.Lock(Sync, () => { }, 1000);
		base.OnBeforeDispose();
	}

	#region Implementation of ICollection<T>

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public override void Add(T item)
	{
		lock (Sync) base.Add(item);
	}

	/// <inheritdoc />
#if NET9_0_OR_GREATER
	public override void AddThese(T item1, T item2, params ReadOnlySpan<T> items)
#else
	public override void AddThese(T item1, T item2, params T[] items)
#endif
	{
		lock (Sync)
		{
			AssertIsAlive();
			AddInternal(in item1);
			AddInternal(in item2);
			foreach (T? i in items)
				AddInternal(in i);
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
	public void Read(Action action)
	{
		lock (Sync) action();
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public TResult Read<TResult>(Func<TResult> action)
	{
		lock (Sync) return action();
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
			var source = InternalSource;
			if (!source.Contains(item)) return false;
			action(source);
			return true;
		}
	}

	/// <inheritdoc />
	public virtual bool IfNotContains(T item, Action<TCollection> action)
	{
		lock (Sync)
		{
			var source = InternalSource;
			if (source.Contains(item)) return false;
			action(source);
			return true;
		}
	}
}
