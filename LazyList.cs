/*!
 * @author electricessence / https://github.com/electricessence/
 * Origin: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Open.Threading;
using Open.Disposable;

namespace Open.Collections
{
	public class LazyList<T> : DisposableBase, IReadOnlyList<T>
	{
		List<T> _cached;
		IEnumerator<T> _enumerator;

		ReaderWriterLockSlim Sync;

		public readonly bool IsEndless;
		public LazyList(IEnumerable<T> source, bool isEndless = false)
		{
			_enumerator = source.GetEnumerator();
			_cached = new List<T>();
			Sync = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			IsEndless = isEndless; // To indicate if a source is not allowed to fully enumerate.
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			using (Sync.WriteLock())
			{
				Interlocked.Exchange(ref _enumerator, null)?.Dispose();
				Interlocked.Exchange(ref _cached, null)?.Dispose();
			}
			Interlocked.Exchange(ref Sync, null)?.Dispose();
		}

		public T this[int index]
		{
			get
			{
				AssertIsAlive();

				if (index < 0)
					throw new ArgumentOutOfRangeException("index", "Cannot be less than zero.");
				while (_cached.Count <= index && GetNext()) { }
				if (index < _cached.Count)
					return _cached[index];

				throw new ArgumentOutOfRangeException("index", "Great than total count.");
			}
		}

		public int Count
		{
			get
			{
                AssertIsAlive();
				Finish();
				return _cached.Count;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
            AssertIsAlive();

			int index = 0;
			bool more = _enumerator != null;
			while (more || index < _cached.Count)
			{
				if (index < _cached.Count)
				{
					yield return _cached[Interlocked.Increment(ref index) - 1];
				}
				else
				{
					more = GetNext();
				}
			}
		}

		public int IndexOf(T item)
		{
            AssertIsAlive();
			if (IsEndless)
				throw new InvalidOperationException("This list is marked as endless and may never complete. Use an enumerator, then Take(x).IndexOf().");

			int index = 0;
			bool more = _enumerator != null;
			while (more || index < _cached.Count)
			{
				if (index < _cached.Count)
				{
					if (_cached[index].Equals(item))
						return index;
					index++;
				}
				else
				{
					more = GetNext();
				}
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
			var len = Math.Min(Count, array.Length - arrayIndex);
			for (var i = 0; i < len; i++)
				array[i + arrayIndex] = this[i];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool GetNext()
		{
			if (_enumerator == null) return false;

			using (var uLock = Sync.UpgradableReadLock())
			{
				if (_enumerator != null)
				{
					uLock.UpgradeToWriteLock();
					if (_enumerator.MoveNext())
					{
						_cached.Add(_enumerator.Current);
						return true;
					}
					else
					{
						Interlocked.Exchange(ref _enumerator, null)?.Dispose();
					}
				}
			}

			return false;
		}

		private void Finish()
		{
			if (IsEndless)
				throw new InvalidOperationException("This list is marked as endless and may never complete.");
			while (GetNext()) { }
		}

	}
}