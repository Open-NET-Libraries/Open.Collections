using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Open.Collections
{
	public static partial class Extensions
	{


		public static IEnumerable<T> TryTakeWhile<T>(this ConcurrentBag<T> target, Func<ConcurrentBag<T>, bool> predicate)
		{
			if (target is null) throw new ArgumentNullException(nameof(target));

			while (!target.IsEmpty && predicate(target) && target.TryTake(out var value))
			{
				yield return value;
			}
		}

		public static IEnumerable<T> TryTakeWhile<T>(this ConcurrentBag<T> target, Func<bool> predicate)
		{
			if (target is null) throw new ArgumentNullException(nameof(target));

			return TryTakeWhile(target, t => predicate());
		}

		public static void Trim<T>(this ConcurrentBag<T> target, int maxSize)
		{
			if (target is null) throw new ArgumentNullException(nameof(target));

			foreach (var _ in TryTakeWhile(target, t => t.Count > maxSize))
			{
			}
		}

		public static Task TrimAsync<T>(this ConcurrentBag<T> target, int maxSize, Action<T> handler)
		{
			if (target is null) throw new ArgumentNullException(nameof(target));

			return Task.WhenAll(
				TryTakeWhile(target, t => t.Count > maxSize)
					.Select(t => Task.Run(() => handler(t)))
					.ToArray() // Buffer trimmed so that when this method is returned the target is already trimmed but awaiting handlers to be complete.
			);
		}

		public static Task ClearAsync<T>(this ConcurrentBag<T> target, Action<T> handler)
		{
			return TrimAsync(target, 0, handler);
		}
	}
}
