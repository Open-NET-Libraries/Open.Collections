using Open.Disposable;
using System.Collections;

namespace Open.Collections;

/// <summary>
/// A disposable aware read-only wrapper for a collection.
/// </summary>
public class ReadOnlyCollectionWrapper<T, TCollection>
	: DisposableBase, IReadOnlyCollection<T>
	where TCollection : class, ICollection<T>
{
	/// <summary>
	/// The underlying collection.
	/// </summary>
	protected TCollection? InternalUnsafeSource;

	/// <summary>
	/// If <see langword="true"/>, will call <see cref="IDisposable.Dispose"/> if the source is <see cref="IDisposable"/> when this is disposed.
	/// </summary>
	protected readonly bool SourceOwned;

	/// <summary>
	/// The underlying collection.
	/// </summary>
	/// <exception cref="ObjectDisposedException">If this has been disposed.</exception>
	protected TCollection InternalSource
		=> InternalUnsafeSource ?? throw new ObjectDisposedException(GetType().ToString());

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
	[SuppressMessage("Style", "IDE0290:Use primary constructor")]
	public ReadOnlyCollectionWrapper(TCollection source, bool owner = false)
	{
		InternalUnsafeSource = source ?? throw new ArgumentNullException(nameof(source));
		SourceOwned = owner;
	}

	private void ThrowIfDisposedInternal() => base.AssertIsAlive();

	private Action? _throwIfDisposed;

	/// <summary>
	/// A delegate to throw an exception if this has been disposed.
	/// </summary>
	protected Action ThrowIfDisposedDelegate
		=> _throwIfDisposed ??= ThrowIfDisposedInternal;

	/// <summary>
	/// Produces an enumerable that will throw an exception if this has been disposed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected IEnumerable<T2> ThrowIfDisposed<T2>(IEnumerable<T2> source)
		=> source.Preflight(ThrowIfDisposedDelegate).BeforeGetEnumerator(ThrowIfDisposedDelegate);

	/// <summary>
	/// A utility for ensuring the source is not disposed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected T2 ThrowIfDisposed<T2>(T2 source)
	{
		base.AssertIsAlive();
		return source;
	}

	#region Implementation of IReadOnlyCollection<T>
	/// <inheritdoc cref="ICollection{T}.Contains(T)" />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool Contains(T item)
		=> InternalSource.Contains(item);

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public virtual int Count
		=> InternalSource.Count;

	/// <inheritdoc cref="ICollection{T}.IsReadOnly" />
	[ExcludeFromCodeCoverage]
	public virtual bool IsReadOnly => true;

	/// <summary>
	/// To ensure expected behavior, this returns an enumerator from the underlying collection.  Exceptions can be thrown if the collection content changes.
	/// </summary>
	/// <returns>An enumerator from the underlying collection.</returns>
	[ExcludeFromCodeCoverage]
	public virtual IEnumerator<T> GetEnumerator()
		=> InternalSource.Preflight(ThrowIfDisposedDelegate).GetEnumerator();

	[ExcludeFromCodeCoverage]
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <inheritdoc cref="ICollection{T}.CopyTo(T[], int)" />
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

	/// <inheritdoc cref="ISynchronizedCollection{T}.Export(ICollection{T})" />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void Export(ICollection<T> to)
		=> to.AddRange(InternalSource);

	#region Dispose
	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	protected override void OnDispose()
	{
		var source = Nullify(ref InternalUnsafeSource!);
		if (SourceOwned && source is IDisposable d) d.Dispose();
	}

	/// <summary>
	/// Extracts the underlying collection and returns it before disposing of this synchronized wrapper.
	/// </summary>
	/// <returns>The extracted underlying collection.</returns>
	/// <exception cref="NotSupportedException">If the underlying collection is owned by this wrapper.</exception>
	public TCollection ExtractAndDispose()
	{
		AssertIsAlive();
		if (SourceOwned) throw new NotSupportedException("The underlying collection is owned by this wrapper.");
		using (this)
		{
			return Nullify(ref InternalUnsafeSource!);
		}
	}
	#endregion
}
