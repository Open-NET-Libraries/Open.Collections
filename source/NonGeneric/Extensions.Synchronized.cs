using Open.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Open.Collections.NonGeneric
{
	public static partial class Extensions
	{

		/// <summary>
		/// Attempts to add a value by synchronizing the collection.
		/// </summary>
		/// <returns>
		/// Returns true if a value was added.  False if value already exists or a lock could not be acquired.
		/// </returns>
		public static bool TryAddSynchronized(
			this IDictionary target,
			object key,
			object value,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool added = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref added,
				lockType => !target.Contains(key),
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
		public static bool TryAddSynchronized(
			this IDictionary target,
			object key,
			Func<object> valueFactory,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool added = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref added,
				lockType => !target.Contains(key),
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
		public static bool RemoveSynchronized(
			this IDictionary target,
			object key,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			bool removed = false;
			ThreadSafety.SynchronizeReadWriteKeyAndObject(
				target, key, ref removed,
				lockType => target.Contains(key),
				() =>
				{
					target.Remove(key);
					return true;
				}, millisecondsTimeout, false);
			return removed;
		}

		/// <summary>
		/// Thread safe value for syncronizing acquiring a value from a non-generic dictionary.
		/// </summary>
		/// <returns>True if a value was acquired.</returns>
		public static bool TryGetValueSynchronized<T>(this IDictionary target, object key, out T value)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			T result = default(T);
			bool success = ThreadSafety.SynchronizeRead(target, key, () =>
				ThreadSafety.SynchronizeRead(target, () =>
					target.TryGetValue(key, out result)
				)
			);

			value = result;

			return success;
		}


		/// <summary>
		/// Attempts to acquire a specified type from a non-generic dictonary.
		/// </summary>
		public static T GetValueTypeSynchronized<T>(this IDictionary target, object key, bool throwIfNotExists = false)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			object value = target.GetValueSynchronized(key, throwIfNotExists);
			try
			{
				return value == null ? default(T) : (T)value;
			}
			catch (InvalidCastException) { }

			return default(T);
		}

		public static object GetValueSynchronized(this IDictionary target, object key, bool throwIfNotExists = false)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var exists = target.TryGetValueSynchronized(key, out object value);

			if (!exists && throwIfNotExists)
				throw new KeyNotFoundException(key.ToString());

			return exists ? value : null;
		}


		/// <summary>
		/// Thread safe method for synchronizing acquiring a value from the dictionary.  If no value is present it adds the value provided.
		/// If the millisecondTimeout is reached the value is still returned but the collection is unchanged.
		/// </summary>
		public static T GetOrAddSynchronized<T>(
			this IDictionary target,
			object key,
			T value,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS,
			bool throwsOnTimeout = true)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			ValidateMillisecondsTimeout(millisecondsTimeout);

			T result = default(T);
			// Uses threadsafe means to acquire value.
			Func<LockType, bool> condition = lockType => !target.TryGetValue(key, out result);
			Action render = () =>
			{
				result = value;
				target.Add(key, result); // A lock is required when adding a value.  AKA 'changing the collection'.
			};

			if (!ThreadSafety.SynchronizeReadWrite(target, condition, render, millisecondsTimeout, throwsOnTimeout))
				return value; // Value doesn'T exist and timeout exceeded? Return the add value...

			return result;
		}


		/// <summary>
		/// Thread safe method for synchronizing acquiring a value from the dictionary.  If no value is present it adds it using the valueFactory response.
		/// If the millisecondTimeout is reached the valueFactory is executed and the value is still returned but the collection is unchanged.
		/// </summary>
		public static T GetOrAddSynchronized<T>(
			this IDictionary target,
			object key,
			Func<object, T> valueFactory,
			int millisecondsTimeout = SYNC_TIMEOUT_DEFAULT_MILLISECONDS)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");
			ValidateMillisecondsTimeout(millisecondsTimeout);

			T result = default(T);
			Func<LockType, bool> condition = lockType => !ThreadSafety.SynchronizeRead(target, () => target.TryGetValue(key, out result));

			// Once a per value write lock is established, execute the scheduler, and syncronize adding...
			Action render = () => target.GetOrAddSynchronized(key, result = valueFactory(key), millisecondsTimeout);

			if (!ThreadSafety.SynchronizeReadWrite(target, key, condition, render, millisecondsTimeout, false))
				render(); // Timeout failed? Lock insert anyway and move on...

			// ^^^ What actually happens...
			// See previous (generic) method explaination.

			return result;
		}

	}
}
