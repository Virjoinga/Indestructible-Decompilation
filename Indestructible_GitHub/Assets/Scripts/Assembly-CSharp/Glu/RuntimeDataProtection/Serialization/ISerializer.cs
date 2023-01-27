using System;
using System.IO;

namespace Glu.RuntimeDataProtection.Serialization
{
	public interface ISerializer
	{
		void Serialize(Stream stream, object obj);

		object Deserialize(Stream stream, Type type);
	}
}
