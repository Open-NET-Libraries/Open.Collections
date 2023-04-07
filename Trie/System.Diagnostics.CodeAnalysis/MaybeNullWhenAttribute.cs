#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis;

// Use a shim for simplicity.

/// <summary>
/// Indicates that the output may be null even if the corresponding type disallows it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class MaybeNullWhenAttribute : Attribute
{
    /// <summary>
    /// Constructs a <see cref="MaybeNullWhenAttribute"/>.
    /// </summary>
    public MaybeNullWhenAttribute(bool returnValue)
        => ReturnValue = returnValue;

    /// <summary>
    /// The return value condition.
    /// </summary>
    public bool ReturnValue { get; }
}
#endif