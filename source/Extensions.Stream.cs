using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Copies the source stream to the target.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Reliability",
			"CA2016:Forward the 'CancellationToken' parameter to methods",
			Justification = "Is required to ensure recieved data is not lost.")]
		public static async ValueTask DualBufferCopyToAsync(
			this Stream source,
			Stream target,
			int bufferSize = 4096,
			bool clearBufferAfter = false,
			CancellationToken cancellationToken = default)
		{
			if (source is null) throw new ArgumentNullException(nameof(source));
			if (target is null) throw new ArgumentNullException(nameof(target));

			var pool = ArrayPool<byte>.Shared;
			var cNext = pool.Rent(bufferSize);
			var cCurrent = pool.Rent(bufferSize);

			try
			{
				var next = source.ReadAsync(cNext, 0, bufferSize, cancellationToken);
				while (true)
				{
					var n = await next.ConfigureAwait(false);
					if (n == 0) break;

					// Preemptive request before yielding.
					var current = cancellationToken.IsCancellationRequested ? null : source.ReadAsync(cCurrent, 0, bufferSize);
#if NETSTANDARD2_1_OR_GREATER
					await target.WriteAsync(cNext.AsMemory(0, n)).ConfigureAwait(false);
#else
					await target.WriteAsync(cNext, 0, n).ConfigureAwait(false);
#endif
					if (current is null) throw new OperationCanceledException();
					var swap = cNext;
					cNext = cCurrent;
					cCurrent = swap;
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
}
