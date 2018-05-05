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
			if (source == null)
				throw new NullReferenceException();
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			byte[] bytes = new byte[4096];

			int cnt;
			while ((cnt = source.Read(bytes, 0, bytes.Length)) != 0)
				target.Write(bytes, 0, cnt);
		}
	}
}
