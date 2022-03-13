using System;
using System.Collections;
using System.Diagnostics.Contracts;

namespace Open.Collections.NonGeneric;

public static partial class Extensions
{
	const int SYNC_TIMEOUT_DEFAULT_MILLISECONDS = 10000;

	internal static void ValidateMillisecondsTimeout(int? millisecondsTimeout)
	{
		if ((millisecondsTimeout ?? 0) < 0)
			throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, "Cannot be a negative value.");
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds it using the valueFactory response.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T GetOrAdd<T>(
		this IDictionary target,
		object key,
		Func<object, T> valueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));

		if (!target.TryGetValue(key, out T value))
			target.Add(key, value = valueFactory(key));
		return value;
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds the value provided.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T GetOrAdd<T>(
		this IDictionary target,
		object key,
		T value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		if (!target.TryGetValue(key, out T v))
			target.Add(key, v = value);
		return v;
	}

	/// <summary>
	/// Tries to acquire a value from a non-generic dictionary.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	/// <returns>True if a value was acquired.</returns>
	public static bool TryGetValue<T>(this IDictionary target, object key, out T value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));

		if (target.Contains(key))
		{
            object? result = target[key];
			value = result is null ? default! : (T)result;
			return true;
		}

		value = default!;
		return false;
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the response of the valueFactory.
	/// </summary>
	public static T GetOrDefault<T>(
		this IDictionary target,
		object key,
		Func<object, T> valueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		Contract.EndContractBlock();

		return target.TryGetValue(key, out T value) ? value : valueFactory(key);
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the provided defaultValue.
	/// </summary>
	public static T GetOrDefault<T>(
		this IDictionary target,
		object key,
		T defaultValue)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		return target.TryGetValue(key, out T value) ? value : defaultValue;
	}
}
