using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public static partial class Extensions
{
	/// <summary>
	/// Will remove an entry if the value is null or matches the default type value.
	/// Otherwise will set the value.
	/// </summary>
	public static void SetOrRemove<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		T value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		if (value is null || value.Equals(default(T)))
			target.Remove(key);
		else
			target[key] = value;
	}

	/// <summary>
	/// Adds a value to list only if it does not exist.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static void Register<T>(this ICollection<T> target, T value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		if (!target.Contains(value))
			target.Add(value);
	}

	/// <summary>
	/// Adds each value to the end of the <paramref name="target"/> collection.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="target"/> is null.</exception>
	public static void AddRange<T>(
		this ICollection<T> target,
		IEnumerable<T> values)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		if (values is null)
			return;

		foreach (T? value in values)
			target.Add(value);
	}

	/// <inheritdoc cref="AddRange{T}(ICollection{T}, IEnumerable{T})"/>
#if NET9_0_OR_GREATER
	[OverloadResolutionPriority(1)]
#endif
	public static void AddRange<T>(
		this ICollection<T> target,
		ReadOnlySpan<T> values)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		foreach (T value in values)
			target.Add(value);
	}

	/// <summary>
	/// Adds each value to the end of the <paramref name="target"/> collection.
	/// </summary>
#if NET9_0_OR_GREATER
	public static void AddThese<T>(this ICollection<T> target, T a, T b, params ReadOnlySpan<T> more)
#else
	public static void AddThese<T>(this ICollection<T> target, T a, T b, params T[] more)
#endif
	{
		target.Add(a);
		target.Add(b);
		foreach (T value in more)
			target.Add(value);
	}

	/// <summary>
	/// Removes each value from the <paramref name="target"/> collection.
	/// </summary>
	public static int Remove<T>(this ICollection<T> target, IEnumerable<T> values)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		int count = 0;
		if (values is not null)
		{
			foreach (T? value in values)
			{
				if (target.Remove(value))
					count++;
			}
		}

		return count;
	}

	/// <summary>
	/// Shortcut for adding a value or updating based on exising value.
	/// If no value exists, it adds the provided value.
	/// If a value exists, it sets the value using the updateValueFactory.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		T value,
		T updateValue)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(d, key);
			return System.Runtime.CompilerServices.Unsafe.IsNullRef(ref val)
				? (val = value)
				: (val = updateValue);
		}
#endif

		T valueUsed;
		if (target.TryGetValue(key, out _))
			target[key] = valueUsed = updateValue;
		else
			target.Add(key, valueUsed = value);

		return valueUsed;
	}

	/// <summary>
	/// Shortcut for adding a value or updating based on exising value.
	/// If no value exists, it adds the provided value.
	/// If a value exists, it sets the value using the updateValueFactory.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key, T value,
		Func<TKey, T, T> updateValueFactory)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (updateValueFactory is null) throw new ArgumentNullException(nameof(updateValueFactory));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(d, key);
			return System.Runtime.CompilerServices.Unsafe.IsNullRef(ref val)
				? (val = value)
				: (val = updateValueFactory(key, val));
		}
#endif

		T valueUsed;
		if (target.TryGetValue(key, out T? old))
			target[key] = valueUsed = updateValueFactory(key, old);
		else
			target.Add(key, valueUsed = value);

		return valueUsed;
	}

	/// <summary>
	/// Shortcut for adding a value or updating based on exising value.
	/// If no value exists, it adds the value using the newValueFactory.
	/// If a value exists, it sets the value using the updateValueFactory.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		Func<TKey, T> newValueFactory,
		Func<TKey, T, T> updateValueFactory)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (newValueFactory is null) throw new ArgumentNullException(nameof(newValueFactory));
		if (updateValueFactory is null) throw new ArgumentNullException(nameof(updateValueFactory));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(d, key);
			return System.Runtime.CompilerServices.Unsafe.IsNullRef(ref val)
				? (val = newValueFactory(key))
				: (val = updateValueFactory(key, val));
		}
