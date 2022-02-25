using Open.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Open.Collections.NonGeneric;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		var added = false;
		ThreadSafety.SynchronizeReadWriteKeyAndObject(
			target, key, ref added,
			_ => !target.Contains(key),
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		var added = false;
		ThreadSafety.SynchronizeReadWriteKeyAndObject(
			target, key, ref added,
			_ => !target.Contains(key),
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		var removed = false;
		ThreadSafety.SynchronizeReadWriteKeyAndObject(
			target, key, ref removed,
			_ => target.Contains(key),
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		T result = default!;
		var success = ThreadSafety.SynchronizeRead(target, key, () =>
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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		var value = target.GetValueSynchronized(key, throwIfNotExists);
		try
		{
			return value is null ? default! : (T)value;
		}
		catch (InvalidCastException) { }

		return default!;
	}

	/// <summary>
	/// Thread safe method for getting a value from a dictionary.
	/// </summary>
	[SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
	public static object? GetValueSynchronized(this IDictionary target, object key, bool throwIfNotExists = false)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		ValidateMillisecondsTimeout(millisecondsTimeout);

		T result = default!;
		// Uses threadsafe means to acquire value.
		bool condition(LockType _) => !target.TryGetValue(key, out result);

		void render()
		{
			result = value;
			target.Add(key, result); // A lock is required when adding a value.  AKA 'changing the collection'.
		}

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
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		ValidateMillisecondsTimeout(millisecondsTimeout);

		T result = default!;
		bool condition(LockType _) => !ThreadSafety.SynchronizeRead(target, () => target.TryGetValue(key, out result));

		// Once a per value write lock is established, execute the scheduler, and syncronize adding...
		void render() => target.GetOrAddSynchronized(key, result = valueFactory(key), millisecondsTimeout);

		if (!ThreadSafety.SynchronizeReadWrite(target, key, condition, render, millisecondsTimeout, false))
			render(); // Timeout failed? Lock insert anyway and move on...

		// ^^^ What actually happens...
		// See previous (generic) method explaination.

		return result;
	}
}
