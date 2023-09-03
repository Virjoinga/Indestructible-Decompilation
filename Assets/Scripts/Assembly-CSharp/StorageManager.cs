using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class StorageManager
{
	public class StorageManagerException : Exception
	{
		public StorageManagerException()
		{
		}

		public StorageManagerException(string message)
			: base(message)
		{
		}
	}

	public static Version Version
	{
		get
		{
			return new Version(1, 1, 1);
		}
	}

	public static string PersistentDataPath
	{
		get
		{
			return Application.persistentDataPath;
		}
	}

	public static string TemporaryCachePath
	{
		get
		{
			return Application.temporaryCachePath;
		}
	}

	public static void WriteToLocation(string pathName, string temporaryPathName, byte[] data)
	{
		string text = pathName + ".check";
		string text2 = temporaryPathName + ".check.tmp";
		temporaryPathName += ".tmp";
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] array = sHA256Managed.ComputeHash(data);
		using (FileStream fileStream = File.Create(temporaryPathName))
		{
			fileStream.Write(data, 0, data.Length);
		}
		using (FileStream fileStream2 = File.Create(text2))
		{
			fileStream2.Write(array, 0, array.Length);
		}
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		File.Move(text2, text);
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void Write(string fileName, byte[] data)
	{
		string temporaryPathName = Path.Combine(TemporaryCachePath, fileName);
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteToLocation(pathName, temporaryPathName, data);
	}

	public static byte[] ReadFromLocation(string pathName)
	{
		byte[] array = File.ReadAllBytes(pathName);
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] strB = sHA256Managed.ComputeHash(array);
		byte[] array2 = null;
		string path = pathName + ".check";
		if (!File.Exists(path))
		{
			throw new StorageManagerException("Hash file for " + pathName + " not found");
		}
		array2 = File.ReadAllBytes(path);
		if (!ByteArrayEquals(array2, strB))
		{
			throw new StorageManagerException("File " + pathName + " is corrupted");
		}
		return array;
	}

	public static byte[] ReadFromLocationWithoutChecking(string pathName)
	{
		return File.ReadAllBytes(pathName);
	}

	public static byte[] Read(string fileName)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadFromLocation(pathName);
	}

	public static byte[] ReadWithoutChecking(string fileName)
	{
		string path = Path.Combine(PersistentDataPath, fileName);
		return File.ReadAllBytes(path);
	}

	public static void WriteXmlToLocation(string pathName, string temporaryPathName, object data)
	{
		temporaryPathName += ".tmp";
		MemoryStream memoryStream = new MemoryStream();
		XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());
		xmlSerializer.Serialize(XmlWriter.Create(memoryStream), data);
		SHA256Managed sHA256Managed = new SHA256Managed();
		memoryStream.Position = 0L;
		byte[] array = sHA256Managed.ComputeHash(memoryStream);
		using (FileStream stream = File.Create(temporaryPathName))
		{
			memoryStream.WriteTo(stream);
		}
		string text = pathName + ".check";
		string text2 = temporaryPathName + ".check.tmp";
		using (FileStream fileStream = File.Create(text2))
		{
			fileStream.Write(array, 0, array.Length);
		}
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		File.Move(text2, text);
		if (File.Exists(pathName))
		{
			File.Delete(pathName);
		}
		File.Move(temporaryPathName, pathName);
	}

	public static void WriteXml(string fileName, object data)
	{
		string temporaryPathName = Path.Combine(TemporaryCachePath, fileName);
		string pathName = Path.Combine(PersistentDataPath, fileName);
		WriteXmlToLocation(pathName, temporaryPathName, data);
	}

	public static object ReadXmlFromLocation(string pathName, Type type)
	{
		object obj = null;
		MemoryStream memoryStream = null;
		using (FileStream fileStream = File.OpenRead(pathName))
		{
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			memoryStream = new MemoryStream(array);
		}
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] strB = sHA256Managed.ComputeHash(memoryStream);
		byte[] array2 = null;
		string path = pathName + ".check";
		if (!File.Exists(path))
		{
			throw new StorageManagerException("Hash file for " + pathName + " not found");
		}
		array2 = File.ReadAllBytes(path);
		if (!ByteArrayEquals(array2, strB))
		{
			throw new StorageManagerException("File " + pathName + " is corrupted");
		}
		memoryStream.Position = 0L;
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		return xmlSerializer.Deserialize(XmlReader.Create(memoryStream));
	}

	public static object ReadXml(string fileName, Type type)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadXmlFromLocation(pathName, type);
	}

	public static object ReadXmlFromLocationWithoutChecking(string pathName, Type type)
	{
		object obj = null;
		using (StreamReader textReader = new FileInfo(pathName).OpenText())
		{
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			return xmlSerializer.Deserialize(textReader);
		}
	}

	public static object ReadXmlWithoutChecking(string fileName, Type type)
	{
		string pathName = Path.Combine(PersistentDataPath, fileName);
		return ReadXmlFromLocationWithoutChecking(pathName, type);
	}

	private static bool ByteArrayEquals(byte[] strA, byte[] strB)
	{
		int num = strA.Length;
		if (num != strB.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (strA[i] != strB[i])
			{
				return false;
			}
		}
		return true;
	}
}
