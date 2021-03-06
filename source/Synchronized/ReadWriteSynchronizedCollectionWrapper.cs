﻿using Open.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Open.Collections.Synchronized
{
	public class ReadWriteSynchronizedCollectionWrapper<T, TCollection> : CollectionWrapper<T, TCollection>, ISynchronizedCollectionWrapper<T, TCollection>
		where TCollection : class, ICollection<T>
	{
		protected ReaderWriterLockSlim Sync = new(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.

		protected ReadWriteSynchronizedCollectionWrapper(TCollection source) : base(source)
		{
		}

		#region Implementation of ICollection<T>

		/// <inheritdoc />
		public override void Add(T item)
			=> Sync.Write(() => InternalSource.Add(item));

		/// <inheritdoc />
		public override void Add(T item1, T item2, params T[] items)
		{
			Sync.Write(() =>
			{
				InternalSource.Add(item1);
				InternalSource.Add(item2);
				foreach (var i in items)
					InternalSource.Add(i);
			});
		}

		/// <inheritdoc />
		public override void Add(T[] items)
		{
			Sync.Write(() =>
			{
				foreach (var i in items)
					InternalSource.Add(i);
			});
		}

		/// <inheritdoc />
		public override void Clear()
			=> Sync.Write(() => InternalSource.Clear());

		/// <inheritdoc cref="CollectionWrapper&lt;T, TCollection&gt;" />
		public override bool Contains(T item)
			=> Sync.ReadValue(() => InternalSource.Contains(item));

		/// <inheritdoc />
		public override bool Remove(T item)
		{
			var result = false;
			Sync.ReadWriteConditionalOptimized(
				lockType => result = InternalSource.Contains(item),
				() => result = InternalSource.Remove(item));
			return result;
		}

		/// <inheritdoc cref="CollectionWrapper&lt;T, TCollection&gt;" />
		public override int Count
			=> Sync.ReadValue(() => InternalSource.Count);


		/// <inheritdoc />
		public T[] Snapshot()
			=> Sync.ReadValue(() => InternalSource.ToArray());

		/// <inheritdoc cref="CollectionWrapper&lt;T, TCollection&gt;" />
		public override void Export(ICollection<T> to)
			=> Sync.Read(() => to.Add(InternalSource));

		/// <inheritdoc />
		public override void CopyTo(T[] array, int arrayIndex)
			=> Sync.Read(() => InternalSource.CopyTo(array, arrayIndex));

		/// <inheritdoc cref="ReadOnlyCollectionWrapper{T, TCollection}.CopyTo(Span{T})"/>
		public override Span<T> CopyTo(Span<T> span)
		{
			using var read = Sync.ReadLock();
			return InternalSource.CopyToSpan(span);
		}
			
		#endregion

		#region Dispose
		protected override void OnDispose()
		{
			Nullify(ref Sync)?.Dispose();

			base.OnDispose();
		}
		#endregion

		/// <summary>
		/// A thread-safe ForEach method.
		/// WARNING: If useSnapshot is false, the collection will be unable to be written to while still processing results and dead-locks can occur.
		/// </summary>
		/// <param name="action">The action to be performed per entry.</param>
		/// <param name="useSnapshot">Indicates if a copy of the contents will be used instead locking the collection.</param>
		public void ForEach(Action<T> action, bool useSnapshot = true)
		{
			if (useSnapshot)
			{
				foreach (var item in Snapshot())
					action(item);
			}
			else
			{
				Sync.Read(() =>
				{
					foreach (var item in InternalSource)
						action(item);
				});
			}
		}

		/// <inheritdoc />
		public void Modify(Func<bool> condition, Action<TCollection> action)
			=> Sync.ReadWriteConditionalOptimized(lockType => condition(), () => action(InternalSource));

		/// <summary>
		/// Allows for multiple modifications at once.
		/// </summary>
		/// <param name="condition">Only executes the action if the condition is true.  The condition may be invoked more than once.</param>
		/// <param name="action">The action to execute safely on the underlying collection safely.</param>
		public void Modify(Func<LockType, bool> condition, Action<TCollection> action)
			=> Sync.ReadWriteConditionalOptimized(condition, () => action(InternalSource));

		/// <inheritdoc />
		public void Modify(Action<TCollection> action)
			=> Sync.Write(() => action(InternalSource));

		/// <inheritdoc />
		public TResult Modify<TResult>(Func<TCollection, TResult> action)
			=> Sync.WriteValue(() => action(InternalSource));

		/// <inheritdoc />
		public virtual bool IfContains(T item, Action<TCollection> action)
		{
			var executed = false;
			Sync.ReadWriteConditionalOptimized(lockType => InternalSource.Contains(item), () =>
			{
				action(InternalSource);
				executed = true;
			});
			return executed;
		}

		/// <inheritdoc />
		public virtual bool IfNotContains(T item, Action<TCollection> action)
		{
			var executed = false;
			Sync.ReadWriteConditionalOptimized(lockType => !InternalSource.Contains(item), () =>
			{
				action(InternalSource);
				executed = true;
			});
			return executed;
		}

	}
}
