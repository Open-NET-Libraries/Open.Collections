using Open.Disposable;
using Open.Threading;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class ObjectPool<T> : DisposableBase
		where T : class
	{

		ConcurrentBag<T> _pool;
		Func<T> _generator;
		Action<T> _recycler;


		ActionRunner _trimmer;
		ActionRunner _flusher;
		ActionRunner _autoFlusher;

		/**
		 * A transient amount of object to exist over MaxSize until trim() is called.
		 * But any added objects over _localAbsMaxSize will be disposed immediately.
		 */
		uint _localAbsMaxSize;


		public ObjectPool(
			ushort maxSize,
			Func<T> generator = null,
			Action<T> recycler = null)
		{
			MaxSize = maxSize;
			_localAbsMaxSize = Math.Min((uint)maxSize * 2, ushort.MaxValue);

			_generator = generator;
			_recycler = recycler;

            _pool = new ConcurrentBag<T>();
			_trimmer = new ActionRunner(TrimInternal);
			_flusher = new ActionRunner(ClearInternal);
			_autoFlusher = new ActionRunner(ClearInternal);
		}

		/// <summary>
		/// By default will clear after 5 seconds of non-use.
		/// </summary>
		public TimeSpan AutoClearTimeout = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Defines the maximum at which trimming should allow.
		/// </summary>
		public ushort MaxSize
		{
			get;
			private set;
		}


		/// <summary>
		/// Current number of objects in pool.
		/// </summary>
		public int Count
		{
			get
			{
				return _pool?.Count ?? 0;
			}
		}

		private void _onTaken()
		{
			var len = _pool.Count;
			if(len<=MaxSize)
				_trimmer.Cancel();
			if(len!=0)
				ExtendAutoClear();
		}

		/// <summary>
		/// Attempts to extract an item from the pool but if none are availalbe returns false.
		/// </summary>
		/// <param name="value">The item taken.</param>
		/// <returns>If one was available.</returns>
		public bool TryTake(out T value)
		{
			AssertIsAlive();

			bool taken = false;
			try
			{
				taken = _pool.TryTake(out value);
			}
			finally
			{
				if (taken)
					_onTaken();
			}
			return taken;
		}

		/// <summary>
		/// Attempts to extract an item from the pool but if non are availalbe it creates a new one from the factory provided or the underlying generator.
		/// </summary>
		/// <param name="factory">An optional custom factory to use if no items are in the pool.</param>
		/// <returns></returns>
		public T Take(Func<T> factory = null)
		{
			this.AssertIsAlive();
			if (_generator == null && factory == null)
				throw new ArgumentException("factory", "Must provide a factory if on was not provided at construction time.");

			return TryTake(out T value)
				? value
				: (factory ?? _generator).Invoke();
		}


		protected void TrimInternal()
		{
			_trimmer.Cancel();
			_flusher.Cancel();
			_autoFlusher.Cancel();
			foreach(var e in _pool.TryTakeWhile(b=> b.Count > MaxSize))
				(e as IDisposable)?.Dispose();
		}

		/// <summary>
		/// Will initiate trimming the pool to ensure it is less than the maxSize.
		/// </summary>
		/// <param name="defer">Optional millisecond value to wait until trimming starts.</param>
		/// <returns></returns>
		public async Task Trim(int defer = 0)
		{
			AssertIsAlive();
			await _trimmer.Defer(defer);
		}

		protected void ClearInternal()
		{
			var p = Interlocked.Exchange(ref _pool, new ConcurrentBag<T>());
			_trimmer.Cancel();
			_flusher.Cancel();
			_autoFlusher.Cancel();
			Clear(ref p);
		}

		protected static Task Clear(ref ConcurrentBag<T> bag)
		{
			var b = bag;
			bag = null;
			return b.ClearAsync(e => (e as IDisposable)?.Dispose());
		}

		/// <summary>
		/// Will clear out the pool.
		/// Cancels any scheduled trims when executed.
		/// </summary>
		/// <param name="defer">A delay before clearing.  Will be overridden by later calls.</param>
		/// <returns></returns>
		public async Task Clear(int defer = 0)
		{
			AssertIsAlive();
			await _flusher.Defer(defer);
		}

		/// <summary>
		/// Replaces current bag with a new one and returns the existing bag.
		/// </summary>
		/// <returns>The previous ConcurrentBag<T>.</returns>
		public ConcurrentBag<T> Dump()
		{
			AssertIsAlive();
			_trimmer.Cancel();
			_flusher.Cancel();
			return Interlocked.Exchange(ref _pool, new ConcurrentBag<T>());
		}


		protected override void OnDispose(bool calledExplicitly)
		{
			Interlocked.Exchange(ref _generator, null);
			Interlocked.Exchange(ref _recycler, null);

			Interlocked.Exchange(ref _trimmer, null)?.Dispose();
			Interlocked.Exchange(ref _flusher, null)?.Dispose();
			Interlocked.Exchange(ref _autoFlusher, null)?.Dispose();
			Clear(ref _pool);
		}

		/// <summary>
		/// Simply extends the auto-clear sequence from now until later by the AutoClearTimeout value.
		/// </summary>
		public void ExtendAutoClear()
		{
			AssertIsAlive();
			_autoFlusher?.Defer(AutoClearTimeout);
		}

		public void Give(ref T item)
		{
			AssertIsAlive();
			var p = _pool;
			if(p.Count>=_localAbsMaxSize)
			{
				// Getting too big, dispose immediately...
				(item as IDisposable)?.Dispose();
			}
			else
			{
				_recycler?.Invoke(item);
				p.Add(item);
				if (p.Count > MaxSize)
					_trimmer.Defer(500);
			}
			ExtendAutoClear();
		}
	}
}
