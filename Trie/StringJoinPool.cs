using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Open.Collections;

/// <summary>
/// Allows for consistent retrieval of the same string when joining strings together.
/// </summary>
/// <remarks>
/// Useful for (re)generating cache keys.
/// </remarks>
public class StringJoinPool(
	ITrie<string, string> pool, ReadOnlyMemory<char> separator)
{
	private readonly ITrie<string, string> _pool = pool ?? throw new ArgumentNullException(nameof(pool));
	private StringBuilder? _reusableBuilder;

	/// <inheritdoc cref="StringJoinPool(ITrie{string, string}, ReadOnlyMemory{char})"/>
	public StringJoinPool(ITrie<string, string> pool, string? separator = null)
		: this(pool, separator is null ? ReadOnlyMemory<char>.Empty : separator.AsMemory())
	{ }

	/// <inheritdoc cref="StringJoinPool(ITrie{string, string}, ReadOnlyMemory{char})"/>
	/// <remarks>Uses <see cref="ConcurrentTrie{TKey, TValue}"/> as the default underlying pool.</remarks>
	public StringJoinPool(string? separator = null)
		: this(new ConcurrentTrie<string, string>(), separator)
	{ }

	/// <inheritdoc cref="StringJoinPool(string?)"/>
	public StringJoinPool(ReadOnlyMemory<char> separator)
		: this(new ConcurrentTrie<string, string>(), separator)
	{ }

	/// <summary>
	/// Allows for applying a custom transform in sub-classed to segments.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void AppendSegment(StringBuilder sb, string segment)
	{
		if (string.IsNullOrEmpty(segment)) return;
		sb.Append(segment);
	}

	/// <summary>
	/// Gets the string from the pool at the specified path.
	/// </summary>
	public string Get(ReadOnlySpan<string> segments)
	{
		if (segments.IsEmpty)
			return string.Empty;

		var node = _pool.EnsureNodes(segments);
		if (node.TryGetValue(out string? v))
			return v;

		Debug.Assert(segments.Length != 0);
		return node.GetOrAdd(Build(segments));

		string Build(ReadOnlySpan<string> segments)
		{
			var sb = Interlocked.Exchange(ref _reusableBuilder, null) ?? new StringBuilder();
			int len = segments.Length;
			try
			{
				if (separator.IsEmpty)
				{
					for (int i = 0; i < len; i++)
						AppendSegment(sb, segments[i]);
					return sb.ToString();
				}

				Debug.Assert(segments.Length != 0);

				AppendSegment(sb, segments[0]);
				var sepSpan = separator.Span;
				int sLen = sepSpan.Length;

				for (int i = 1; i < len; i++)
				{
					for (int s = 0; s < sLen; s++)
						sb.Append(sepSpan[s]);

					AppendSegment(sb, segments[i]);
				}

				return sb.ToString();
			}
			finally
			{
				sb.Clear();
				if (sb.Capacity > 256) sb.Capacity = 256;
				Interlocked.CompareExchange(ref _reusableBuilder, sb, null);
			}
		}
	}
}
