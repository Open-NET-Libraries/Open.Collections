using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Open.Collections;

public static partial class Extensions
{
    /// <summary>
    /// Shortcut for removeing a value without needing an 'out' parameter.
    /// </summary>
    public static bool TryRemove<TKey, T>(this ConcurrentDictionary<TKey, T> target, TKey key)
    {
        if (target is null) throw new ArgumentNullException(nameof(target));
        Contract.EndContractBlock();

        return target.TryRemove(key, out _);
    }

    /// <remarks>
    /// <paramref name="updated"/> is true if this thread executed the value factory.
    /// But because of the optimistic nature of <see cref="ConcurrentDictionary{TKey, TValue}"/> it does not mean that the value produce is the one used.
    /// </remarks>
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>
    public static TValue GetOrAdd<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> source,
        out bool updated,
        TKey key,
        Func<TKey, TValue> valueFactory)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));
        Contract.EndContractBlock();

        bool u = false;

        TValue? value = source.GetOrAdd(key, (k) =>
        {
            u = true;
            return valueFactory(k);
        });

        updated = u;
        return value;
    }

    /// <remarks>
    /// <paramref name="updated"/> is true if the entry required an update.
    /// But because of the optimistic nature of <see cref="ConcurrentDictionary{TKey, TValue}"/> it does not mean that the value is the one used.
    /// </remarks>
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, TValue)"/>
    public static TValue GetOrAdd<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> source,
        out bool updated,
        TKey key,
        TValue value)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        Contract.EndContractBlock();

        bool u = false;

        TValue? result = source.GetOrAdd(key, (_) =>
        {
            u = true;
            return value;
        });

        updated = u;
        return result;
    }

    /// <summary>
    /// Will return true if the existing <see cref="DateTime"/> value is past due.
    /// </summary>
    public static bool UpdateRequired<TKey>(this ConcurrentDictionary<TKey, DateTime> source, TKey key, TimeSpan timeBeforeExpires)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        Contract.EndContractBlock();

        // Use temporary update value to allow code contract resolution.
        DateTime now = DateTime.Now;
        DateTime lastupdated = source.GetOrAdd(out bool updating, key, now);

        DateTime threshhold = now.Add(-timeBeforeExpires);
        if (!updating && lastupdated < threshhold)
        {
            source.AddOrUpdate(key, now, (_, old) =>
            {
                if (old >= threshhold) return old;
                updating = true;
                return now;
            });
        }

        return updating;
    }

    /// <remarks>Handles evicting an entry if the result of the <see cref="Lazy{T}"/> was erroneous.</remarks>
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>
    public static Lazy<TValue> GetOrAddSafely<TKey, TValue>(
        this ConcurrentDictionary<TKey, Lazy<TValue>> source,
        TKey key,
        Func<TKey, TValue> valueFactory)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));
        Contract.EndContractBlock();

        return source.GetOrAdd(key,
        k => new Lazy<TValue>(() =>
        {
            try
            {
                return valueFactory(k);
            }
            catch
            {
                // Assumes that this is the current entry and no other would be possible until it completes.
                source.TryRemove(k, out _);
                throw;
            }
        }));
    }

    /// <remarks>Handles evicting an entry if the result of the <see cref="Lazy{T}"/> was erroneous or its <see cref="Task{T}"/> did not complete successfully.</remarks>
    /// <inheritdoc cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>
    public static Lazy<Task<TValue>> GetOrAddSafely<TKey, TValue>(
        this ConcurrentDictionary<TKey, Lazy<Task<TValue>>> source,
        TKey key,
        Func<TKey, Task<TValue>> valueFactory)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));
        Contract.EndContractBlock();

        return source.GetOrAddSafely(key,
        k => valueFactory(k).ContinueWith(t =>
        {
            if (t.IsFaulted || t.IsCanceled)
                source.TryRemove(k, out _);
            return t;
        }, TaskContinuationOptions.ExecuteSynchronously).Unwrap());
    }
}
