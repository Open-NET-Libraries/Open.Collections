﻿using System;
using System.Text;

namespace Open.Collections
{
	public static partial class Extensions
	{
		/// <summary>
		/// Converts a string to a byte array.
		/// </summary>
		/// <param name="value">The string value.</param>
		/// <param name="encoding">Default is UTF8.</param>
		public static byte[] ToByteArray(this string value, Encoding? encoding = null)
		{
			if (value is null)
				throw new NullReferenceException();

			return (encoding ?? Encoding.UTF8).GetBytes(value);
		}

		/// <summary>
		/// Converts a string to a sbyte array.
		/// </summary>
		/// <param name="value">The string value.</param>
		/// <param name="encoding">Default is UTF8.</param>
		public static sbyte[] ToSbyteArray(this string value, Encoding? encoding = null)
		{
			if (value is null)
				throw new NullReferenceException();

			return value.ToByteArray(encoding).ToSbyteArray();
		}

		/// <summary>
		/// Directly converts a byte array (byte-by-byte) to an sbyte array.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		public static sbyte[] ToSbyteArray(this byte[] bytes)
		{
			if (bytes is null)
				throw new NullReferenceException();

			var sbytes = new sbyte[bytes.Length];
			for (var i = 0; i < bytes.Length; i++)
				sbytes[i] = (sbyte)bytes[i];

			return sbytes;
		}
	}
}
