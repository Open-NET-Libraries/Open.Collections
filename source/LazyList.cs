/*!
 * @author electricessence / https://github.com/electricessence/
 * Partly based on: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 */

using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Open.Collections;

/// <summary>
/// A a thread-safe list for caching the results of an enumerable.
/// Note: should be disposed manually whenever possible as the locking mechanism is a ReaderWriterLockSlim.
/// </summary>
public sealed class LazyList<T> : LazyListUnsafe<T>
{
	ReaderWriterLockSlim Sync;
	int _safeCount;

	/// <summary>
	/// A value indicating whether the results are known or expected to be finite.
	/// A list that was constructed as endless but has reached the end of the results will return false.
	/// </summary>
	public bool IsEndless { get; private set; }

	public LazyList(IEnumerable<T> source, bool isEndless = false) : base(source)
	{
		Sync = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion); // This is important as it's possible to recurse infinitely to generate a result. :(
		IsEndless = isEndless; // To indicate if a source is not allowed to fully enumerate.
	}

	protected override void OnDispose()
	{
		using (Sync.WriteLock()) base.OnDispose();

		DisposeOf(ref Sync);
	}

	private const string MARKED_ENDLESS = "This list is marked as endless and may never complete.";

	/// <inheritdoc/>
	public override int IndexOf(T item)
	{
		const string MESSAGE = MARKED_ENDLESS + " Use an enumerator, then Take(x).IndexOf().";
		if (IsEndless) throw new InvalidOperationException(MESSAGE);
		Contract.EndContractBlock();

		return base.IndexOf(item);
	}

	protected override bool EnsureIndex(int maxIndex)
	{
		if (maxIndex < _safeCount)
			return true;

		// This is where the fun begins...
		// Mutliple threads can be out of sync (probably through a memory barrier)
		// And a sync read operation must be done to ensure safety.
		int count = Sync.Read(() => _cached.Count);
		if (maxIndex < count)
		{
			// We're still within the existing results, but safe count is not up to date.
			// Make a single attempt to update safe count.
			if (_safeCount < count)
				Interlocked.CompareExchange(ref _safeCount, count, _safeCount);

			return true;
		}

		if (_enumerator is null)
			return false;

		// This very well could be a simple lock{} statement but the ReaderWriterLockSlim recursion protection is actually quite useful.
		using var uLock = Sync.UpgradableReadLock();
		// Note: Within an upgradable read, other reads pile up.

		int c = _cached.Count;
		if (_safeCount != c) // Always do comparisons outside of interlocking first.
			Interlocked.CompareExchange(ref _safeCount, c, _safeCount);

		if (maxIndex < _safeCount)
			return true;

		if (_enumerator is null)
			return false;

		using var wLock = Sync.WriteLock();

		while (_enumerator.MoveNext())
		{
			if (_cached.Count == int.MaxValue)
				throw new Exception("Reached maximium contents for a single list.  Cannot memoize further.");

			_cached.Add(_enumerator.Current);

			if (maxIndex < _cached.Count)
				return true;
		}

		IsEndless = false;
		DisposeOf(ref _enumerator);

		return false;
	}

	protected override void Finish()
	{
		if (IsEndless)
			throw new InvalidOperationException(MARKED_ENDLESS);
		base.Finish();
	}
}
