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
		public static async ValueTask DualBufferCopyToAsync(
			this Stream source,
			Stream target,
			int bufferSize = 4096,
			bool clearBufferAfter = false,
			CancellationToken? cancellationToken = null)
		{
			if (source is null)
				throw new NullReferenceException();
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var pool = ArrayPool<byte>.Shared;
			var cNext = pool.Rent(bufferSize);
			var cCurrent = pool.Rent(bufferSize);

			var token = cancellationToken ?? CancellationToken.None;
			try
			{
				var next = source.ReadAsync(cNext, 0, bufferSize, token);
				while (true)
				{
					var n = await next.ConfigureAwait(false);
					if (n == 0) break;

					// Preemptive request before yielding.
					var current = source.ReadAsync(cCurrent, 0, bufferSize, token);
					await target.WriteAsync(cNext, 0, n, token);

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
