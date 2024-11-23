using Microsoft.Extensions.Primitives;
using Open.Text;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Open.Collections;

/// <summary>
/// Various extension methods for operating on enumerables, collections, and spans.
/// </summary>
public static partial class Extensions
{
	// Original Source: http://theburningmonk.com/2011/05/idictionarystring-object-to-expandoobject-extension-method/
	/// <summary>
	/// Extension method that turns a dictionary of string and object to an ExpandoObject
	/// </summary>
	public static ExpandoObject ToExpando(this IEnumerable<KeyValuePair<string, object>> source)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		ExpandoObject expando = new();
		var expandoDic = (IDictionary<string, object>)expando!;

		// go through the items in the dictionary and copy over the key value pairs)

		foreach (KeyValuePair<string, object> kvp in source)
		{
			switch (kvp.Value)
			{
				// if the value can also be turned into an ExpandoObject, then do it!
				case IDictionary<string, object> objects:
					ExpandoObject? expandoValue = objects.ToExpando();
					expandoDic.Add(kvp.Key, expandoValue);
					break;

				case ICollection collection:
					// iterate through the collection and convert any string-object dictionaries
					// along the way into expando objects
					var itemList = new List<object>();
					foreach (object? item in collection)
					{
						if (item is IDictionary<string, object> dictionary)
						{
							ExpandoObject? expandoItem = dictionary.ToExpando();
							itemList.Add(expandoItem);
						}
						else
						{
							itemList.Add(item);
						}
					}

					expandoDic.Add(kvp.Key, itemList);
					break;

				default:
					expandoDic.Add(kvp);
					break;
			}
		}

		return expando;
	}

	/// <summary>
	/// Clones the source 2D array into a new 2D array.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
	public static T[,] BiClone<T>(this T[,] source)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		int d0 = source.GetLength(0);
		int d1 = source.GetLength(1);

		var newArray = new T[d0, d1];

		source.Overwrite(newArray);

		return newArray;
	}

	/// <summary>
	/// Overwrites the target 2D array with the source 2D array.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="source"/> or <paramref name="target"/> is null.</exception>
	public static void Overwrite<T>(this T[,] source, T[,] target)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		source.ForEach((x, y, value) => target[x, y] = value);
	}

	/// <summary>
	/// Iterates through the 2D array and executes the closure for each element.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="source"/> or <paramref name="closure"/> is null.</exception>
	public static void ForEach<T>(this T[,] source, Action<int, int, T> closure)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		int d0 = source.GetLength(0);
		int d1 = source.GetLength(1);

		for (int i0 = 0; i0 < d0; i0++)
		{
			for (int i1 = 0; i1 < d1; i1++)
			{
				closure(i0, i1, source[i0, i1]);
			}
		}
	}

	/// <summary>
	/// Creates a copy of the source array with a new length.
	/// </summary>
	/// <remarks>If the <paramref name="length"/> is less than the <paramref name="source"/> length, the copy will contain all elements up to that length.  If <paramref name="length"/> is greater then there will be untouched elements after that.</remarks>
	/// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
	public static T[] ToArrayOfLength<T>(this T[] source, int length)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		var newArray = new T[length];
		int len = Math.Min(newArray.Length, source.Length);
		for (int i = 0; i < len; i++)
			newArray[i] = source[i];
		return newArray;
	}

	/// <summary>
	/// Coerces to a collection either by matching the type or by creating a new array.
	/// </summary>
	public static ICollection<T> ToCollection<T>(this IEnumerable<T> source)
		=> source is null
			? null!
			: source as ICollection<T> ?? source.ToArray();

	/// <summary>
	/// Iterates over the source in parallel.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this IEnumerable<T> target, ParallelOptions? parallelOptions, Action<T> closure)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		if (parallelOptions is null)
		{
			foreach (T? t in target)
				closure(t);
			return;
		}

		Parallel.ForEach(
			target,
			parallelOptions,
			closure);
	}

	/// <summary>
	/// Iterates over the source in parallel.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this IEnumerable<T> target, Action<T> closure, ushort maxConcurrency)
		=> target.ForEach(
			maxConcurrency == 0
			? null
			: new ParallelOptions { MaxDegreeOfParallelism = maxConcurrency },
			closure);

	/// <summary>
	/// Iterates over the source and optionally can do so in parallel.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this IEnumerable<T> target, Action<T> closure, bool allowParallel = false)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		if (allowParallel)
		{
			Parallel.ForEach(
				target,
				closure);
			return;
		}

		foreach (T? t in target)
			closure(t);
	}

	/// <summary>
	/// Iterates over the source in parallel with a lock.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this ISynchronizedCollection<T> target, ParallelOptions? parallelOptions, Action<T> closure)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		target.Read(() =>
		{
			if (parallelOptions is null)
			{
				foreach (T? t in target)
					closure(t);
				return;
			}

			Parallel.ForEach(
				target,
				parallelOptions,
				closure);
		});
	}

	/// <summary>
	/// Iterates over the source in parallel with a lock.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this ISynchronizedCollection<T> target, Action<T> closure, ushort maxConcurrency)
		=> target.ForEach(
			maxConcurrency == 0
			? null
			: new ParallelOptions { MaxDegreeOfParallelism = maxConcurrency },
			closure);

	/// <summary>
	/// Iterates over the source with a lock and optionally can do so in parallel.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this ISynchronizedCollection<T> target, Action<T> closure, bool allowParallel = false)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		target.Read(() =>
		{
			if (allowParallel)
			{
				Parallel.ForEach(
					target,
					closure);
				return;
			}

			foreach (T? t in target)
				closure(t);
		});
	}

	/// <summary>
	/// Iterates over the source and can be cancelled.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
