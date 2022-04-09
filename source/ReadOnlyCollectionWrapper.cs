using Open.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections;

public class ReadOnlyCollectionWrapper<T, TCollection> : DisposableBase, IReadOnlyCollection<T>
		where TCollection : class, ICollection<T>
{
	protected TCollection InternalSource;
    protected readonly bool SourceOwned;

    /// <summary>
    /// Constructs a wrapper for read-only access to a collection.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="owner">
    /// If <see langword="true"/>, will call <paramref name="source"/>.Dispose() if the source is <see cref="IDisposable"/> when this is disposed.<br/>
    /// And will throw <see cref="NotSupportedException"/> to prevent direct access to the source when .ExtractAndDispose() is called.
    /// </param>
    /// <exception cref="ArgumentNullException">If the <paramref name="source"/> is <see langword="null"/>.</exception>
    [ExcludeFromCodeCoverage]
    public ReadOnlyCollectionWrapper(TCollection source, bool owner = false)
    {
        InternalSource = source ?? throw new ArgumentNullException(nameof(source));
        SourceOwned = owner;
    }

    #region Implementation of IReadOnlyCollection<T>
    /// <inheritdoc cref="ICollection&lt;T&gt;.Contains(T)" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Contains(T item)
        => InternalSource.Contains(item);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public virtual int Count
		=> InternalSource.Count;

    /// <inheritdoc cref="ICollection&lt;T&gt;.IsReadOnly" />
    [ExcludeFromCodeCoverage]
    public virtual bool IsReadOnly
		=> true;

    /// <summary>
    /// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
    /// </summary>
    /// <returns>An enumerator from the underlying collection.</returns>
    [ExcludeFromCodeCoverage]
    public virtual IEnumerator<T> GetEnumerator() => InternalSource.GetEnumerator();

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="ICollection&lt;T&gt;.CopyTo(T[], int)" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void CopyTo(T[] array, int arrayIndex)
        => InternalSource.CopyTo(array, arrayIndex);
    #endregion

    /// <inheritdoc cref="Extensions.CopyToSpan{T}(IEnumerable{T}, Span{T})"/>
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual Span<T> CopyTo(Span<T> span)
        => InternalSource.CopyToSpan(span);

    /// <inheritdoc cref="ISynchronizedCollection&lt;T&gt;.Export(ICollection{T})" />
    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Export(ICollection<T> to)
        => to.AddRange(InternalSource);

    #region Dispose
    [ExcludeFromCodeCoverage]
    protected override void OnDispose()
    {
        var source = Nullify(ref InternalSource);
        if (SourceOwned && source is IDisposable d) d.Dispose();
    }

    /// <summary>
    /// Extracts the underlying collection and returns it before disposing of this synchronized wrapper.
    /// </summary>
    /// <returns>The extracted underlying collection.</returns>
    /// <exception cref="NotSupportedException">If the underlying collection is owned by this wrapper.</exception>
    public TCollection ExtractAndDispose()
	{
        if (SourceOwned) throw new NotSupportedException("The underlying collection is owned by this wrapper.");
		using (this)
		{
			return Nullify(ref InternalSource);
		}
	}
	#endregion
}
