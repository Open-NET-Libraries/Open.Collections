using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Open.Collections.Trie;

/// <summary>
/// Allows for consistent retrieval of the same string when joining strings together.
/// </summary>
/// <remarks>
/// Useful for (re)generating cache keys.
/// </remarks>
public class StringJoinPool
{
    private readonly ReadOnlyMemory<char> _separator;
    private readonly ITrie<string, string> _pool;
    private StringBuilder? _reusableBuilder;

    /// <summary>
    /// Constructs a <see cref="StringJoinPool"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">If the supplied pool is null.</exception>
    public StringJoinPool(ITrie<string, string> pool, ReadOnlyMemory<char> separator)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _separator = separator;
    }

    /// <inheritdoc cref="StringJoinPool(ITrie{string, string}, ReadOnlyMemory{char})"/>
    public StringJoinPool(ITrie<string, string> pool, string? separator = null)
        : this(pool, separator is null ? ReadOnlyMemory<char>.Empty : separator.AsMemory())
    {

    }

    /// <inheritdoc cref="StringJoinPool(ITrie{string, string}, ReadOnlyMemory{char})"/>
    /// <remarks>Uses <see cref="ConcurrentTrie{TKey, TValue}"/> as the default underlying pool.</remarks>
    public StringJoinPool(string? separator = null)
        :this(new ConcurrentTrie<string, string>(), separator)
    {
    }

    /// <inheritdoc cref="StringJoinPool(string?)"/>
    public StringJoinPool(ReadOnlyMemory<char> separator)
        : this(new ConcurrentTrie<string, string>(), separator)
    {
    }

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
                if (_separator.IsEmpty)
                {
                    for (int i = 0; i < len; i++)
                        sb.Append(segments[i] ?? string.Empty);
                    return sb.ToString();
                }

                Debug.Assert(segments.Length != 0);

                sb.Append(segments[0]);
                var sepSpan = _separator.Span;
                int sLen = sepSpan.Length;

                for (int i = 1; i < len; i++)
                {
                    for(int s = 0; s < sLen; s++)
                        sb.Append(sepSpan[s]);

                    sb.Append(segments[i] ?? string.Empty);
                }

                return sb.ToString();
            }
            finally
            {
                sb.Clear();
                if(sb.Capacity > 256) sb.Capacity = 256;
                Interlocked.CompareExchange(ref _reusableBuilder, sb, null);
            }
        }
    }
}