#pragma warning disable IDE0079 // Remove unnecessary suppression
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Allows for simpler implementation. Other methods cover non-cancellable case.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
	public static void ForEach<T>(this IEnumerable<T> target, CancellationToken token, Action<T> closure)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		foreach (T? t in target)
		{
			if (!token.IsCancellationRequested)
				closure(t);
		}
	}

	/// <summary>
	/// Iterates over the source with a lock and can be cancelled.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="target"/> or <paramref name="closure"/> are null.</exception>
	public static void ForEach<T>(this ISynchronizedCollection<T> target, CancellationToken token, Action<T> closure)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		if (closure is null) throw new ArgumentNullException(nameof(closure));
		Contract.EndContractBlock();

		target.Read(() =>
		{
			foreach (T? t in target)
			{
				if (!token.IsCancellationRequested)
					closure(t);
			}
		});
	}

	/// <summary>
	/// Randomizes the order of the source.
	/// </summary>
	public static IEnumerable<T> Shuffle<T>(
		this IEnumerable<T> target, Random? rnd = null)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		var r = rnd ?? new Random();
		return target.OrderBy(_ => r.Next());
	}

	/// <summary>
	/// Tests the count of the source to see if there's any items.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="source"/> is null.</exception>
	/// <remarks>
	/// First checks the type to see if a count can be aquired directly.  If not, it will iterate through the source to count the items.
	/// </remarks>
	public static bool HasAny<T>(this IEnumerable<T> source)
		=> source.HasAtLeast(1);

	/// <summary>
	/// Tests the count of the source to see if there's at least the <paramref name="minimum"/> number of items.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">If the <paramref name="minimum"/> is less than 1.</exception>
	/// <inheritdoc cref="HasAny{T}(IEnumerable{T})"/>
	public static bool HasAtLeast<T>(this IEnumerable<T> source, int minimum)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));
		if (minimum < 1)
			throw new ArgumentOutOfRangeException(nameof(minimum), minimum, "Cannot be zero or negative.");
		Contract.EndContractBlock();

		switch (source)
		{
			case T[] array:
				return array.Length >= minimum;
			case IReadOnlyCollection<T> collection:
				return collection.Count >= minimum;
			case ICollection collection:
				return collection.Count >= minimum;
			case ICollection<T> collection:
				return collection.Count >= minimum;
		}

		using IEnumerator<T>? e = source.GetEnumerator();
		while (e.MoveNext())
		{
			if (--minimum == 0)
				return true;
		}
		return false;
	}

	/// <summary>
	/// Synchronizes enumerting by locking on the enumerator.
	/// </summary>
	public static bool ConcurrentTryMoveNext<T>(this IEnumerator<T> source, out T item)
	{
		// Always lock on next to prevent concurrency issues.
		lock (source) // a standard enumerable can't handle concurrency.
		{
			if (source.MoveNext())
			{
				item = source.Current;
				return true;
			}
		}

		item = default!;
		return false;
	}

	/// <summary>
	/// Syncronizes enumerting by locking on the enumerator and invokes the provided handlers depending on if .MoveNext() was true.
	/// </summary>
	public static bool ConcurrentMoveNext<T>(this IEnumerator<T> source, Action<T> trueHandler, Action? falseHandler = null)
	{
		// Always lock on next to prevent concurrency issues.
		lock (source) // a standard enumerable can't handle concurrency.
		{
			if (source.MoveNext())
			{
				trueHandler(source.Current);
				return true;
			}
		}
		falseHandler?.Invoke();
		return false;
	}

	static async Task PreCacheWorker<T>(IEnumerator<T> e, Channel<T> queue, CancellationToken cancellationToken)
	{
		try
		{
			using (e)
			{
				while (e.MoveNext())
				{
					T? value = e.Current;

retry:
					if (queue.Writer.TryWrite(value)) continue;
					if (await queue.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false)) goto retry;

					break;
				}
			}

			queue.Writer.Complete();
		}
		catch (Exception ex)
		{
			queue.Writer.TryComplete(ex);
		}
	}

	/// <summary>
	/// Similar to a buffer but is loaded by another thread and attempts keep the buffer full while contents are being pulled.
	/// </summary>
	public static IEnumerable<T> PreCache<T>(this IEnumerable<T> source, int count = 1, CancellationToken cancellationToken = default)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return PreCacheCore(source, count, cancellationToken);

		static IEnumerable<T> PreCacheCore(IEnumerable<T> source, int count, CancellationToken cancellationToken)
		{
			if (count <= 0)
			{
				foreach (T? item in source)
					yield return item;
				yield break;
			}

			var queue = Channel.CreateBounded<T>(count);
			IEnumerator<T>? e = source.GetEnumerator();

			// The very first call (kick-off) should be synchronous.
			if (e.MoveNext()) yield return e.Current;
			else yield break;

			// Queue up.
			PreCacheWorker(e, queue, cancellationToken).ConfigureAwait(false);

			// Dequeue into the enumerable.
			do
			{
				while (queue.Reader.TryRead(out T? item))
					yield return item;
			}
			while (queue.Reader.WaitToReadAsync(cancellationToken).AsTask().Result);

			Task complete = queue.Reader.Completion;
			if (complete.IsFaulted)
			{
				throw complete.Exception.InnerException ?? complete.Exception;
			}
		}
	}

	/// <summary>
	/// Concatenates any enumerable into a string using an optional separator.
	/// </summary>
	public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator = "")
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		var b = new StringBuilder();
		bool hasSeparator = !string.IsNullOrEmpty(separator);
		bool needSeparator = false;

		foreach (T? item in source)
		{
			if (needSeparator)
				b.Append(separator);

			b.Append(selector(item));
			needSeparator = hasSeparator;
		}

		return b.ToString();
	}

	/*public static T ValidateNotNull<T>(this T target)
	{

		Contract.Assume(target is not null);
		return target;
	}*/

