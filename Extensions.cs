/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Open/blob/dotnet-core/LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Threading;
using Open.Text;


namespace Open.Collections
{
	public static class Extensions
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

		const int SYNC_TIMEOUT_DEFAULT_MILLISECONDS = 10000;

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

		#region Join to String extensions
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
		#endregion


		#region ToByteArray extensions
		/// <summary>
		/// Converts a string to a byte array.
		/// </summary>
		/// <param name="encoding">Default is UTF8.</param>
		public static byte[] ToByteArray(this string value, Encoding encoding = null)
		{
			if (value == null)
				throw new NullReferenceException();

			return (encoding ?? Encoding.UTF8).GetBytes(value);
		}

		/// <summary>
		/// Converts a string to a sbyte array.
		/// </summary>
		/// <param name="encoding">Default is UTF8.</param>
		public static sbyte[] ToSbyteArray(this string value, Encoding encoding = null)
		{
			if (value == null)
				throw new NullReferenceException();

			return value.ToByteArray(encoding).ToSbyteArray();
		}

		/// <summary>
		/// Directly converts a byte array (byte-by-byte) to an sbyte array.
		/// </summary>
		public static sbyte[] ToSbyteArray(this byte[] bytes)
		{
			if (bytes == null)
				throw new NullReferenceException();

			var sbytes = new sbyte[bytes.Length];
			for (int i = 0; i < bytes.Length; i++)
				sbytes[i] = (sbyte)bytes[i];

			return sbytes;
		}
		#endregion

		/// <summary>
		/// Copies the source stream to the target.
		/// </summary>
		public static void CopyTo(this Stream source, Stream target)
		{
			if (source == null)
				throw new NullReferenceException();
			if (target == null)
				throw new ArgumentNullException("target");

			byte[] bytes = new byte[4096];

			int cnt;
			while ((cnt = source.Read(bytes, 0, bytes.Length)) != 0)
				target.Write(bytes, 0, cnt);
		}


