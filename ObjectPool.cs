using Open.Disposable;
using Open.Threading;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Open.Collections
{
	public class ObjectPool<T> : DisposableBase
		where T : class
	{

		ConcurrentBag<T> _pool;
		Func<T> _generator;


		Task _trimmer;
		Task _flusher;
		Task _autoFlusher;

		/**
		 * A transient amount of object to exist over _maxSize until trim() is called.
		 * But any added objects over _localAbsMaxSize will be disposed immediately.
		 */
		ushort _localAbsMaxSize;

		/**
		 * By default will clear after 5 seconds of non-use.
		 */
		int autoClearTimeout = 5000;


		/**
		 * Defines the maximum at which trimming should allow.
		 * @returns {number}
		 */

		public ushort MaxSize
		{
			get;
			private set;
		}

		/**
		 * Current number of objects in pool.
		 * @returns {number}
		 */
		public int Count
		{
			get
			{
				return _pool?.Count ?? 0;
			}
		}

		public T Take(Func<T> factory = null)
		{
			this.AssertIsAlive();
			if (_generator == null && factory == null)
				throw new ArgumentException("factory", "Must provide a factory if on was not provided at construction time.");

			try
			{
				return _pool.TryTake(out T value)
					? value
					: (factory ?? _generator).Invoke();
			}
			finally
			{
				_onTaken();
			}
		}

		public ObjectPool(
			ushort maxSize,
			Func<T> generator = null,
			Action<T> recycler = null)
		{
			//ushort.MaxValue
			//this._localAbsMaxSize = Math.min(_maxSize*2, ABSOLUTE_MAX_SIZE);

			//const _ = this;
			//_._disposableObjectName = OBJECT_POOL;
			//_._pool = [];
			//_._trimmer = new TaskHandler(()=>_._trim());
			//const clear = () => _._clear();
			_flusher = new AsyncProcess(Clear);
			_autoFlusher = new AsyncProcess(Clear);
		}


		protected void _trim()
		{
			//foreach(var t in _pool.TryTakeWhile()
			//{
			//		dispose.single(< any > pool.pop(), true);
			//	}
		}

		/**
		 * Will trim ensure the pool is less than the maxSize.
		 * @param defer A delay before trimming.  Will be overridden by later calls.
		 */
		public void Trim(int defer = 0)
		{
			//this.throwIfDisposed();
			//this._trimmer.start(defer);
		}

		protected void _clear()
		{
			//var p = _pool;
			//_._trimmer.cancel();
			//_._flusher.cancel();
			//_._autoFlusher.cancel();
			//dispose.these.noCopy(<any>p, true);
			//p.length = 0;
		}

		/**
		 * Will clear out the pool.
		 * Cancels any scheduled trims when executed.
		 * @param defer A delay before clearing.  Will be overridden by later calls.
		 */
		public void Clear(int defer = 0)
		{
			AssertIsAlive();
			if (defer == 0)
			{
				_flusher.EnsureActive();
				_flusher.Wait(); // Newer verson has a combined method.. ^^^
			}
			else
			{
				Task.Delay(defer)
					.ContinueWith(t=>Clear());
			}
		}

		void Clear(Progress p)
		{
			Clear();
		}

		public T[] ToArrayAndClear()
		{
			AssertIsAlive();
			//_._trimmer.cancel();
			//_._flusher.cancel();
			var p = _pool;
			_pool = new ConcurrentBag<T>();
			return p.ToArray();
		}

		/**
		 * Shortcut for toArrayAndClear();
		 */
		public T[] Dump()
		{
			return ToArrayAndClear();
		}


		protected override void OnDispose(bool calledExplicitly)
		{

			//const _:any = this;
			//_._generator = null;
			//_._recycler = null;
			//dispose(
			//	_._trimmer,
			//	_._flusher,
			//	_._autoFlusher
			//);
			//_._trimmer = null;
			//_._flusher = null;
			//_._autoFlusher = null;

			//_._pool.length = 0;
			//_._pool = null;

		}

		public void ExtendAutoClear()
		{
			//const _ = this;
			//_.throwIfDisposed();
			//const t = _.autoClearTimeout;
			//if(isFinite(t) && !_._autoFlusher.isScheduled)
			//	_._autoFlusher.start(t);
		}

		public void Add(T o)
		{
			//const _ = this;
			//_.throwIfDisposed();
			//if(_._pool.length>=_._localAbsMaxSize)
			//{
			//	// Getting too big, dispose immediately...
			//	dispose(<any>o);
			//}
			//else
			//{
			//	if(_._recycler) _._recycler(o);
			//_._pool.push(o);
			//	const m = _._maxSize;
			//	if(m<ABSOLUTE_MAX_SIZE && _._pool.length> m)
			//		_._trimmer.start(500);
			//}
			//_.extendAutoClear();

		}

		private void _onTaken()
		{
			//const _ = this, len = _._pool.length;
			//if(len<=_._maxSize)
			//	_._trimmer.cancel();
			//if(len)
			//	_.extendAutoClear();
		}

		public T TryTake()
		{
			//const _ = this;
			//_.throwIfDisposed();

			//try
			//{
			//	return _._pool.pop();
			//}
			//finally
			//{
			//	_._onTaken();
			//}
		}

	}
}
