using System;
using System.Collections.Concurrent;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Shortcut for removeing a value without needing an 'out' parameter.
		/// </summary>
		public static bool TryRemove<TKey, T>(this ConcurrentDictionary<TKey, T> target, TKey key)
		{
			if (target == null) throw new NullReferenceException();

			return target.TryRemove(key, out T value);
		}


		public static TValue GetOrAdd<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> source,
			out bool updated,
			TKey key,
			Func<TKey, TValue> valueFactory)
		{
			if (source == null)
				throw new NullReferenceException();
			if (key == null) throw new ArgumentNullException(nameof(key));

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
			if (key == null) throw new ArgumentNullException(nameof(key));

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
			if (key == null) throw new ArgumentNullException(nameof(key));

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

	}
}
