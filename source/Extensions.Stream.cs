using System;
using System.IO;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Copies the source stream to the target.
		/// </summary>
		public static void CopyTo(this Stream source, Stream target)
		{
			if (source is null)
				throw new NullReferenceException();
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(4096);
			int cnt;
			while ((cnt = source.Read(bytes, 0, bytes.Length)) != 0)
				target.Write(bytes, 0, cnt);
			System.Buffers.ArrayPool<byte>.Shared.Return(bytes, true);
		}
	}
}
