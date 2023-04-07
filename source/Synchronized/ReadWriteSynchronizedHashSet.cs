using Open.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Open.Collections.Synchronized;

public sealed class ReadWriteSynchronizedHashSet<T>
	: ReadWriteSynchronizedCollectionWrapper<T, HashSet<T>>, ISet<T>
{
	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet() : base(new HashSet<T>()) { }

	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }

	[ExcludeFromCodeCoverage]
	public ReadWriteSynchronizedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

	// Asumes that .Contains is a thread-safe read-only operation.
	// But any potentially iterative operation will be locked.

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Contains(T item)
		=> InternalSource.Contains(item);

	/// <inheritdoc />
	public new bool Add(T item)
		=> IfNotContains(item, c => c.Add(item));

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void ExceptWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.ExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void IntersectWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.IntersectWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsSubsetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.IsSupersetOf(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool Overlaps(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.Overlaps(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public bool SetEquals(IEnumerable<T> other)
	{
		using var read = RWLock.ReadLock();
		return InternalSource.SetEquals(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void UnionWith(IEnumerable<T> other)
	{
		using var write = RWLock.WriteLock();
		InternalSource.UnionWith(other);
	}

	/// <inheritdoc />
	public override bool IfContains(T item, Action<HashSet<T>> action)
		=> InternalSource.Contains(item) && base.IfContains(item, action);

	/// <inheritdoc />
	public override bool IfNotContains(T item, Action<HashSet<T>> action)
		=> !InternalSource.Contains(item) && base.IfNotContains(item, action);
}
