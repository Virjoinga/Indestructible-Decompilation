using System;
using System.Runtime.Serialization;

namespace Glu.Localization
{
	[Serializable]
	public class InvalidDataException : LocalizationException
	{
		public InvalidDataException()
		{
		}

		public InvalidDataException(string message)
			: base(message)
		{
		}

		public InvalidDataException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected InvalidDataException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
