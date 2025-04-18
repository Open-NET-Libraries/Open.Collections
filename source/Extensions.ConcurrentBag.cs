using System.Collections.Concurrent;

namespace Open.Collections;

public static partial class Extensions
{
	/// <summary>
	/// Attempts to take items from the <see cref="ConcurrentBag{T}"/> while the <paramref name="predicate"/> is true.
	/// </summary>
	public static IEnumerable<T> TryTakeWhile<T>(this ConcurrentBag<T> target, Func<ConcurrentBag<T>, bool> predicate)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		return TryTakeWhileCore(target, predicate);

		static IEnumerable<T> TryTakeWhileCore(ConcurrentBag<T> target, Func<ConcurrentBag<T>, bool> predicate)
		{
			while (!target.IsEmpty && predicate(target) && target.TryTake(out T? value))
			{
				yield return value;
			}
		}
	}

	/// <inheritdoc cref="TryTakeWhile{T}(ConcurrentBag{T}, Func{bool})"/>
	public static IEnumerable<T> TryTakeWhile<T>(this ConcurrentBag<T> target, Func<bool> predicate)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		return TryTakeWhile(target, _ => predicate());
	}

	/// <summary>
	/// Trims the <see cref="ConcurrentBag{T}"/> to the specified <paramref name="maxSize"/>.
	/// </summary>
	public static void Trim<T>(this ConcurrentBag<T> target, int maxSize)
	{
		foreach (T? _ in TryTakeWhile(target, t => t.Count > maxSize))
		{
		}
	}

	/// <summary>
	/// Trims the <see cref="ConcurrentBag{T}"/> to the specified <paramref name="maxSize"/> and calls the <paramref name="handler"/> for each trimmed item.
	/// </summary>
	public static Task TrimAsync<T>(this ConcurrentBag<T> target, int maxSize, Action<T> handler)
	{
		if (target is null) throw new ArgumentNullException(nameof(target));
		Contract.EndContractBlock();

		return Task.WhenAll(
			TryTakeWhile(target, t => t.Count > maxSize)
				.Select(t => Task.Run(() => handler(t)))
				.ToArray() // Buffer trimmed so that when this method is returned the target is already trimmed but awaiting handlers to be complete.
		);
	}

	/// <summary>
	/// Clears the <see cref="ConcurrentBag{T}"/> and calls the <paramref name="handler"/> for each item.
	/// </summary>
	public static Task ClearAsync<T>(this ConcurrentBag<T> target, Action<T> handler)
		=> TrimAsync(target, 0, handler);
}
