using System;
using System.IO;
using System.Runtime.Serialization;

namespace Glu.RuntimeDataProtection.Serialization
{
	public class FormatterSerializerAdapter : ISerializer
	{
		private IFormatter formatter;

		public FormatterSerializerAdapter(IFormatter formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}
			this.formatter = formatter;
		}

		public void Serialize(Stream stream, object obj)
		{
			formatter.Serialize(stream, obj);
		}

		public object Deserialize(Stream stream, Type type)
		{
			return formatter.Deserialize(stream);
		}
	}
}
