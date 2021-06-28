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
			if (target is null) throw new ArgumentNullException(nameof(target));

			return target.TryRemove(key, out _);
		}


		public static TValue GetOrAdd<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> source,
			out bool updated,
			TKey key,
			Func<TKey, TValue> valueFactory)
		{
			if (source is null)
				throw new NullReferenceException();
			if (key is null) throw new ArgumentNullException(nameof(key));

			var u = false;

			var value = source.GetOrAdd(key, (k) =>
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
			if (source is null)
				throw new NullReferenceException();
			if (key is null) throw new ArgumentNullException(nameof(key));

			var u = false;

			var result = source.GetOrAdd(key, (k) =>
			{
				u = true;
				return value;
			});

			updated = u;
			return result;
		}

		public static bool UpdateRequired<TKey>(this ConcurrentDictionary<TKey, DateTime> source, TKey key, TimeSpan timeBeforeExpires)
		{
			if (source is null)
				throw new NullReferenceException();
			if (key is null) throw new ArgumentNullException(nameof(key));

			// Use temporary update value to allow code contract resolution.
			var now = DateTime.Now;
			var lastupdated = source.GetOrAdd(out var updating, key, now);

			var threshhold = now.Add(-timeBeforeExpires);
			if (!updating && lastupdated < threshhold)
			{
				source.AddOrUpdate(key, now, (k, old) =>
				{
					if (old >= threshhold) return old;
					updating = true;
					return now;
				});
			}

			return updating;
		}

	}
}
