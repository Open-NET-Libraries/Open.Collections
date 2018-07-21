/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using Open.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Open.Collections
{
	public static partial class Extensions
	{

		// Original Source: http://theburningmonk.com/2011/05/idictionarystring-object-to-expandoobject-extension-method/
		/// <summary>
		/// Extension method that turns a dictionary of string and object to an ExpandoObject
		/// </summary>
		public static ExpandoObject ToExpando(this IEnumerable<KeyValuePair<string, object>> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			var expando = new ExpandoObject();
			var expandoDic = (IDictionary<string, object>)expando;

			// go through the items in the dictionary and copy over the key value pairs)

			foreach (var kvp in source)
			{
				switch (kvp.Value)
				{
					// if the value can also be turned into an ExpandoObject, then do it!
					case IDictionary<string, object> objects:
						var expandoValue = objects.ToExpando();
						expandoDic.Add(kvp.Key, expandoValue);
						break;

					case ICollection collection:
						// iterate through the collection and convert any strin-object dictionaries
						// along the way into expando objects
						var itemList = new List<object>();
						foreach (var item in collection)
						{
							if (item is IDictionary<string, object> dictionary)
							{
								var expandoItem = dictionary.ToExpando();
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

		public static T[,] BiClone<T>(this T[,] source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			var d0 = source.GetLength(0);
			var d1 = source.GetLength(1);

			var newArray = new T[d0, d1];

			source.Overwrite(newArray);

			return newArray;
		}

		public static void Overwrite<T>(this T[,] source, T[,] target)
		{
			if (source == null)
				throw new NullReferenceException();
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			Contract.EndContractBlock();

			source.ForEach((x, y, value) => target[x, y] = value);
		}

		public static void ForEach<T>(this T[,] source, Action<int, int, T> closure)
		{
			if (source == null)
				throw new NullReferenceException();
			if (closure == null)
				throw new ArgumentNullException(nameof(closure));
			Contract.EndContractBlock();

			var d0 = source.GetLength(0);
			var d1 = source.GetLength(1);

			for (var i0 = 0; i0 < d0; i0++)
			{
				for (var i1 = 0; i1 < d1; i1++)
				{
					closure(i0, i1, source[i0, i1]);
				}
			}
		}

		public static T[] AsCopy<T>(this T[] source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			var newArray = new T[source.Length];
			for (var i = 0; i < source.Length; i++)
				newArray[i] = source[i];
			return newArray;
		}

		public static ICollection<T> AsCollection<T>(this IEnumerable<T> source)
		{
			if (source == null)
				return null;
			return source as ICollection<T> ?? source.ToArray();
		}

		/// <summary>
		/// Selective single or multiple threaded exectution.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> target, ParallelOptions parallelOptions, Action<T> closure)
		{
			if (closure == null)
				throw new ArgumentNullException(nameof(closure));
			Contract.EndContractBlock();

			if (target != null)
			{
				if (parallelOptions == null)
				{
					foreach (var t in target)
						closure(t);
				}
				else
				{
					Parallel.ForEach(
						target,
						parallelOptions,
						closure);
				}
			}
		}

		/// <summary>
		/// Selective single or multiple threaded exectution.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> target, Action<T> closure, ushort parallel)
		{
			if (target == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			target.ForEach(
				parallel == 0
				? null
				: new ParallelOptions { MaxDegreeOfParallelism = parallel },
				closure);
		}

		public static void ForEach<T>(this IEnumerable<T> target, Action<T> closure, bool allowParallel = false)
		{
			if (target == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			if (allowParallel)
			{
				Parallel.ForEach(
					target,
					closure);
			}
			else
			{
				foreach (var t in target)
					closure(t);
			}
		}

		public static void ForEach<T>(this IEnumerable<T> target, CancellationToken token, Action<T> closure)
		{
			if (target == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			foreach (var t in target)
				if (!token.IsCancellationRequested)
					closure(t);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> target)
		{
			if (target == null)
				return null;

			var r = new Random();
			return target.OrderBy(x => (r.Next()));
		}

		// Ensures an optimized means of acquiring Any();
		public static bool HasAny<T>(this IEnumerable<T> source)
		{
			return source.HasAtLeast(1);
		}

		public static bool HasAtLeast<T>(this IEnumerable<T> source, int minimum)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			if (minimum < 1)
				throw new ArgumentOutOfRangeException(nameof(minimum), minimum, "Cannot be zero or negative.");

			Contract.EndContractBlock();

			switch (source)
			{
				case T[] array:
					return array.Length >= minimum;
				case ICollection collection:
					return collection.Count >= minimum;
			}

			using (var e = source.GetEnumerator())
			{
				while (e.MoveNext())
				{
					if (--minimum == 0)
						return true;
				}
				return false;
			}

		}

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
			item = default;
			return false;
		}

		public static bool ConcurrentMoveNext<T>(this IEnumerator<T> source, Action<T> trueHandler, Action falseHandler = null)
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

		static async Task PreCacheWorker<T>(IEnumerator<T> e, Channel<T> queue)
		{
			try
			{
				using (e)
				{
					while (e.MoveNext())
					{
						var value = e.Current;

						retry:
						if (queue.Writer.TryWrite(value)) continue;
						if (await queue.Writer.WaitToWriteAsync()) goto retry;

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
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public static IEnumerable<T> PreCache<T>(this IEnumerable<T> source, int count = 1)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			Contract.EndContractBlock();

			if (count == 0)
			{
				foreach (var item in source)
					yield return item;
				yield break;
			}


			var queue = Channel.CreateBounded<T>(count);
			var e = source.GetEnumerator();

			// The very first call (kick-off) should be synchronous.
			if (e.MoveNext()) yield return e.Current;
			else yield break;

			// Queue up.
			PreCacheWorker(e, queue).ConfigureAwait(false);

			// Dequeue into the enumerable.
			do
			{
				while (queue.Reader.TryRead(out var item))
					yield return item;
			}
			while (queue.Reader.WaitToReadAsync().Result);

			var complete = queue.Reader.Completion;
			if (complete.IsFaulted)
			{
				throw complete.Exception.InnerException;
			}

		}

		/// <summary>
		/// Concatentates any enumerable into a string using an optional separator.
		/// </summary>
		public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator = "")
		{
			if (source == null)
				return null;

			var b = new StringBuilder();
			var hasSeparator = !string.IsNullOrEmpty(separator);
			var needSeparator = false;

			foreach (var item in source)
			{
				if (needSeparator)
					b.Append(separator);

				b.Append(selector(item));
				needSeparator = hasSeparator;
			}

			return b.ToString();
		}

		/// <summary>
		/// Shortcut to String.Join() using "," as a default value.
		/// </summary>
		public static string Join(this string[] array, char separator)
		{
			if (array == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();


			return string.Join(separator + string.Empty, array);
		}

		public static string Join(this string[] array, string separator = ",")
		{
			if (array == null)
				throw new NullReferenceException();
			if (separator == null)
				throw new ArgumentNullException(nameof(separator));
			Contract.EndContractBlock();

			return string.Join(separator, array);
		}



		/// <summary>
		/// Concatentates a set of values into a single string using a character as a separator.
		/// </summary>
		public static string JoinToString<T>(this IEnumerable<T> source, char separator)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return (new StringBuilder()).AppendAll(source, separator).ToString();
		}

		/// <summary>
		/// Concatentates set of values into a single string using another string as a separator.
		/// </summary>
		public static string JoinToString<T>(this IEnumerable<T> source, string separator)
		{
			if (source == null)
				throw new NullReferenceException();
			if (separator == null)
				throw new ArgumentNullException(nameof(separator));
			Contract.EndContractBlock();

			return (new StringBuilder()).AppendAll(source, separator).ToString();
		}

		/*public static T ValidateNotNull<T>(this T target)
		{

			Contract.Assume(target != null);
			return target;
		}*/

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this ParallelQuery<KeyValuePair<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			var result = new SortedDictionary<TKey, TValue>();
			foreach (var kv in source)
				result.Add(kv.Key, kv.Value);

			return result;
		}

		public static SortedDictionary<TKey, TValue> ToSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
		{
			if (source == null)
				throw new NullReferenceException();
			if (keySelector == null)
				throw new ArgumentNullException(nameof(keySelector));
			if (valueSelector == null)
				throw new ArgumentNullException(nameof(valueSelector));
			Contract.EndContractBlock();

			var result = new SortedDictionary<TKey, TValue>();
			foreach (var s in source)
				result.Add(keySelector(s), valueSelector(s));

			return result;
		}

		public static SortedDictionary<TKey, IEnumerable<TValue>> ToSortedDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			var result = new SortedDictionary<TKey, IEnumerable<TValue>>();
			foreach (var g in source)
			{
				result.Add(g.Key, g);
			}

			return result;
		}

		//public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<dynamic> source,
		//	Func<dynamic, TKey> keySelector, Func<dynamic, TValue> valueSelector)
		//{
		//	if (source == null)
		//		throw new NullReferenceException();
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

		// Smilar effect can be done with .Distinct();
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			if (source == null)
				throw new NullReferenceException();
			Contract.EndContractBlock();

			return new HashSet<T>(source);
		}



		/// <summary>
		/// Validates if the indexes and values of source array match the target.
		/// </summary>
		public static bool IsEquivalentTo<T>(this T[] source, T[] target) where T : struct
		{
			if (source == target)
				return true;

			if (source == null || target == null)
				return false;

			var sCount = source.Length;
			var tCount = target.Length;
			if (sCount != tCount)
				return false;

			for (var i = 0; i < sCount; i++)
				if (!source[i].Equals(target[i]))
					return false;

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

			if (source == null || target == null)
				return false;

			if (source is IReadOnlyCollection<T> sC && target is IReadOnlyCollection<T> tC && sC.Count != tC.Count)
				return false;

			using (var enumSource = source.GetEnumerator())
			using (var enumTarget = target.GetEnumerator())
			{
				while (enumSource.MoveNext() && enumTarget.MoveNext())
				{
					if (!enumSource.Current.Equals(enumTarget.Current))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Converts object values to their string equivalents.
		/// </summary>
		public static string[] ToStringArray<T>(this IEnumerable<T> list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			Contract.EndContractBlock();

			return list.Select(r => r.ToString()).ToArray();
		}


		/// <summary>
		/// Creates a single enumerable using the values contained.
		/// </summary>
		public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> target)
		{
			if (target == null) throw new NullReferenceException();
			Contract.EndContractBlock();

			foreach (var i in target)
				foreach (var t in i)
					yield return t;
		}

		/// <summary>
		/// Shortcut for ordering an enumerable by an "ORDER BY" string.
		/// </summary>
		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, string orderBy)
		{
			if (enumerable == null) throw new NullReferenceException();
			Contract.EndContractBlock();

			return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
		}

		/// <summary>
		/// Shortcut for ordering by an "ORDER BY" string.
		/// </summary>
		public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, string orderBy)
		{
			if (collection == null) throw new NullReferenceException();
			Contract.EndContractBlock();

			return ParseOrderBy(orderBy).Aggregate(collection, ApplyOrderBy);
		}


		private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> collection, OrderByInfo orderByInfo)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			if (orderByInfo == null) throw new ArgumentNullException(nameof(orderByInfo));
			Contract.EndContractBlock();

			var props = orderByInfo.PropertyName.Split('.');
			var typeT = typeof(T);
			var type = typeT;

			var arg = Expression.Parameter(type, "x");
			Expression expr = arg;
			foreach (var prop in props)
			{
				// use reflection (not ComponentModel) to mirror LINQ
				var pi = type.GetProperty(prop);
				if (pi == null)
					throw new ArgumentException("'" + prop + "' does not exist as a property of " + type);
				expr = Expression.Property(expr, pi);
				type = pi.PropertyType;
			}
			var delegateTypeSource = typeof(Func<,>);

			//var delegateTypeSourceArgs = delegateTypeSource.GetGenericArguments();

			var delegateType = delegateTypeSource.MakeGenericType(typeT, type);
			var lambda = Expression.Lambda(delegateType, expr, arg);
			string methodName;

			if (!orderByInfo.Initial && collection is IOrderedQueryable<T>)
			{
				methodName = orderByInfo.Direction == SortDirection.Ascending
					? "ThenBy"
					: "ThenByDescending";
			}
			else
			{
				methodName = orderByInfo.Direction == SortDirection.Ascending
					? "OrderBy"
					: "OrderByDescending";
			}

			//TODO: apply caching to the generic methodsinfos?
			var methods = typeof(Queryable).GetMethods();
			var r1 = methods
				.Single(method => method.Name == methodName
					&& method.IsGenericMethodDefinition
					&& method.GetGenericArguments().Length == 2
					&& method.GetParameters().Length == 2);

			var result = r1
				.MakeGenericMethod(typeof(T), type)
				.Invoke(null, new object[] { collection, lambda });

			return (IOrderedQueryable<T>)result;
		}

		private static IEnumerable<OrderByInfo> ParseOrderBy(string orderBy)
		{

			if (string.IsNullOrEmpty(orderBy))
				yield break;

			var items = orderBy.Split(',');
			var initial = true;
			foreach (var item in items)
			{
				var pair = item.Trim().Split(' ');

				if (pair.Length > 2)
					throw new ArgumentException(
						$"Invalid OrderBy string '{item}'. Order By Format: Property, Property2 ASC, Property2 DESC");

				var prop = pair[0].Trim();

				if (string.IsNullOrWhiteSpace(prop))
					throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC");

				var dir = SortDirection.Ascending;

				if (pair.Length == 2)
					dir = ("desc".Equals(pair[1].Trim(), StringComparison.OrdinalIgnoreCase)
						? SortDirection.Descending
						: SortDirection.Ascending);

				yield return new OrderByInfo { PropertyName = prop, Direction = dir, Initial = initial };

				initial = false;
			}
		}


		#region Nested type: OrderByInfo
		private class OrderByInfo
		{
			public string PropertyName { get; set; }
			public SortDirection Direction { get; set; }
			public bool Initial { get; set; }
		}
		#endregion


		#region Nested type: SortDirection

		#endregion

		public static T? NullableFirstOrDefault<T>(this IEnumerable<T> source)
			where T : struct
		{
			if (source == null) return null;

			var result = source.Take(1).ToList();
			if (result.Any()) return result.First();
			return null;
		}


		public static IEnumerable<T> Weave<T>(this IEnumerable<IEnumerable<T>> source)
		{
			LinkedList<IEnumerator<T>> queue = null;
			foreach (var s in source)
			{
				var e1 = s.GetEnumerator();
				if (e1.MoveNext())
				{
					yield return e1.Current;
					LazyInitializer.EnsureInitialized(ref queue);
					queue.AddLast(e1);
				}
				else
				{
					e1.Dispose();
				}
			}

			if (queue == null) yield break;

			// Start by getting the first enuerator if it exists.
			var n = queue.First;
			while (n != null)
			{
				while (n != null)
				{
					// Loop through all the enumerators..
					var e2 = n.Value;
					if (e2.MoveNext())
					{
						yield return e2.Current;
						n = n.Next;
					}
					else
					{
						// None left? Remove the node.
						var r = n;
						n = n.Next;
						queue.Remove(r);
						e2.Dispose();
					}
				}
				// Reset and try again.
				n = queue.First;
			}
		}

		public static LazyList<T> Memoize<T>(this IEnumerable<T> list, bool isEndless = false)
			=> new LazyList<T>(list, isEndless);

		/// <summary>
		/// .IndexOf extension optimized for an array.
		/// </summary>
		public static int IndexOf<T>(this T[] source, T value)
		{
			var len = source.Length;
			for (var i = 0; i < len; i++)
				if (source[i].Equals(value))
					return i;
			return -1;
		}

		// from http://stackoverflow.com/questions/127704/algorithm-to-return-all-combinations-of-k-elements-from-n
		public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k, bool uniqueOnly = false)
		{
			if (elements == null)
				throw new ArgumentNullException(nameof(elements));
			if (k < 0)
				throw new ArgumentOutOfRangeException(nameof(k), k, "Cannot be less than zero.");
			Contract.EndContractBlock();

			if (k == 0)
			{
				yield return Enumerable.Empty<T>();
			}
			else
			{
				using (var el = elements.Memoize())
				{
					foreach (var combination in el.SelectMany((e, i) => el
						.Skip(i + (uniqueOnly ? 1 : 0))
						.Combinations(k - 1, uniqueOnly)
						.Select(c => (new[] { e }).Concat(c))))
						yield return combination;
				}
			}

		}

	}
}
