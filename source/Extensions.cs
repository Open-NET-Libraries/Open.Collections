/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using Open.Text;
using Open.Threading;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;


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

			var expando = new ExpandoObject();
			var expandoDic = (IDictionary<string, object>)expando;

			// go through the items in the dictionary and copy over the key value pairs)

			foreach (var kvp in source)
			{
				// if the value can also be turned into an ExpandoObject, then do it!
				if (kvp.Value is IDictionary<string, object>)
				{
					var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
					expandoDic.Add(kvp.Key, expandoValue);
				}
				else if (kvp.Value is ICollection)
				{
					// iterate through the collection and convert any strin-object dictionaries
					// along the way into expando objects
					var itemList = new List<object>();
					foreach (var item in (ICollection)kvp.Value)
					{
						if (item is IDictionary<string, object>)
						{
							var expandoItem = ((IDictionary<string, object>)item).ToExpando();
							itemList.Add(expandoItem);
						}
						else
						{
							itemList.Add(item);
						}
					}


					expandoDic.Add(kvp.Key, itemList);
				}
				else
				{
					expandoDic.Add(kvp);
				}
			}

			return expando;
		}

		public static T[,] BiClone<T>(this T[,] source)
		{
			if (source == null)
				throw new NullReferenceException();

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
				throw new ArgumentNullException("target");

			source.ForEach((x, y, value) => target[x, y] = value);
		}

		public static void ForEach<T>(this T[,] source, Action<int, int, T> closure)
		{
			if (source == null)
				throw new NullReferenceException();
			if (closure == null)
				throw new ArgumentNullException("closure");

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
				throw new ArgumentNullException("closure");

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

			if (target != null)
			{
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
		}

		public static void ForEach<T>(this IEnumerable<T> target, CancellationToken token, Action<T> closure)
		{
			if (target == null)
				throw new NullReferenceException();

			if (target != null)
				foreach (var t in target)
					if (!token.IsCancellationRequested)
						closure(t);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> target)
		{
			if (target == null)
				return null;

			Random r = new Random();
			return target.OrderBy(x => (r.Next()));
		}

		// Ensures an optimized means of acquiring Any();
		public static bool HasAny<T>(this IEnumerable<T> source)
		{
			return source.HasAtLeast(1);
		}

		public static bool HasAtLeast<T>(this IEnumerable<T> source, int minimum)
		{
			if (minimum < 1)
				throw new ArgumentOutOfRangeException("minimum", minimum, "Cannot be zero or negative.");

			if (source == null)
				throw new ArgumentNullException("source");

			if (source is System.Array)
				return ((System.Array)source).Length >= minimum;

			if (source is ICollection)
				return ((ICollection)source).Count >= minimum;

			var e = source.GetEnumerator();
			try
			{
				while (e.MoveNext())
				{
					if (--minimum == 0)
						return true;
				}
				return false;
			}
			finally
			{
				(e as IDisposable)?.Dispose();
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
			item = default(T);
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

		/// <summary>
		/// Similar to a buffer but is loaded by another thread and attempts keep the buffer full while contents are being pulled.
		/// </summary>
		public static IEnumerable<T> PreCache<T>(this IEnumerable<T> source, int count = 1)
		{
			var e = source.GetEnumerator();
			var queue = new BufferBlock<T>();
			ActionBlock<bool> worker = null;

			Func<bool> tryQueue = () =>
				e.ConcurrentMoveNext(
					value => queue.Post(value),
					() => worker.Complete());

			worker = new ActionBlock<bool>(synchronousFill =>
				{ while (queue.Count < count && tryQueue() && synchronousFill); },
				// The consumers will dictate the amount of parallelism.
				new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 32 });

            worker.Completion.ContinueWith(task =>
            {
                if (task.IsFaulted)
                    ((IDataflowBlock)queue).Fault(task.Exception.InnerException);
                else
                    queue.Complete();
            });

			// The very first call (kick-off) should be synchronous.
			if(tryQueue()) while (true)
			{
                    // Is something already availaible in the queue?  Get it.
                    if (queue.TryReceive(null, out T item))
                    {
                        worker.SendAsync(true);
                        yield return item;
                    }
                    else
                    {
                        // At this point, something could be in the queue again, but let's assume not and try an trigger more.
                        if (worker.Post(true))
                        {
                            // The .Post call is 'accepted' (doesn't mean it was run).
                            // Setup the wait for recieve the next avaialable.
                            var task = queue.ReceiveAsync();
                            task.Wait();
                            if (task.IsFaulted)
                            {
                                throw task.Exception.InnerException;
                            }
                            if (!task.IsCanceled) // Cancelled means there's nothing to get.
                            {
                                // Task was not cancelled and there is a result availaible;
                                yield return task.Result;
                                continue;
                            }
                        }

                        yield break;
                    }
                }
		}


		// The idea here is a zero capacity string array is effectively imutable and will not change.  So it can be reused for comparison.
		public static readonly string[] StringArrayEmpty = new string[0];

		/// <summary>
		/// Concatentates any enumerable into a string using an optional separator.
		/// </summary>
		public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator = "")
		{
			if (source == null)
				return null;

			var b = new StringBuilder();
			bool hasSeparator = !String.IsNullOrEmpty(separator);
			bool needSeparator = false;

			foreach (T item in source)
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


			return String.Join(separator + String.Empty, array);
		}

		public static string Join(this string[] array, string separator = ",")
		{
			if (array == null)
				throw new NullReferenceException();
			if (separator == null)
				throw new ArgumentNullException("separator");

			return String.Join(separator, array);
		}



		/// <summary>
		/// Concatentates a set of values into a single string using a character as a separator.
		/// </summary>
		public static string JoinToString<T>(this IEnumerable<T> source, char separator)
		{
			if (source == null)
				throw new NullReferenceException();

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
				throw new ArgumentNullException("separator");

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

			return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();

			return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();

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
				throw new ArgumentNullException("keySelector");
			if (valueSelector == null)
				throw new ArgumentNullException("valueSelector");

			var result = new SortedDictionary<TKey, TValue>();
			foreach (var s in source)
				result.Add(keySelector(s), valueSelector(s));

			return result;
		}

		public static SortedDictionary<TKey, IEnumerable<TValue>> ToSortedDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source)
		{
			if (source == null)
				throw new NullReferenceException();

			var result = new SortedDictionary<TKey, IEnumerable<TValue>>();
			foreach (var g in source)
			{
				result.Add(g.Key, g);
			}

			return result;
		}

		public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<dynamic> source,
			Func<dynamic, TKey> keySelector, Func<dynamic, TValue> valueSelector)
		{
			if (source == null)
				throw new NullReferenceException();

			var result = new SortedDictionary<TKey, TValue>();
			foreach (var s in source)
				result.Add(keySelector(s), valueSelector(s));

			return result;
		}

		// Smilar effect can be done with .Distinct();
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			if (source == null)
				throw new NullReferenceException();

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
			if (source == target)
				return true;
			if (source == null || target == null || source.Count() != target.Count())
				return false;

			var enumSource = source.GetEnumerator();
			var enumTarget = target.GetEnumerator();

			while (enumSource.MoveNext() && enumTarget.MoveNext())
			{
				if (!enumSource.Current.Equals(enumTarget.Current))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Converts object values to their string equivalents.
		/// </summary>
		public static string[] ToStringArray<T>(this IEnumerable<T> list)
		{
			if (list == null) throw new ArgumentNullException("list");

			return list.Select(r => r.ToString()).ToArray();
		}


		/// <summary>
		/// Creates a single enumerable using the values contained.
		/// </summary>
		public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> target)
		{
			if (target == null) throw new NullReferenceException();

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

			return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
		}

		/* .Cast<T> does this...
		public static IEnumerable<T> ToGeneric<T>(this IEnumerable enumerable)
		{
			foreach (T item in enumerable)
				yield return item;
		}*/

		/// <summary>
		/// Shortcut for ordering by an "ORDER BY" string.
		/// </summary>
		public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, string orderBy)
		{
			if (collection == null) throw new NullReferenceException();

			return ParseOrderBy(orderBy).Aggregate(collection, ApplyOrderBy);
		}


		private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> collection, OrderByInfo orderByInfo)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			if (orderByInfo == null) throw new ArgumentNullException("orderByInfo");

			string[] props = orderByInfo.PropertyName.Split('.');
			Type typeT = typeof(T);
			Type type = typeT;

			ParameterExpression arg = Expression.Parameter(type, "x");
			Expression expr = arg;
			foreach (string prop in props)
			{
				// use reflection (not ComponentModel) to mirror LINQ
				PropertyInfo pi = type.GetProperty(prop);
				if (pi == null)
					throw new ArgumentException("'" + prop + "' does not exist as a property of " + type);
				expr = Expression.Property(expr, pi);
				type = pi.PropertyType;
			}
			var delegateTypeSource = typeof(Func<,>);

			var delegateTypeSourceArgs = delegateTypeSource.GetGenericArguments();

			var delegateType = delegateTypeSource.MakeGenericType(typeT, type);
			var lambda = Expression.Lambda(delegateType, expr, arg);
			string methodName;

			if (!orderByInfo.Initial && collection is IOrderedQueryable<T>)
			{
				methodName = orderByInfo.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending";
			}
			else
			{
				methodName = orderByInfo.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
			}

			//TODO: apply caching to the generic methodsinfos?
			var methods = typeof(Queryable).GetMethods();
			var r1 = methods
				.Single(
					method => method.Name == methodName
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

			if (String.IsNullOrEmpty(orderBy))
				yield break;

			string[] items = orderBy.Split(',');
			bool initial = true;
			foreach (string item in items)
			{
				string[] pair = item.Trim().Split(' ');

				if (pair.Length > 2)
					throw new ArgumentException(String.Format("Invalid OrderBy string '{0}'. Order By Format: Property, Property2 ASC, Property2 DESC", item));

				string prop = pair[0].Trim();

				if (String.IsNullOrWhiteSpace(prop))
					throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC");

				SortDirection dir = SortDirection.Ascending;

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
			if (source != null)
			{
				var result = source.Take(1).ToList();
				if (result.Any()) return result.First();
			}
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

			if (queue != null)
			{

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


		}

		public static LazyList<T> Memoize<T>(this IEnumerable<T> list, bool isEndless = false)
		{
			return new LazyList<T>(list, isEndless);
		}

		/// <summary>
		/// Similar to Cast but filters out any that don't match.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static IEnumerable<T> OfType<TSource, T>(this IEnumerable<TSource> list)
		{
			return list.Where(e => e is T).Cast<T>();
		}

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
			if (k < 0)
				throw new ArgumentOutOfRangeException("k", k, "Cannot be less than zero.");

			return k == 0 ? new[] { new T[0] } :
				elements.SelectMany((e, i) =>
					elements
						.Skip(i + (uniqueOnly ? 1 : 0))
						.Combinations(k - 1, uniqueOnly)
						.Select(c => (new[] { e }).Concat(c)));
		}
		
	}
}
