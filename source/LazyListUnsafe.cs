/*!
 * @author electricessence / https://github.com/electricessence/
 * Partly based on: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 */

using Open.Disposable;

namespace Open.Collections;

/// <summary>
/// An "unsafe" lazy list is one that is not thread safe, does not manage recursion, and does not protect against stack overflow (infinite length).
/// Only use if you know the results are finite and access is thread safe.
/// Note: disposing releases the underlying enumerator if it never reached the end of the results.
/// </summary>
public class LazyListUnsafe<T>(IEnumerable<T> source)
	: DisposableBase, IReadOnlyList<T>
{
	/// <summary>
	/// The memoized results.
	/// </summary>
	protected List<T> Cached = [];

	/// <summary>
	/// The enumerator for the source.
	/// </summary>
	protected IEnumerator<T> Enumerator = source.GetEnumerator();

	/// <inheritdoc />
	protected override void OnDispose()
	{
		DisposeOf(ref Enumerator);
		Nullify(ref Cached)?.Clear();
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

			return Cached[index];
		}
	}

	/// <inheritdoc />
	public int Count
	{
		get
		{
			AssertIsAlive();
			Finish();
			return Cached.Count;
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
			value = Cached[index];
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
			T? value = Cached[index];
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

	/// <summary>
	/// Ensures that the index is available by enumerating to that index and memoizing results.
	/// </summary>
	protected virtual bool EnsureIndex(int maxIndex)
	{
		if (maxIndex < Cached.Count)
			return true;

		if (Enumerator is null)
			return false;

		while (Enumerator.MoveNext())
		{
			if (Cached.Count == int.MaxValue)
				throw new Exception("Reached maximum contents for a single list.  Cannot memoize further.");

			Cached.Add(Enumerator.Current);

			if (maxIndex < Cached.Count)
				return true;
		}

		DisposeOf(ref Enumerator);

		return false;
	}

	/// <summary>
	/// Ensures all results are memoized.
	/// </summary>
	protected virtual void Finish()
	{
		while (EnsureIndex(int.MaxValue)) { }
	}
}
