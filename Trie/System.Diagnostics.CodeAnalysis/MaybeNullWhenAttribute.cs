#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis;

// Use a shim for simplicity.

/// <summary>
/// Indicates that the output may be null even if the corresponding type disallows it.
/// </summary>
/// <remarks>
/// Constructs a <see cref="MaybeNullWhenAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
{
	/// <summary>
	/// The return value condition.
	/// </summary>
	public bool ReturnValue { get; } = returnValue;
}
#endif