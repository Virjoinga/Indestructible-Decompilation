using System;
using System.Runtime.Serialization;

namespace Glu.AssetMapping
{
	[Serializable]
	public class AssetMappingException : Exception
	{
		public AssetMappingException()
		{
		}

		public AssetMappingException(string message)
			: base(message)
		{
		}

		public AssetMappingException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected AssetMappingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