        #region ICollection extensions
        /// <summary>
        /// Adds a value to list only if it does not exist.
        /// NOT THREAD SAFE: Use only when a collection local or is assured single threaded.
        /// </summary>
        public static void Register<T>(this ICollection<T> target, T value)
		{
			if (target == null) throw new NullReferenceException();

			if (!target.Contains(value))
				target.Add(value);
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

		public static void Add<T>(this ICollection<T> target, IEnumerable<T> values)
		{
			if (target == null)
                throw new NullReferenceException();

			if (values != null)
				foreach (var value in values)
					target.Add(value);
		}

        public static void Add<T>(this ICollection<T> target, T a, T b, params T[] more)
        {
            target.Add(a);
            target.Add(b);
            target.Add(more);
        }


        public static int Remove<T>(this ICollection<T> target, IEnumerable<T> values)
		{
			if (target == null) throw new NullReferenceException();
			int count = 0;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (
					target.Remove(value))
						count++;
				}
			}
			return count;
		}

		#endregion


		/// <summary>
		/// Thread safe value for syncronizing acquiring a value from a non-generic dictionary.
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
		/// Will remove an entry if the value is null or matches the default type value.
		/// Otherwise will set the value.
		/// </summary>
		public static void SetOrRemove<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			T value)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			if (value == null || value.Equals(default(T)))
				target.Remove(key);
			else
				target[key] = value;
		}

		/// <summary>
		/// Shortcut for removeing a value without needing an 'out' parameter.
		/// </summary>
		public static bool TryRemove<TKey, T>(this ConcurrentDictionary<TKey, T> target, TKey key)
		{
			if (target == null) throw new NullReferenceException();

            return target.TryRemove(key, out T value);
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


		/// <summary>
		/// Attempts to get a value from a dictionary and if no value is present, it returns the default.
		/// </summary>
		public static T GetOrDefault<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			return target.GetOrDefault(key, default(T));
		}

		/// <summary>
		/// Attempts to get a value from a dictionary and if no value is present, it returns the provided defaultValue.
		/// </summary>
		public static T GetOrDefault<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			T defaultValue)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

            return target.TryGetValue(key, out T value) ? value : defaultValue;
        }


		/// <summary>
		/// Attempts to get a value from a dictionary and if no value is present, it returns the response of the valueFactory.
		/// </summary>
		public static T GetOrDefault<TKey, T>(
			this IDictionary<TKey, T> target,
			TKey key,
			Func<TKey, T> valueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            return target.TryGetValue(key, out T value) ? value : valueFactory(key);
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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            if (!target.TryGetValue(key, out T value))
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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

            if (!target.TryGetValue(key, out T v))
                target.Add(key, v = value);
            return v;
		}

        internal static void ValidateMillisecondsTimeout(int? millisecondsTimeout)
        {
            if ((millisecondsTimeout ?? 0) < 0)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "Cannot be a negative value.");
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

		public static void AddSynchronized<T>(this ICollection<T> target, T value)
		{
			if (target == null) throw new NullReferenceException();
			ThreadSafety.SynchronizeWrite(target, () => target.Add(value));
		}

		/// <summary>
		/// Thread safe shortcut for adding a value to list within a dictionary.
		/// </summary>
		public static void AddTo<TKey, TValue>(this IDictionary<TKey, IList<TValue>> c, TKey key, TValue value)
		{
			if (c == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var list = c.GetOrAdd(key, k => new List<TValue>());
			list.Add(value);
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
		/// Shortcut for ensuring a cacheKey contains a action.  If no action exists, it adds the provided defaultValue.
		/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
		/// </summary>
		public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key, T defaultValue)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			if (!target.ContainsKey(key))
				target.Add(key, defaultValue);
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
		/// Shortcut for ensuring a cacheKey contains a Value.  If no action exists, it adds it using the provided defaultValueFactory.
		/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
		/// </summary>
		public static void EnsureDefault<TKey, T>(this IDictionary<TKey, T> target, TKey key,
			Func<TKey, T> defaultValueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (defaultValueFactory == null) throw new ArgumentNullException("defaultValueFactory");

			if (!target.ContainsKey(key))
				target.Add(key, defaultValueFactory(key));
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
		/// Shortcut for adding a value or updating based on exising value.
		/// If no value exists, it adds the provided value.
		/// If a value exists, it sets the value using the updateValueFactory.
		/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
		/// </summary>
		public static T AddOrUpdate<TKey, T>(this IDictionary<TKey, T> target, TKey key,
			T value,
			T updateValue)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

            T valueUsed;
            if (target.TryGetValue(key, out T old))
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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

            T valueUsed;
            if (target.TryGetValue(key, out T old))
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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (newValueFactory == null) throw new ArgumentNullException("newValueFactory");
			if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

            T valueUsed;
            if (target.TryGetValue(key, out T old))
				target[key] = valueUsed = updateValueFactory(key, old);
			else
				target.Add(key, valueUsed = newValueFactory(key));

			return valueUsed;
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
		private enum SortDirection
		{
			Ascending = 0,
			Descending = 1
		}
		#endregion


		public static TValue GetOrAdd<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> source,
			out bool updated,
			TKey key,
			Func<TKey, TValue> valueFactory)
		{
			if (source == null)
				throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var u = false;

			TValue value = source.GetOrAdd(key, (k) =>
			{
				u = true;
				return valueFactory(k);
			});

			updated = u;
			return value;
		}

		public static TValue GetOrAdd<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> source,
			out bool updated,
			TKey key,
			TValue value)
		{
			if (source == null)
				throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			var u = false;

			TValue result = source.GetOrAdd(key, (k) =>
			{
				u = true;
				return value;
			});

			updated = u;
			return result;
		}

		public static bool UpdateRequired<TKey>(this ConcurrentDictionary<TKey, DateTime> source, TKey key, TimeSpan timeBeforeExpires)
		{
			if (source == null)
				throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

            // Use temporary update value to allow code contract resolution.
            DateTime now = DateTime.Now;
            DateTime lastupdated = source.GetOrAdd(out bool updating, key, now);

			var threshhold = now.Add(-timeBeforeExpires);
			if (!updating && lastupdated < threshhold)
			{
				lastupdated = source.AddOrUpdate(key, now, (k, old) =>
				{
					if (old < threshhold)
					{
						updating = true;
						return now;
					}
					return old;
				});
			}

			return updating;
		}

		public static DateTime? NullableFirstOrDefault(this IEnumerable<DateTime> source)
		{
			if (source == null)
				return null;

			var result = source.Take(1).ToList();
			return result.Any() ? result.First() : new DateTime?();
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

		public static IEnumerable<T> OfType<TSource, T>(this IEnumerable<TSource> list)
		{
			return list.Where(e => e is T).Cast<T>();
		}

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

namespace Open.Collections.NonGeneric
{
	public static class Extensions
	{
		const int SYNC_TIMEOUT_DEFAULT_MILLISECONDS = 10000;

        internal static void ValidateMillisecondsTimeout(int? millisecondsTimeout)
        {
            if ((millisecondsTimeout ?? 0) < 0)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "Cannot be a negative value.");
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
		/// Tries to acquire a value from the dictionary.  If no value is present it adds it using the valueFactory response.
		/// NOT THREAD SAFE: Use only when a dictionary local or is assured single threaded.
		/// </summary>
		public static T GetOrAdd<T>(
			this IDictionary target,
			object key,
			Func<object, T> valueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");

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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

			if (target.Contains(key))
			{
				var result = target[key];
				value = result == null ? default(T) : (T)result;
				return true;
			}

			value = default(T);
			return false;
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
		/// Attempts to get a value from a dictionary and if no value is present, it returns the response of the valueFactory.
		/// </summary>
		public static T GetOrDefault<T>(
			this IDictionary target,
			object key,
			Func<object, T> valueFactory)
		{
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");

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
			if (target == null) throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException("key");

            return target.TryGetValue(key, out T value) ? value : defaultValue;
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
