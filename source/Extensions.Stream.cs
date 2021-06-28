using System;
using System.Buffers;
using System.IO;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Copies the source stream to the target.
		/// </summary>
		public static void CopyTo(this Stream source, Stream target, int bufferSize = 4096, bool clearBufferAfter = false)
		{
			if (source is null)
				throw new NullReferenceException();
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var pool = ArrayPool<byte>.Shared;
			var bytes = pool.Rent(bufferSize);

			try
			{
				int cnt;
				while ((cnt = source.Read(bytes, 0, bufferSize)) != 0)
					target.Write(bytes, 0, cnt);
			}
			finally
			{
				pool.Return(bytes, clearBufferAfter);
			}
		}
	}
}
