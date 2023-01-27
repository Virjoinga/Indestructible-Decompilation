using System;
using System.IO;
using Glu.RuntimeDataProtection.Serialization;

namespace Glu.RuntimeDataProtection.Internal
{
	internal static class StaticData
	{
		[ThreadStatic]
		private static ISerializer defaultSerializer;

		[ThreadStatic]
		private static MemoryStream memoryStream;

		private static Random random;

		public static ISerializer DefaultSerializer
		{
			get
			{
				if (defaultSerializer == null)
				{
					defaultSerializer = new SpecializedSerializer();
				}
				return defaultSerializer;
			}
		}

		public static MemoryStream MemoryStream
		{
			get
			{
				if (memoryStream == null)
				{
					memoryStream = new MemoryStream();
				}
				return memoryStream;
			}
		}

		public static uint GenerateUInt()
		{
			if (random == null)
			{
				random = new Random();
			}
			uint num = (uint)random.Next(65536);
			uint num2 = (uint)random.Next(65536);
			return (num << 16) | num2;
		}
	}
}
