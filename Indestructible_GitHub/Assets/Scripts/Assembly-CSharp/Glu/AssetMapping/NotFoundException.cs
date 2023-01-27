using System;
using System.Runtime.Serialization;

namespace Glu.AssetMapping
{
	[Serializable]
	public class NotFoundException : AssetMappingException
	{
		public NotFoundException()
		{
		}

		public NotFoundException(string message)
			: base(message)
		{
		}

		public NotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected NotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
