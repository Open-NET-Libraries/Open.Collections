﻿using Open.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized;

public class TrackedList<T> : ModificationSynchronizedBase, IList<T>
{
	protected List<T> _source = new();

	public TrackedList(ModificationSynchronizer? sync = null) : base(sync)
	{
	}

	public TrackedList(out ModificationSynchronizer sync) : base(out sync)
	{
	}

	protected override ModificationSynchronizer InitSync(object? sync = null)
	{
		_syncOwned = true;
		return new ReadWriteModificationSynchronizer(sync as ReaderWriterLockSlim);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Nullify(ref _source)?.Clear();
	}

	/// <inheritdoc />
	public T this[int index]
	{
		get => Sync!.Reading(() =>
		{
			AssertIsAlive();
			return _source[index];
		});

		set => SetValue(index, value);
	}

	public bool SetValue(int index, T value) => Sync!.Modifying(() => AssertIsAlive(), () => SetValueInternal(index, value));

	private bool SetValueInternal(int index, T value)
	{
        bool changing = index >= _source.Count || !(_source[index]?.Equals(value) ?? value is null);
		if (changing)
			_source[index] = value;
		return changing;
	}

	/// <inheritdoc />
	public int Count
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			return _source.Count;
		});

	/// <inheritdoc />
	public virtual void Add(T item)
		=> Sync!.Modifying(() => AssertIsAlive(), () =>
		{
			_source.Add(item);
			return true;
		});

	public void Add(T item, T item2, params T[] items)
		=> AddThese(new[] { item, item2 }.Concat(items));

	public void AddThese(IEnumerable<T> items)
	{
        T[]? enumerable = items as T[] ?? items?.ToArray();
		if (enumerable is not null && enumerable.Length != 0)
		{
			Sync!.Modifying(() => AssertIsAlive(), () =>
			{
				foreach (T? item in enumerable)
					Add(item); // Yes, we could just _source.Add() but this allows for overrideing Add();
				return true;
			});
		}
	}

	/// <inheritdoc />
	public void Clear()
		=> Sync!.Modifying(
		() => AssertIsAlive() && _source.Count != 0,
		() =>
		{
            int count = Count;
            bool hasItems = count != 0;
			if (hasItems)
			{
				_source.Clear();
			}
			return hasItems;
		});

	/// <inheritdoc />
	public bool Contains(T item)
		=> Sync!.Reading(() => AssertIsAlive() && _source.Contains(item));

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			_source.CopyTo(array, arrayIndex);
		});

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			return _source.GetEnumerator();
		});

	/// <inheritdoc />
	public int IndexOf(T item)
		=> Sync!.Reading(
			() => AssertIsAlive()
			? _source.IndexOf(item)
			: -1);

	/// <inheritdoc />
	public void Insert(int index, T item)
		=> Sync!.Modifying(
			() => AssertIsAlive(),
			() =>
			{
				_source.Insert(index, item);
				return true;
			});

	/// <inheritdoc />
	public bool Remove(T item)
		=> Sync!.Modifying(
			() => AssertIsAlive(),
			() => _source.Remove(item));

	/// <inheritdoc />
	public void RemoveAt(int index)
		=> Sync!.Modifying(
			() => AssertIsAlive(),
			() =>
			{
				_source.RemoveAt(index);
				return true;
			});

	IEnumerator IEnumerable.GetEnumerator()
		=> Sync!.Reading(() =>
		{
			AssertIsAlive();
			return _source.GetEnumerator();
		});

	/// <summary>
	/// Synchonizes finding an item (<paramref name="target"/>), and if found, replaces it with the <paramref name="replacement"/>.
	/// </summary>
	/// <exception cref="ArgumentException">If <paramref name="throwIfNotFound"/> is true and the <paramref name="target"/> is not found.</exception>
	public bool Replace(T target, T replacement, bool throwIfNotFound = false)
	{
		AssertIsAlive();
        int index = -1;
		return !(target?.Equals(replacement) ?? replacement is null)
			&& Sync!.Modifying(
			() =>
			{
				AssertIsAlive();
				index = _source.IndexOf(target);
				return index != -1 || (throwIfNotFound ? throw new ArgumentException("Not found.", nameof(target)) : false);
			},
			() =>
				SetValueInternal(index, replacement)
		);
	}
}
