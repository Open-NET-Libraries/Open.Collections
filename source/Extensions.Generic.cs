﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
	/// NOT THREAD SAFE: Use only when a collection local or is assured single threaded.
	/// </summary>
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

	/// <summary>
	/// Adds each value to the end of the <paramref name="target"/> collection.
	/// </summary>
	public static void AddThese<T>(this ICollection<T> target, T a, T b, params T[] more)
	{
		target.Add(a);
		target.Add(b);
		if (more.Length != 0)
			target.AddRange(more);
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
				if (
				target.Remove(value))
					count++;
			}
		}

		return count;
	}

	/// <summary>
	/// Shortcut for adding a value or updating based on exising value.
	/// If no value exists, it adds the provided value.
	/// If a value exists, it sets the value using the updateValueFactory.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		T value,
		T updateValue)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

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
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key, T value,
		Func<TKey, T, T> updateValueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (updateValueFactory is null) throw new ArgumentNullException(nameof(updateValueFactory));
		Contract.EndContractBlock();

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
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		Func<TKey, T> newValueFactory,
		Func<TKey, T, T> updateValueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (newValueFactory is null) throw new ArgumentNullException(nameof(newValueFactory));
		if (updateValueFactory is null) throw new ArgumentNullException(nameof(updateValueFactory));
		Contract.EndContractBlock();

		T valueUsed;
		if (target.TryGetValue(key, out T? old))
			target[key] = valueUsed = updateValueFactory(key, old);
		else
			target.Add(key, valueUsed = newValueFactory(key));

		return valueUsed;
	}

	/// <summary>
	/// Thread safe shortcut for adding a value to list within a dictionary.
	/// </summary>
	public static void AddTo<TKey, TValue>(this IDictionary<TKey, IList<TValue>> c, TKey key, TValue value)
	{
		if (c is null) throw new ArgumentNullException(nameof(c));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		IList<TValue>? list = c.GetOrAdd(key, _ => []);
		list.Add(value);
	}

	/// <summary>
	/// Shortcut for ensuring a cacheKey contains a action.  If no action exists, it adds the provided defaultValue.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key, T defaultValue)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		if (!target.ContainsKey(key))
			target.Add(key, defaultValue);
	}

	/// <summary>
	/// Shortcut for ensuring a cacheKey contains a Value.  If no action exists, it adds it using the provided defaultValueFactory.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key,
		Func<TKey, T> defaultValueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (defaultValueFactory is null) throw new ArgumentNullException(nameof(defaultValueFactory));
		Contract.EndContractBlock();

		if (!target.ContainsKey(key))
			target.Add(key, defaultValueFactory(key));
	}

	/// <summary>
	/// Attempts to get a value from a dictionary and if no value is present, it returns the default.
	/// </summary>
	public static T GetOrDefault<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key)
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
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		Contract.EndContractBlock();

		return target.TryGetValue(key, out T? value) ? value : valueFactory(key);
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds it using the valueFactory response.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T GetOrAdd<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		Func<TKey, T> valueFactory)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));
		Contract.EndContractBlock();

		if (!target.TryGetValue(key, out T? value))
			target.Add(key, value = valueFactory(key));
		return value;
	}

	/// <summary>
	/// Tries to acquire a value from the dictionary.  If no value is present it adds the value provided.
	/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
	/// </summary>
	public static T GetOrAdd<TKey, T>(
		this IDictionary<TKey, T> target,
		TKey key,
		T value)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (key is null) throw new ArgumentNullException(nameof(key));
		Contract.EndContractBlock();

		if (!target.TryGetValue(key, out T? v))
			target.Add(key, v = value);
		return v;
	}
}
