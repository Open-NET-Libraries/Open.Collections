/*!
 * @author electricessence / https://github.com/electricessence/
 * Partly based on: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 */

using Open.Disposable;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Open.Collections;

/// <summary>
/// An "unsafe" lazy list is one that is not thread safe, does not manage recursion, and does not protect against stack overflow (infinite length).
/// Only use if you know the results are finite and access is thread safe.
/// Note: disposing releases the underlying enumerator if it never reached the end of the results.
/// </summary>
public class LazyListUnsafe<T> : DisposableBase, IReadOnlyList<T>
{
	protected List<T> _cached;
	protected IEnumerator<T> _enumerator;

	public LazyListUnsafe(IEnumerable<T> source)
	{
		_enumerator = source.GetEnumerator();
		_cached = new List<T>();
	}

	protected override void OnDispose()
	{
		DisposeOf(ref _enumerator);
		Nullify(ref _cached)?.Clear();
	}

	const string MUST_BE_AT_LEAST_ZERO = "Must be at least zero.";
	const string GREATER_THAN_TOTAL = "Greater than total count.";

	/// <inheritdoc />
	public T this[int index]
	{
		get
		{
			AssertIsAlive();

			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), MUST_BE_AT_LEAST_ZERO);
			if (!EnsureIndex(index))
				throw new ArgumentOutOfRangeException(nameof(index), GREATER_THAN_TOTAL);
			Contract.EndContractBlock();

			return _cached[index];
		}
	}

	/// <inheritdoc />
	public int Count
	{
		get
		{
			AssertIsAlive();
			Finish();
			return _cached.Count;
		}
	}

	/// <summary>
	/// Will attempt to get a value in the list if it is within the count of the results.
	/// </summary>
	/// <param name="index">The index to look up.</param>
	/// <param name="value">The value at that index.</param>
	/// <returns>True if acquired.  False if the index is greater</returns>
	public bool TryGetValueAt(int index, out T value)
	{
		AssertIsAlive();
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, MUST_BE_AT_LEAST_ZERO);

		if (EnsureIndex(index))
		{
			value = _cached[index];
			return true;
		}

		value = default!;
		return false;
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		AssertIsAlive();

		int index = -1;
		// Interlocked allows for multi-threaded access to this enumerator.
		while (TryGetValueAt(Interlocked.Increment(ref index), out T? value))
			yield return value;
	}

	/// <inheritdoc/>
	public virtual int IndexOf(T item)
	{
		AssertIsAlive();

		for (int index = 0; EnsureIndex(index); index++)
		{
			T? value = _cached[index];
			if (value is null)
			{
				if (item is null) return index;
			}
			else if (value.Equals(item))
			{
				return index;
			}
		}

		return -1;
	}

	/// <inheritdoc/>
	public bool Contains(T item)
	{
		AssertIsAlive();
		return IndexOf(item) != -1;
	}

	/// <inheritdoc cref="ReadOnlyCollectionWrapper{T, TCollection}.CopyTo(Span{T})"/>
	public Span<T> CopyTo(T[] array, int startIndex = 0)
	{
		if (array is null) throw new ArgumentNullException(nameof(array));
		if (startIndex >= array.Length)
			throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Must be less than the length of the provided array.");

		Span<T> span = array.AsSpan();
		return this.CopyToSpan(startIndex == 0 ? span : span.Slice(startIndex));
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

	protected virtual bool EnsureIndex(int maxIndex)
	{
		if (maxIndex < _cached.Count)
			return true;

		if (_enumerator is null)
			return false;

		while (_enumerator.MoveNext())
		{
			if (_cached.Count == int.MaxValue)
				throw new Exception("Reached maximium contents for a single list.  Cannot memoize further.");

			_cached.Add(_enumerator.Current);

			if (maxIndex < _cached.Count)
				return true;
		}

		DisposeOf(ref _enumerator);

		return false;
	}

	protected virtual void Finish()
	{
		while (EnsureIndex(int.MaxValue)) { }
	}
}
