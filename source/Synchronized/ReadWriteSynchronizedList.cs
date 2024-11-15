using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Open.Collections.Synchronized;

/// <summary>
/// A synchronized <see cref="List{T}"/> that uses a <see cref="System.Threading.ReaderWriterLockSlim"/> for thread safety.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ReadWriteSynchronizedList<T>
	: ReadWriteSynchronizedListWrapper<T>
{
	/// <summary>
	/// Constructs a new instance.
	/// </summary>
	public ReadWriteSynchronizedList()
		: base([]) { }

	/// <summary>
	/// Constructs a new instance with the specified capacity.
	/// </summary>
	public ReadWriteSynchronizedList(int capacity = 0)
		: base(new List<T>(capacity)) { }

	/// <summary>
	/// Constructs a new instance with the specified collection.
	/// </summary>
	public ReadWriteSynchronizedList(IEnumerable<T> collection)
		: base(new List<T>(collection)) { }
}
