/*!
 * @author electricessence / https://github.com/electricessence/
 * Origin: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using Open.Disposable;
using Open.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Open.Collections
{
	public class LazyList<T> : DisposableBase, IReadOnlyList<T>
	{
		List<T> _cached;
		IEnumerator<T> _enumerator;

		ReaderWriterLockSlim Sync;

		public bool IsEndless { get; private set; }
		public LazyList(IEnumerable<T> source, bool isEndless = false)
		{
			_enumerator = source.GetEnumerator();
			_cached = new List<T>();
			Sync = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion); // This is important as it's possible to recurse infinitely to generate a result. :(
			IsEndless = isEndless; // To indicate if a source is not allowed to fully enumerate.
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			if (!calledExplicitly) return;

			using (Sync.WriteLock())
			{
				DisposeOf(ref _enumerator);
				Nullify(ref _cached)?.Clear();
			}

			DisposeOf(ref Sync);
		}

		public T this[int index]
		{
			get
			{
				AssertIsAlive();

				if (index < 0)
					throw new ArgumentOutOfRangeException(nameof(index), "Cannot be less than zero.");
				if (!EnsureIndex(index))
					throw new ArgumentOutOfRangeException(nameof(index), "Greater than total count.");

				return _cached[index];
			}
		}

		private int _safeCount;
		public int Count
		{
			get
			{
				AssertIsAlive();
				Finish();
				return _cached.Count;
			}
		}

		public bool TryGetValueAt(int index, out T value)
		{
			if (EnsureIndex(index))
			{
				value = _cached[index];
				return true;
			}

			value = default;
			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			AssertIsAlive();

			var index = -1;
			// Interlocked allows for multi-threaded access to this enumerator.
			while (TryGetValueAt(Interlocked.Increment(ref index), out var value))
				yield return value;
		}

		public int IndexOf(T item)
		{
			AssertIsAlive();
			if (IsEndless)
				throw new InvalidOperationException("This list is marked as endless and may never complete. Use an enumerator, then Take(x).IndexOf().");

			var index = 0;
			while (EnsureIndex(index))
			{
				var value = _cached[index];
				if (value == null)
				{
					if (item == null) return index;
				}
				else if (value.Equals(item))
					return index;

				index++;
			}

			return -1;

		}

		public bool Contains(T item)
		{
			AssertIsAlive();
			return IndexOf(item) != -1;
		}

		public void CopyTo(T[] array, int arrayIndex = 0)
		{
			AssertIsAlive();
			var len = Math.Min(IsEndless ? int.MaxValue : Count, array.Length - arrayIndex);
			for (var i = 0; i < len; i++)
				array[i + arrayIndex] = this[i];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool EnsureIndex(int maxIndex)
		{
			if (maxIndex < _safeCount)
				return true;

			// This is where the fun begins...
			// Mutliple threads can be out of sync (probably through a memory barrier)
			// And a sync read operation must be done to ensure safety.
			var count = Sync.ReadValue(() => _cached.Count);
			if (maxIndex < count)
			{
				// We're still within the existing results, but safe count is not up to date.
				// Make a single attempt to update safe count.
				if (_safeCount < count)
					Interlocked.CompareExchange(ref _safeCount, count, _safeCount);

				return true;
			}

			if (_enumerator == null)
				return false;

			// This very well could be a simple lock{} statement but the ReaderWriterLockSlim recursion protection is actually quite useful.
			using (var uLock = Sync.UpgradableReadLock())
			{
				// Note: Within an upgradable read, other reads pile up.

				var c = _cached.Count;
				if (_safeCount != c) // Always do comparisons outside of interlocking first.
					Interlocked.CompareExchange(ref _safeCount, c, _safeCount);

				if (maxIndex < _safeCount)
					return true;

				if (_enumerator == null)
					return false;

				uLock.UpgradeToWriteLock();

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
			}

			return false;
		}

		private void Finish()
		{
			if (IsEndless)
				throw new InvalidOperationException("This list is marked as endless and may never complete.");
			while (EnsureIndex(int.MaxValue)) { }
		}

	}
}
