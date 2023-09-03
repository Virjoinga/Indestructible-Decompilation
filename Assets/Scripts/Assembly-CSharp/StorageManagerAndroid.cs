using System;
using System.IO;

public class StorageManagerAndroid : StorageManager
{
	public new static void Write(string fileName, byte[] data)
	{
		string temporaryPathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		string pathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		StorageManager.WriteToLocation(pathName, temporaryPathName, data);
		AJavaTools.Backup.DataChanged();
	}

	public new static byte[] Read(string fileName)
	{
		string pathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		return StorageManager.ReadFromLocation(pathName);
	}

	public new static void WriteXml(string fileName, object data)
	{
		string temporaryPathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		string pathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		StorageManager.WriteXmlToLocation(pathName, temporaryPathName, data);
		AJavaTools.Backup.DataChanged();
	}

	public new static object ReadXml(string fileName, Type type)
	{
		string pathName = Path.Combine(GameConstants.AndroidFilePath, fileName);
		return StorageManager.ReadXmlFromLocation(pathName, type);
	}
}
