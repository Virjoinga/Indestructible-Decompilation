using System;
using System.Runtime.Serialization;

namespace Glu.AssetMapping
{
	[Serializable]
	public class InvalidDataException : AssetMappingException
	{
		public InvalidDataException()
		{
		}

		public InvalidDataException(string message)
			: base(message)
		{
		}

		public InvalidDataException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected InvalidDataException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
