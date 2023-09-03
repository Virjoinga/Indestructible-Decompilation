using System;

namespace Glu.RuntimeDataProtection
{
	public static class Encryption
	{
		public static uint Encrypt(byte[] arr, int offset, int length, uint key)
		{
			if (arr == null)
			{
				throw new ArgumentNullException("arr");
			}
			if (offset < 0 || offset > arr.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			int num = offset + length;
			if (length < 0 || num > arr.Length)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			uint num2 = key;
			for (int i = offset; i < num; i++)
			{
				byte b = (byte)(num2 & 0xFFu);
				arr[i] ^= b;
				num2 = GenerateNextUInt(num2);
			}
			return num2;
		}

		private static uint GenerateNextUInt(uint value)
		{
			return (uint)((22695477L * (long)value + 1) & 0xFFFFFFFFu);
		}
	}
}
