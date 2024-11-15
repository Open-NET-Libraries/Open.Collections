using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Collections;

public static partial class Extensions
{
	/// <summary>
	/// Copies the source stream to the target.
	/// </summary>
	public static async ValueTask DualBufferCopyToAsync(
		this Stream source,
		Stream target,
		int bufferSize = 4096,
		bool clearBufferAfter = false,
		CancellationToken cancellationToken = default)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));
		if (target is null) throw new ArgumentNullException(nameof(target));

		ArrayPool<byte>? pool = ArrayPool<byte>.Shared;
		byte[]? cNext = pool.Rent(bufferSize);
		byte[]? cCurrent = pool.Rent(bufferSize);

		try
		{
			Task<int>? next = source.ReadAsync(cNext, 0, bufferSize, cancellationToken);
			while (true)
			{
				int n = await next.ConfigureAwait(false);
				if (n == 0) break;

				// Preemptive request before yielding.
				Task<int> current = source.ReadAsync(cCurrent, 0, bufferSize, cancellationToken);
#if NETSTANDARD2_0
				await target.WriteAsync(cNext, 0, n, cancellationToken).ConfigureAwait(false);
#else
				await target.WriteAsync(cNext.AsMemory(0, n), cancellationToken).ConfigureAwait(false);
#endif
				if (current is null) throw new OperationCanceledException();
				(cCurrent, cNext) = (cNext, cCurrent);
				next = current;
			}
		}
		finally
		{
			pool.Return(cNext, clearBufferAfter);
			pool.Return(cCurrent, clearBufferAfter);
		}
	}
}