#if NET9_0_OR_GREATER
#else
	/// <summary>
	/// Returns a dictionary from the source key-value pairs.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="source"/> is null.</exception>
	public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		where TKey : notnull
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}
#endif

	/// <summary>
	/// Converts an enumerable to a sorted dictionary.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="source"/> is null.</exception>
	public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		where TKey : notnull
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		var result = new SortedDictionary<TKey, TValue>();
		foreach (KeyValuePair<TKey, TValue> kv in source)
			result.Add(kv.Key, kv.Value);

		return result;
	}

	/// <summary>
	/// Converts an enumerable to a sorted dictionary.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="source"/>, <paramref name="keySelector"/>, or <typeparamref name="TValue"/> are null.</exception>
	public static SortedDictionary<TKey, TValue> ToSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
		where TKey : notnull
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));
		if (valueSelector is null) throw new ArgumentNullException(nameof(valueSelector));
		Contract.EndContractBlock();

		var result = new SortedDictionary<TKey, TValue>();
		foreach (TSource? s in source)
			result.Add(keySelector(s), valueSelector(s));

		return result;
	}

	/// <summary>
	/// Converts a grouping to a sorted dictionary.
	/// </summary>
	/// <exception cref="ArgumentNullException">If the <paramref name="source"/> is null.</exception>
	public static SortedDictionary<TKey, IEnumerable<TValue>> ToSortedDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source)
		where TKey : notnull
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		Contract.EndContractBlock();

		var result = new SortedDictionary<TKey, IEnumerable<TValue>>();
		foreach (IGrouping<TKey, TValue>? g in source)
		{
			result.Add(g.Key, g);
		}

		return result;
	}

	//public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<dynamic> source,
	//	Func<dynamic, TKey> keySelector, Func<dynamic, TValue> valueSelector)
	//{
	//  if (source is null) throw new ArgumentNullException(nameof(source));
	//	Contract.EndContractBlock();

	//	var result = new SortedDictionary<TKey, TValue>();
	//	foreach (var s in source)
	//	{
	//		var key = keySelector(s);
	//		var value = valueSelector(s);
	//		result.Add(key, value);
	//	}

	//	return result;
	//}

	/// <summary>
	/// Validates if the indexes and values of source array match the target.
	/// </summary>
	public static bool IsEquivalentTo<T>(this T[] source, T[] target) where T : struct
	{
		if (source == target)
			return true;

		if (source is null || target is null)
			return false;

		int sCount = source.Length;
		int tCount = target.Length;
		if (sCount != tCount)
			return false;

		for (int i = 0; i < sCount; i++)
		{
			if (!source[i].Equals(target[i]))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Validates if the positions/indexes and values of source match the target.
	/// </summary>
	public static bool IsEquivalentTo<T>(this IEnumerable<T> source, IEnumerable<T> target) where T : struct
	{
		// ReSharper disable once PossibleUnintendedReferenceComparison
		if (source == target)
			return true;

		if (source is null || target is null)
			return false;

		// Both are not null, okay go.
		return source.SequenceEqual(target);
	}

	/// <summary>
	/// Converts object values to their string equivalents.
	/// </summary>
	public static string[] ToStringArray<T>(this IEnumerable<T> list)
	{
		if (list is null) throw new ArgumentNullException(nameof(list));
		Contract.EndContractBlock();

		return list.Select(r => r!.ToString() ?? "null").ToArray();
	}

	/// <summary>
	/// Creates a single enumerable using the values contained.
	/// </summary>
	public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> target)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		return MergeCore(target);

		static IEnumerable<T> MergeCore(IEnumerable<IEnumerable<T>> target)
		{
			foreach (IEnumerable<T>? i in target)
			{
				foreach (T? t in i)
					yield return t;
			}
		}
	}

	/// <summary>
	/// Shortcut for ordering an enumerable by an "ORDER BY" string.
	/// </summary>
	public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, StringSegment orderBy)
	{
		if (enumerable is null) throw new ArgumentNullException(nameof(enumerable));
		Contract.EndContractBlock();

		return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
	}

	/// <summary>
	/// Shortcut for ordering by an "ORDER BY" string.
	/// </summary>
	public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, StringSegment orderBy)
	{
		if (queryable is null) throw new ArgumentNullException(nameof(queryable));
		Contract.EndContractBlock();

		return ParseOrderBy(orderBy).Aggregate(queryable, ApplyOrderBy);
	}

	/// <summary>
	/// Returns an enumerable with the values from the source in the order provided.
	/// </summary>
	public static IEnumerable<T> Arrange<T>(this IReadOnlyList<T> source, IEnumerable<int> order)
	{
		foreach (int i in order)
			yield return source[i];
	}

	private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> collection, OrderByInfo orderByInfo)
	{
		if (collection is null) throw new ArgumentNullException(nameof(collection));
		if (orderByInfo is null) throw new ArgumentNullException(nameof(orderByInfo));
		Contract.EndContractBlock();

		IEnumerable<string>? props = orderByInfo.PropertyName.SplitAsSegments('.').Select(s => s.ToString());
		Type? typeT = typeof(T);
		Type? type = typeT;

		ParameterExpression? arg = Expression.Parameter(type, "x");
		Expression expr = arg;
		foreach (string? prop in props)
		{
			// use reflection (not ComponentModel) to mirror LINQ
			System.Reflection.PropertyInfo? pi
				= type.GetProperty(prop) ?? throw new ArgumentException("'" + prop + "' does not exist as a property of " + type);
			expr = Expression.Property(expr, pi);
			type = pi.PropertyType;
		}
		Type? delegateTypeSource = typeof(Func<,>);

		//var delegateTypeSourceArgs = delegateTypeSource.GetGenericArguments();

		Type? delegateType = delegateTypeSource.MakeGenericType(typeT, type);
		LambdaExpression? lambda = Expression.Lambda(delegateType, expr, arg);

		string methodName = !orderByInfo.Initial && collection is IOrderedQueryable<T>
				? orderByInfo.Direction == SortDirection.Ascending
					? "ThenBy"
					: "ThenByDescending"
				: orderByInfo.Direction == SortDirection.Ascending
					? "OrderBy"
					: "OrderByDescending";

		//TODO: apply caching to the generic methodsinfos?
		System.Reflection.MethodInfo[]? methods = typeof(Queryable).GetMethods();
		System.Reflection.MethodInfo? r1 = methods
			.Single(method => method.Name == methodName
				&& method.IsGenericMethodDefinition
				&& method.GetGenericArguments().Length == 2
				&& method.GetParameters().Length == 2);

		object? result = r1
			.MakeGenericMethod(typeof(T), type)
			.Invoke(null, [collection, lambda]);

		Debug.Assert(result is not null);
		return (IOrderedQueryable<T>)result;
	}

	private static IEnumerable<OrderByInfo> ParseOrderBy(StringSegment orderBy)
	{
		if (!orderBy.HasValue || orderBy.Length == 0)
			yield break;

		IEnumerable<StringSegment>? items = orderBy.SplitAsSegments(',');
		bool initial = true;
		foreach (StringSegment item in items)
		{
			StringSegment name = default;
			SortDirection dir = SortDirection.Ascending;

			int i = 0;
			foreach (StringSegment segment in item.Trim().SplitAsSegments(' '))
			{
				switch (i)
				{
					case 0:
						name = segment.Trim();
						if (name.Length == 0)
							throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC");
						break;

					case 1:
						dir = "desc".Equals(segment.Trim(), StringComparison.OrdinalIgnoreCase)
							? SortDirection.Descending
							: SortDirection.Ascending;
						break;

					default:
					{
						throw new ArgumentException(
							$"Invalid OrderBy string '{item}'. Order By Format: Property, Property2 ASC, Property2 DESC");
					}
				}
				i++;
			}

			yield return new OrderByInfo { PropertyName = name, Direction = dir, Initial = initial };
			initial = false;
		}
	}

	#region Nested type: OrderByInfo
	private class OrderByInfo
	{
		public StringSegment PropertyName { get; set; }
		public SortDirection Direction { get; set; }
		public bool Initial { get; set; }
	}
	#endregion

	/// <summary>
	/// A nullable struct version of FirstOrDefault.
	/// </summary>
	public static T? NullableFirstOrDefault<T>(this IEnumerable<T> source)
		where T : struct
	{
		if (source is null) return null;

		foreach (T e in source)
			return e;

		return null;
	}

	/// <summary>
	/// Rotates through each enumerable and returns the next value until none are left.
	/// </summary>
	public static IEnumerable<T> Weave<T>(this IEnumerable<IEnumerable<T>> source)
	{
		LinkedList<IEnumerator<T>>? queue = null;
		foreach (IEnumerable<T>? s in source)
		{
			IEnumerator<T>? e1 = s.GetEnumerator();
			if (e1.MoveNext())
			{
				yield return e1.Current;
				LazyInitializer.EnsureInitialized(ref queue)!.AddLast(e1);
			}
			else
			{
				e1.Dispose();
			}
		}

		if (queue is null) yield break;

		// Start by getting the first enuerator if it exists.
		LinkedListNode<IEnumerator<T>>? n = queue.First;
		while (n is not null)
		{
			while (n is not null)
			{
				// Loop through all the enumerators..
				IEnumerator<T>? e2 = n.Value;
				if (e2.MoveNext())
				{
					yield return e2.Current;
					n = n.Next;
				}
				else
				{
					// None left? Remove the node.
					LinkedListNode<IEnumerator<T>>? r = n;
					n = n.Next;
					queue.Remove(r);
					e2.Dispose();
				}
			}
			// Reset and try again.
			n = queue.First;
		}
	}

	/// <summary>
	/// Caches the results up to the last index requested.
	/// </summary>
	/// <param name="list">The source enumerable to cache.</param>
	/// <param name="isEndless">When true, will throw an InvalidOperationException if anything causes the list to evaluate to completion.</param>
	/// <returns>A LazyList<typeparamref name="T"/> for accessing the cached results.</returns>
	public static LazyList<T> Memoize<T>(this IEnumerable<T> list, bool isEndless = false) => new(list, isEndless);

	/// <summary>
	/// <para>Caches the results up to the last index requested.</para>
	/// <para>
	/// WARNING:
	/// - Is not thread safe.
	/// - There is a risk of recursion.
	/// - An endless enumerable may result in a stack overflow.
	/// </para>
	/// <para>If any of the above are of concern, then use .Memoize() instead.</para>
	/// </summary>
	/// <param name="list">The source enumerable to cache.</param>
	/// <returns>A LazyList<typeparamref name="T"/> for accessing the cached results.</returns>
	public static LazyListUnsafe<T> MemoizeUnsafe<T>(this IEnumerable<T> list) => new(list);

	/// <summary>
	/// .IndexOf extension optimized for an array.
	/// </summary>
	public static int IndexOf<T>(this T[] source, T value)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));

		int len = source.Length;
		for (int i = 0; i < len; i++)
		{
			if (source[i]?.Equals(value) ?? value is null)
				return i;
		}
		return -1;
	}

	/// <summary>
	/// Optimized method for getting a node by index
	/// </summary>
	public static LinkedListNode<T> GetNodeAt<T>(this LinkedList<T> list, int index)
	{
		LinkedListNode<T> current;

		// Determine whether to start from the beginning or the end.
		int count = list.Count;
		if (index < count / 2)
		{
			// Start from the beginning
			current = list.First!;
			for (int i = 0; i < index; i++)
				current = current.Next!;

			return current;
		}

		// Start from the end
		current = list.Last!;
		for (int i = count - 1; i > index; i--)
			current = current.Previous!;

		return current;
	}

	/// <summary>
	/// Copies the results to the provided span up to its length or until the end of the results.
	/// </summary>
	/// <param name="source">The source enumerable.</param>
	/// <param name="target">The span to copy to.</param>
	/// <returns>
	/// A span representing the results.
	/// If the count was less than the target length, a new span representing the results.
	/// Otherwise the target is returned.
	/// </returns>
	public static Span<T> CopyToSpan<T>(this IEnumerable<T> source, Span<T> target)
	{
		int tLen = target.Length;
		if (tLen == 0) return target;

		if (source is T[] a)
		{
			int sLen = a.Length;
			if (tLen < sLen)
			{
				a.AsSpan(0, tLen).CopyTo(target);
				return target;
			}

			a.CopyTo(target);
			return tLen == sLen ? target : target.Slice(0, sLen);
		}

		int count = 0;
		foreach (T? e in source)
		{
			target[count] = e;
			if (tLen == ++count) return target;
		}

		return tLen == count ? target : target.Slice(0, count);
	}

	/// <summary>
	/// Builds an immutable array using the contents of the span.
	/// </summary>
	public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> span) => [.. span];

	/// <inheritdoc cref="ToImmutableArray{T}(ReadOnlySpan{T})"/>
	public static ImmutableArray<T> ToImmutableArray<T>(this Span<T> span)
		=> [.. span];

	/// <summary>
	/// Builds an immutable array using the contents of the memory.
	/// </summary>
	public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlyMemory<T> memory)
		=> memory.Span.ToImmutableArray();

	/// <inheritdoc cref="ToImmutableArray{T}(ReadOnlyMemory{T})"/>
	public static ImmutableArray<T> ToImmutableArray<T>(this Memory<T> memory)
		=> memory.Span.ToImmutableArray();

	/// <summary>
	/// Builds an readonly collection using the contents of the span.
	/// </summary>
	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this ReadOnlySpan<T> span)
		=> Array.AsReadOnly(span.ToArray());

	/// <inheritdoc cref="ToReadOnlyCollection{T}(ReadOnlySpan{T})"/>
	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this Span<T> span)
		=> Array.AsReadOnly(span.ToArray());

	/// <summary>
	/// Builds an immutable array using the contents of the memory.
	/// </summary>
	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this ReadOnlyMemory<T> memory)
		=> memory.Span.ToReadOnlyCollection();

	/// <inheritdoc cref="ToImmutableArray{T}(ReadOnlyMemory{T})"/>
	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this Memory<T> memory)
		=> memory.Span.ToReadOnlyCollection();

	/// <summary>
	/// Executes an action before the enumerable is consumed.
	/// </summary>
	public static IEnumerable<T> Preflight<T>(
		this IEnumerable<T> source,
		Action before)
	{
		before();
		foreach (var item in source)
			yield return item;
	}

	/// <summary>
	/// Executes an action before the enumerable is consumed.
	/// </summary>
	public static IEnumerator<T> Preflight<T>(
		this IEnumerator<T> source,
		Action before)
	{
		before();
		while (source.MoveNext())
			yield return source.Current;
	}

	/// <summary>
	/// Executes an action when the begins <paramref name="source"/> enumeration.
	/// </summary>
	public static IEnumerable<T> BeforeGetEnumerator<T>(
		this IEnumerable<T> source,
		Action before)
		=> new PreflightEnumerable<T>(source, before);

	private class PreflightEnumerable<T>(
		IEnumerable<T> source, Action before)
		: IEnumerable<T>
	{
		private readonly IEnumerable<T> _source = source ?? throw new ArgumentNullException(nameof(source));

		private readonly Action _before = before ?? throw new ArgumentNullException(nameof(before));

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			_before();
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	private const string MustBeAtLeast0 = "Must be at least 0.";
	private const string MustBeLessThanTheSize = "Must be less than the size.";

	/// <summary>
	/// Creates a segment from the array.
	/// </summary>
	public static ArraySegment<T> AsSegment<T>(this T[] array)
		=> new(array);

	/// <summary>
	/// Creates a segment from the array starting at the specified <paramref name="offset"/>.
	/// </summary>
	public static ArraySegment<T> AsSegment<T>(this T[] array, int offset)
	{
		if (array is null) throw new ArgumentNullException(nameof(array));
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeAtLeast0);
		if (offset > array.Length)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeLessThanTheSize);
		Contract.EndContractBlock();

		return new(array, offset, array.Length - offset);
	}

	/// <summary>
	/// Creates a segment from the array starting at the specified <paramref name="offset"/> and extending for the <paramref name="count"/>.
	/// </summary>
	public static ArraySegment<T> AsSegment<T>(this T[] array, int offset, int count)
	{
		if (array is null) throw new ArgumentNullException(nameof(array));
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeAtLeast0);
		Contract.EndContractBlock();

		return new(array, offset, count);
	}