#endif

		T valueUsed;
		if (target.TryGetValue(key, out T? old))
			target[key] = valueUsed = updateValueFactory(key, old);
		else
			target.Add(key, valueUsed = newValueFactory(key));

		return valueUsed;
	}

	/// <summary>
	/// Shortcut for adding a value to list within a dictionary.
	/// </summary>
	public static void AddTo<TKey, TValue>(this IDictionary<TKey, IList<TValue>> c, TKey key, TValue value)
		where TKey : notnull
	{
		if (c is null) throw new ArgumentNullException(nameof(c));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		IList<TValue>? list = c.GetOrAdd(key, _ => []);
		list.Add(value);
	}

	/// <summary>
	/// Shortcut for ensuring a cacheKey contains a action.  If no value exists, it adds the provided defaultValue.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	[Obsolete("Use TryAdd instead.")]
	public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key, T defaultValue)
		where TKey : notnull
		=> TryAdd(target, key, defaultValue);

	/// <summary>
	/// Shortcut for ensuring a cacheKey contains a Value.  If no value exists, it adds it using the provided defaultValueFactory.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	[Obsolete("Use TryAdd instead.")]
	public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		Func<TKey, T> defaultValueFactory)
		where TKey : notnull
		=> TryAdd(target, key, defaultValueFactory);

	/// <summary>
	/// Attempts to add a value to a dictionary if it does not already exist.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static bool TryAdd<TKey, T>(this IDictionary<TKey, T> target, TKey key, T value)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(d, key, out bool exists);
			if (!exists) val = value;
			return !exists;
		}
#endif
		if (target.ContainsKey(key))
			return false;

		target.Add(key, value);
		return true;
	}

	/// <inheritdoc cref="TryAdd{TKey, T}(IDictionary{TKey, T}, TKey, T)"/>
	public static bool TryAdd<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		Func<TKey, T> defaultValueFactory)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (defaultValueFactory is null) throw new ArgumentNullException(nameof(defaultValueFactory));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(d, key, out bool exists);
			if (!exists) val = defaultValueFactory(key);
			return !exists;
		}
#endif
		if (target.ContainsKey(key))
			return false;

		target.Add(key, defaultValueFactory(key));
		return true;
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the default.
	/// </summary>
	public static T GetOrDefault<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		return target.GetOrDefault(key, default(T)!);
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the provided defaultValue.
	/// </summary>
	public static T GetOrDefault<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		T defaultValue)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		return target.TryGetValue(key, out T? value) ? value : defaultValue;
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the response of the valueFactory.
	/// </summary>
	public static T GetOrDefault<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		Func<TKey, T> valueFactory)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		Contract.EndContractBlock();

		return target.TryGetValue(key, out T? value) ? value : valueFactory(key);
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds it using the valueFactory response.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static T GetOrAdd<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		Func<TKey, T> valueFactory)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(d, key, out bool exists);
			if(!exists) val = valueFactory(key);
			return val!;
		}
#endif
		if (!target.TryGetValue(key, out var value))
			target.Add(key, value = valueFactory(key));

		return value;
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds the value provided.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	public static T GetOrAdd<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key, T value)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(d, key, out bool exists);
			if (!exists) val = value;
			return val!;
		}
#endif

		if (!target.TryGetValue(key, out T? v))
			target.Add(key, v = value);
		return v;
	}

	/// <summary>
	/// Tries to update an existing value in the dictionary.
	/// </summary>
	/// <remarks>NOT THREAD SAFE: Use only when a dictionary is assured to be single threaded.</remarks>
	/// <returns>
	/// <see langword="true"/> if the value was overwritten;
	/// <see langword="false"/> if there was no original value
	/// or if <paramref name="compareExisting"/> is <see langword="true"/> and the values are equal.
	/// </returns>
	public static bool TryUpdate<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key, T value,
		bool compareExisting = false)
		where TKey : notnull
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

#if NET9_0_OR_GREATER
		if (target is Dictionary<TKey, T> d)
		{
			ref var val = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(d, key);
			if(System.Runtime.CompilerServices.Unsafe.IsNullRef(ref val))
				return false;

			if(compareExisting && !AreEqual(val, value))
				return false;

			val = value;
			return true;
		}
#endif

		if (!target.TryGetValue(key, out var v))
			return false;

		if (compareExisting && !AreEqual(v, value))
			return false;

		target[key] = value;
		return true;

		static bool AreEqual(T a, T b) => a is null ? b is null : a.Equals(b);
	}
}
