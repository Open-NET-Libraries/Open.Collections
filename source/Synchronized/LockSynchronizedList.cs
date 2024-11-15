using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized list.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class LockSynchronizedList<T>
	: LockSynchronizedListWrapper<T>
{
	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	public LockSynchronizedList() : base([]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity.
	/// </summary>
	public LockSynchronizedList(int capacity = 0) : base(new List<T>(capacity)) { }

	/// <summary>
	/// Constructs a new instance with the specified collection.
	/// </summary>
	public LockSynchronizedList(IEnumerable<T> collection) : base(new List<T>(collection)) { }
}
