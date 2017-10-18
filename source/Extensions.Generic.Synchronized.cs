using Open.Threading;
using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Collections
{
    public static partial class Extensions
    {

		const int SYNC_TIMEOUT_DEFAULT_MILLISECONDS = 10000;

		internal static void ValidateMillisecondsTimeout(int? millisecondsTimeout)
		{
			if ((millisecondsTimeout ?? 0) < 0)
				throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "Cannot be a negative value.");
		}

		/// <summary>
		/// Thread safe value for syncronizing acquiring a value from a generic dictionary.
		/// </summary>
		/// <returns>True if a value was acquired.</returns>
		public static bool TryGetValueSynchronized<TKey, TValue>(
			this IDictionary<TKey, TValue> target,
			TKey key, out TValue value)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			TValue result = default(TValue);
			bool success = ThreadSafety.SynchronizeRead(target, key, () =>
				ThreadSafety.SynchronizeRead(target, () =>
					target.TryGetValue(key, out result)
				)
			);

			value = result;

			return success;
		}

		/// <summary>
		/// Attempts to acquire a specified type from a generic dictonary.
		/// </summary>
		public static TValue GetValueSynchronized<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, bool throwIfNotExists = true)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var exists = target.TryGetValueSynchronized(key, out TValue value);

			if (!exists && throwIfNotExists)
				throw new KeyNotFoundException(key.ToString());

			return exists ? value : default(TValue);
		}


		/// <summary>
		/// Thread safe value for syncronizing adding a value to list only if it does not exist.
		/// </summary>
		public static void RegisterSynchronized<T>(this ICollection<T> target, T value)
		{
			if (target == null) throw new NullReferenceException();

			ThreadSafety.SynchronizeReadWriteKeyAndObject(target, value,
				lockType => !target.Contains(value),
				() => target.Add(value));
		}


		/// <summary>
		/// Thread safe shortcut for adding a value or updating based on exising value.
		/// If no value exists, it adds the provided value.
		/// If a value exists, it sets the value using the updateValueFactory.
		/// </summary>
		public static T AddOrUpdateSynchronized<TKey, T>(this IDictionary<TKey, T> target, TKey key, T value,
			Func<TKey, T, T> updateValueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

			T valueUsed = default(T);

			// First we get a lock on the key action which should prevent the individual action from changing..
			ThreadSafety.SynchronizeWrite(target, key, () =>
			{
				// Synchronize reading the action and seeing what we need to do next...
				if (target.TryGetValue(key, out T old))
				{
					// Since we have a lock on the entry, go ahead an render the update action.
					var updateValue = updateValueFactory(key, old);
					// Then with a write lock on the collection, try it all again...
					ThreadSafety.SynchronizeWrite(target, () => valueUsed = target.AddOrUpdate(key, value, updateValue));
				}
				else
				{
					// Fallback for if the action changed.  Will end up locking the collection but what can we do.. :(
					ThreadSafety.SynchronizeWrite(target, () => valueUsed = target.AddOrUpdate(key, value, updateValueFactory));
				}
			});

			return valueUsed;
		}


		/// <summary>
		/// Thread safe shortcut for adding a value or updating based on exising value.
		/// If no value exists, it adds the value using the newValueFactory.
		/// If a value exists, it sets the value using the updateValueFactory.
		/// </summary>
		public static T AddOrUpdateSynchronized<TKey, T>(this IDictionary<TKey, T> target, TKey key,
			Func<TKey, T> newValueFactory,
			Func<TKey, T, T> updateValueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (newValueFactory == null) throw new ArgumentNullException("newValueFactory");
			if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

			T valueUsed = default(T);

			// First we get a lock on the key action which should prevent the individual action from changing..
			ThreadSafety.SynchronizeWrite(target, key, () =>
			{
				// Synchronize reading the action and seeing what we need to do next...
				if (target.TryGetValue(key, out T old))
				{
					// Since we have a lock on the entry, go ahead an render the update action.
					var updateValue = updateValueFactory(key, old);
					// Then with a write lock on the collection, try it all again...
					ThreadSafety.SynchronizeWrite(target,
					() =>
						valueUsed = target.AddOrUpdate(key,
							newValueFactory,
							(k, o) => o.Equals(old) && k.Equals(key) ? updateValue : updateValueFactory(k, o)
				));
				}
				else
				{
					// Since we have a lock on the entry, go ahead an render the add action.
					var value = newValueFactory(key);
					// Then with a write lock on the collection, try it all again...
					ThreadSafety.SynchronizeWrite(target,
					() =>
						valueUsed = target.AddOrUpdate(key,
							k => k.Equals(key) ? value : newValueFactory(k),
							updateValueFactory
				));
				}
			});

			return valueUsed;
		}

		public static void AddSynchronized<T>(this ICollection<T> target, T value)
		{
			if (target == null) throw new NullReferenceException();
			ThreadSafety.SynchronizeWrite(target, () => target.Add(value));
		}

		/// <summary>
		/// Thread safe shortcut for adding a value to list within a dictionary.
		/// </summary>
		public static void AddToSynchronized<TKey, TValue>(this IDictionary<TKey, IList<TValue>> c, TKey key, TValue value)
		{
			if (c == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var list = c.GetOrAddSynchronized(key, k => new List<TValue>());
			list.AddSynchronized(value);
		}

		/// <summary>
		/// Thread safe shortcut for ensuring a cacheKey contains a action.  If no action exists, it adds the provided defaultValue.
		/// </summary>
		public static void EnsureDefaultSynchronized<TKey, T>(this IDictionary<TKey, T> target, TKey key, T defaultValue)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			ThreadSafety.SynchronizeReadWrite(target,
				lockType => !target.ContainsKey(key),
				() => target.Add(key, defaultValue));
		}

		/// <summary>
		/// Thread safe shortcut for ensuring a cacheKey contains a Value.  If no value exists, it adds it using the provided defaultValueFactory.
		/// </summary>
		public static void EnsureDefaultSynchronized<TKey, T>(this IDictionary<TKey, T> target, TKey key,
			Func<TKey, T> defaultValueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (defaultValueFactory == null) throw new ArgumentNullException("defaultValueFactory");

			ThreadSafety.SynchronizeReadWrite(target, key,
				lockType => !target.ContainsKey(key),
				() => target.EnsureDefaultSynchronized(key, defaultValueFactory));
		}



		/// <summary>
		/// Thread safe method for synchronizing acquiring a value from the dictionary.  If no value is present it adds the value provided.
		/// If the millisecondTimeout is reached the value is still returned but the collection is unchanged.
		/// </summary>
		public static T GetOrAddSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			T value,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS,
			bool throwsOnTimeout = true)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			ValidateMillisecondsTimeout(millisecondsTimeout);

			T result = default(T);
			Func<LockType, bool> condition = lockType => !target.TryGetValue(key, out result);
			Action render = () =>
			{
				result = value;
				target.Add(key, result);
			};

			if (!ThreadSafety.SynchronizeReadWrite(target, condition, render, millisecondsTimeout, throwsOnTimeout))
				return value; // Value doesn'T exist and timeout exceeded? Return the add value...

			return result;
		}

		/// <summary>
		/// Thread safe method for synchronizing acquiring a value from the dictionary.  If no value is present it adds it using the valueFactory response.
		/// If the millisecondTimeout is reached the valueFactory is executed and the value is still returned but the collection is unchanged.
		/// </summary>
		public static T GetOrAddSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			Func<TKey, T> valueFactory,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");
			ValidateMillisecondsTimeout(millisecondsTimeout);

			T result = default(T);
			// Note, the following sync read is on the TARGET and not the key. See below.
			Func<LockType, bool> condition = lockType => !ThreadSafety.SynchronizeRead(target, () => target.TryGetValue(key, out result));

			// Once a per value write lock is established, execute the scheduler, and syncronize adding...
			Action render = () => target.GetOrAddSynchronized(key, result = valueFactory(key), millisecondsTimeout);

			// This will queue up subsequent reads for the same value.
			if (!ThreadSafety.SynchronizeReadWrite(target, key, condition, render, millisecondsTimeout, false))
				render(); // Timeout failed? Lock insert anyway and move on...

			// ^^^ What actually happens...
			// 1) Value is checked for without a lock and if acquired returns it using the 'condition' query.
			// 2) Value is checked for WITH a lock and if acquired returns it using the 'condition' query.
			// 3) A localized lock is acquired for the the key which tells other _threads to wait while the value is generated and added.
			// 4) Value is checked for without a lock and if acquired returns it using the 'condition' query.
			// 5) The value is then rendered using the ensureRendered query without locking the entire collection.  This allows for other values to be added.
			// 6) The rendered value is then used to add to the collection if the value is missing, locking the collection if an add is necessary.

			return result;
		}



		/// <summary>
		/// Attempts to add a value by synchronizing the collection.
		/// </summary>
		/// <returns>
		/// Returns true if a value was added.  False if value already exists or a lock could not be acquired.
		/// </returns>
		public static bool TryAddSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			T value,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool added = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref added,
			lockType => !target.ContainsKey(key),
			() =>
			{
				target.Add(key, value);
				return true;
			}, millisecondsTimeout, false);
			return added;
		}


		/// <summary>
		/// Attempts to add a value by synchronizing the collection.
		/// </summary>
		/// <returns>
		/// Returns true if a value was added.  False if value already exists or a lock could not be acquired.
		/// </returns>
		public static bool TryAddSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			Func<T> valueFactory,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool added = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref added,
			lockType => !target.ContainsKey(key),
			() =>
			{
				target.Add(key, valueFactory());
				return true;
			}, millisecondsTimeout, false);
			return added;
		}


		/// <summary>
		/// Attempts to add a value by synchronizing the collection.
		/// </summary>
		/// <returns>
		/// Returns true if a value was added.  False if value already exists or a lock could not be acquired.
		/// </returns>
		public static bool TryRemoveSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool removed = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref removed,
				lockType => target.ContainsKey(key),
				() => target.Remove(key),
				millisecondsTimeout, false);
			return removed;
		}

		/// <summary>
		/// Attempts to add a value by synchronizing the collection.
		/// </summary>
		/// <returns>
		/// Returns true if a value was added.  False if value already exists or a lock could not be acquired.
		/// </returns>
		public static bool TryRemoveSynchronized<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			out T value,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			value = default(T);
			bool removed = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref value,
				lockType => target.ContainsKey(key),
				() =>
				{
					var r = target[key];
					removed = target.Remove(key);
					return removed ? r : default(T);
				},
					millisecondsTimeout, false);
			return removed;
		}
	}
}