#if NETSTANDARD2_0
	/// <summary>
	/// Enumerates the source segment.
	/// </summary>
	public static IEnumerator<T> GetEnumerator<T>(this ArraySegment<T> source)
	{
		int start = source.Offset;
		int end = start + source.Count;
		for (int i = start; i < end; i++)
			yield return source.Array[i];
	}

	/// <summary>
	/// Forms a slice out of the <paramref name="array"/> segment starting at the specified <paramref name="offset"/>.
	/// </summary>
	public static ArraySegment<T> Slice<T>(this ArraySegment<T> array, int offset)
	{
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeAtLeast0);
		if (offset > array.Count)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeLessThanTheSize);
		Contract.EndContractBlock();

		return new ArraySegment<T>(array.Array, array.Offset + offset, array.Count - offset);
	}

	/// <summary>
	/// Forms a slice out of the <paramref name="source"/> segment
	/// starting at the specified <paramref name="offset"/>
	/// and extending for the<paramref name = "count" />.
	/// </summary>
	public static ArraySegment<T> Slice<T>(this ArraySegment<T> source, int offset, int count)
	{
		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeAtLeast0);
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), count, MustBeAtLeast0);
		if (offset > source.Count)
			throw new ArgumentOutOfRangeException(nameof(offset), offset, MustBeLessThanTheSize);
		Contract.EndContractBlock();

		return new ArraySegment<T>(source.Array, source.Offset + offset, count);
	}
#endif
}
