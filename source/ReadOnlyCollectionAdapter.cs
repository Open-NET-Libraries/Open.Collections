using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public sealed class ReadOnlyCollectionAdapter<T>
    : IReadOnlyCollection<T>, ICollection<T>
{
    readonly IEnumerable<T> _source;
    readonly Func<int> _getCount;
    readonly Func<T, bool> _contains;

    [ExcludeFromCodeCoverage]
    public ReadOnlyCollectionAdapter(IEnumerable<T> source, Func<int> getCount)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _getCount = getCount ?? throw new ArgumentNullException(nameof(getCount));
        _contains = source is ICollection<T> c
            ? item => c.Contains(item)
            : item => source.Contains(item);
    }

    [ExcludeFromCodeCoverage]
    public ReadOnlyCollectionAdapter(IReadOnlyCollection<T> source)
        : this(source, () => source.Count) { }

    [ExcludeFromCodeCoverage]
    public ReadOnlyCollectionAdapter(ICollection<T> source)
        : this(source, () => source.Count) { }

    #region Implementation of IReadOnlyCollection<T>

    /// <inheritdoc cref="ICollection{T}.Contains(T)" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => _contains(item);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public int Count => _getCount();

    /// <summary>
    /// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
    /// </summary>
    /// <returns>An enumerator from the underlying collection.</returns>
    [ExcludeFromCodeCoverage]
    public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
        => CopyTo(array.AsSpan().Slice(arrayIndex));
    #endregion

    /// <inheritdoc cref="Extensions.CopyToSpan{T}(IEnumerable{T}, Span{T})"/>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> CopyTo(Span<T> span)
        => _source.CopyToSpan(span);

    /// <inheritdoc cref="ISynchronizedCollection{T}.Export(ICollection{T})" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Export(ICollection<T> to)
        => to.AddRange(_source);

    /// <inheritdoc cref="ICollection{T}.Add(T)" />
    [ExcludeFromCodeCoverage]
    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    /// <inheritdoc cref="ICollection{T}.Clear()" />
    [ExcludeFromCodeCoverage]
    void ICollection<T>.Clear() => throw new NotSupportedException();

    /// <inheritdoc cref="ICollection{T}.Remove(T)" />
    [ExcludeFromCodeCoverage]
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    /// <inheritdoc cref="ICollection{T}.IsReadOnly" />
    [ExcludeFromCodeCoverage]
    bool ICollection<T>.IsReadOnly => true;
}
