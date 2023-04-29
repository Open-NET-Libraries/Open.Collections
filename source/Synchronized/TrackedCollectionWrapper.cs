using Open.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Open.Collections.Synchronized;

public class TrackedCollectionWrapper<T, TCollection>
	: ModificationSynchronizedBase, ICollection<T>,
	IAddMultiple<T>,
	ISynchronizedCollectionWrapper<T, TCollection>
	where TCollection : class, ICollection<T>
{
	protected TCollection? InternalUnsafeSource;
	protected TCollection InternalSource
		=> InternalUnsafeSource ?? throw new ObjectDisposedException(GetType().ToString());

	/// <summary>
	/// Event fired after a change or group of changes has been made.
	/// </summary>
	public event EventHandler? Modified;

	/// <summary>
	/// Event fired after an item has been added or removed from the collection.
	/// </summary>
	public event EventHandler<ItemChangedEventArgs<T>>? Changed;

	protected bool HasChangedListeners => Changed is not null;

	/// <summary>
	/// Event fired after the collection has been cleared.
	/// </summary>
	public event EventHandler<int>? Cleared;

	[ExcludeFromCodeCoverage]
	public TrackedCollectionWrapper(TCollection collection, ModificationSynchronizer? sync = null)
		: base(sync) => InternalUnsafeSource = collection ?? throw new ArgumentNullException(nameof(collection));

	[ExcludeFromCodeCoverage]
	public TrackedCollectionWrapper(TCollection collection, out ModificationSynchronizer sync)
		: base(out sync) => InternalUnsafeSource = collection ?? throw new ArgumentNullException(nameof(collection));

	[ExcludeFromCodeCoverage]
	protected override ModificationSynchronizer InitSync(object? sync = null)
	{
		_syncOwned = true;
		return new ReadWriteModificationSynchronizer(sync as ReaderWriterLockSlim);
	}

	[ExcludeFromCodeCoverage]
	protected override void OnDispose()
	{
		base.OnDispose();
		Nullify(ref InternalUnsafeSource!); // Eliminate risk from wrapper.
		Modified = null;
		Changed = null;
		Cleared = null;
	}

	private void ThrowIfDisposedInternal() => base.AssertIsAlive();

	private Action? _throwIfDisposed;
	protected Action ThrowIfDisposedDelegate
		=> _throwIfDisposed ??= ThrowIfDisposedInternal;

	/// <inheritdoc />
	public int Count
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			return InternalSource.Count;
		});

	[ExcludeFromCodeCoverage]
	protected virtual void AddInternal(T item)
	   => InternalSource.Add(item);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void OnModified()
		=> Modified?.Invoke(this, EventArgs.Empty);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void OnChanged(ItemChange change, T item, int version)
		=> Changed?.Invoke(this, change.CreateArgs(item, version));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void OnChanged<TIndex>(ItemChange change, TIndex index, T item, int version)
		=> Changed?.Invoke(this, change.CreateArgs(index, item, version));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnAdded(T item, int version)
		=> OnChanged(ItemChange.Added, item, version);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnRemoved(T item, int version)
		=> OnChanged(ItemChange.Removed, item, version);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void OnCleared(int version)
		=> Cleared?.Invoke(this, version);

	/// <inheritdoc />
	public void Add(T item)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() =>
			{
				AddInternal(item);
				return true;
			},
			version => OnAdded(item, version));

	/// <inheritdoc cref="IAddMultiple{T}.AddThese(T, T, T[])" />
	public void AddThese(T item1, T item2, params T[] items)
		=> Sync!.Modifying(AssertIsAliveDelegate,
			() =>
			{
				AddInternal(item1);
				AddInternal(item2);
				foreach (T? i in items)
					AddInternal(i);
				return true;
			},
			version =>
			{
				if (Changed is null) return;
				OnAdded(item1, version);
				OnAdded(item2, version);
				foreach (T? i in items)
					OnAdded(i, version);
			});

	/// <inheritdoc />
	public void AddRange(IEnumerable<T> items)
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

		Sync!.Modifying(AssertIsAliveDelegate, () =>
		{
			foreach (var item in enumerable)
				AddInternal(item);
			return true;
		},
		version =>
		{
			foreach (var item in enumerable)
				OnAdded(item, version);
		});
	}

	[ExcludeFromCodeCoverage]
	protected virtual void ClearInternal()
		=> InternalUnsafeSource!.Clear();

	/// <inheritdoc />
	public void Clear()
		=> Sync!.Modifying(
		() => InternalSource.Count != 0,
		() =>
		{
			int count = InternalUnsafeSource!.Count;
			bool hasItems = count != 0;
			if (hasItems) ClearInternal();
			return hasItems;
		}, OnCleared);

	/// <inheritdoc />
	public bool Contains(T item)
		=> Sync!.Reading(() => InternalSource.Contains(item));

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
		=> Sync!.Reading(() => InternalSource.CopyTo(array, arrayIndex));

	/// <inheritdoc />
	public virtual bool Remove(T item)
		=> Sync!.Modifying(
			AssertIsAliveDelegate,
			() => InternalUnsafeSource!.Remove(item),
			version => OnRemoved(item, version));

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
		=> InternalSource.GetEnumerator().Preflight(ThrowIfDisposedDelegate);

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc />
	public void Read(Action action)
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			action();
		});

	/// <inheritdoc />
	public TResult Read<TResult>(Func<TResult> action)
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			return action();
		});

	/// <inheritdoc />
	public void Modify(Action<TCollection> action)
		 => Sync!.Modifying(AssertIsAliveDelegate, () =>
		 {
			 action(InternalUnsafeSource!);
			 return true;
		 });

	/// <inheritdoc />
	public void Modify(Func<bool> condition, Action<TCollection> action)
		 => Sync!.Modifying(() =>
		 {
			 AssertIsAlive();
			 return condition();
		 }, () =>
		 {
			 action(InternalUnsafeSource!);
			 return true;
		 });

	/// <inheritdoc />
	public TResult Modify<TResult>(Func<TCollection, TResult> action)
	{
		TResult result = default!;
		Sync!.Modifying(AssertIsAliveDelegate, () =>
		{
			result = action(InternalUnsafeSource!);
			return true;
		});
		return result;
	}

	/// <inheritdoc />
	public virtual bool IfContains(T item, Action<TCollection> action)
		=> Sync!.Modifying(
			() => InternalSource.Contains(item),
			() =>
			{
				action(InternalUnsafeSource!);
				return true;
			});

	/// <inheritdoc />
	public virtual bool IfNotContains(T item, Action<TCollection> action)
		=> Sync!.Modifying(
			() => !InternalSource.Contains(item),
			() =>
			{
				action(InternalUnsafeSource!);
				return true;
			});

	/// <inheritdoc />
	public T[] Snapshot()
		=> Sync!.Reading(() => InternalSource.ToArray());

	public void Export(ICollection<T> to)
		=> Sync!.Reading(() => to.AddRange(InternalSource));
}
