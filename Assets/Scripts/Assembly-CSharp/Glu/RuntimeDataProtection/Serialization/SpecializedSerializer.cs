using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Glu.RuntimeDataProtection.Serialization
{
	public class SpecializedSerializer : ISerializer
	{
		public void Serialize(Stream stream, object obj)
		{
			if (object.ReferenceEquals(stream, null))
			{
				throw new ArgumentNullException("stream");
			}
			if (object.ReferenceEquals(obj, null))
			{
				throw new ArgumentNullException("obj");
			}
			Type type = obj.GetType();
			if (type == typeof(int))
			{
				BinaryWriter binaryWriter = new BinaryWriter(stream);
				binaryWriter.Write((int)obj);
				binaryWriter.Flush();
			}
			else if (type == typeof(float))
			{
				BinaryWriter binaryWriter2 = new BinaryWriter(stream);
				binaryWriter2.Write((float)obj);
				binaryWriter2.Flush();
			}
			else if (type == typeof(string))
			{
				string text = (string)obj;
				BinaryWriter binaryWriter3 = new BinaryWriter(stream);
				binaryWriter3.Write(text.Length);
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				stream.Write(bytes, 0, bytes.Length);
				binaryWriter3.Flush();
			}
			else
			{
				XmlSerializer xmlSerializer = new XmlSerializer(type);
				xmlSerializer.Serialize(stream, obj);
			}
		}

		public object Deserialize(Stream stream, Type type)
		{
			if (object.ReferenceEquals(stream, null))
			{
				throw new ArgumentNullException("stream");
			}
			if (object.ReferenceEquals(type, null))
			{
				throw new ArgumentNullException("type");
			}
			if (type == typeof(int))
			{
				BinaryReader binaryReader = new BinaryReader(stream);
				return binaryReader.ReadInt32();
			}
			if (type == typeof(float))
			{
				BinaryReader binaryReader2 = new BinaryReader(stream);
				return binaryReader2.ReadSingle();
			}
			if (type == typeof(string))
			{
				BinaryReader binaryReader3 = new BinaryReader(stream);
				int num = binaryReader3.ReadInt32();
				byte[] array = new byte[num];
				stream.Read(array, 0, num);
				return Encoding.UTF8.GetString(array);
			}
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			return xmlSerializer.Deserialize(stream);
		}
	}
}
