using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class ForcedUpdate : MonoBehaviour
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.ForcedUpdate");
		}
	}

	private const string GAME_OBJECT_NAME = "ForcedUpdateGameObject";

	private static ForcedUpdate _instance;

	private WWW _www;

	private bool _needToUpdate;

	private bool _needToQuit;

	private string _updateURL = string.Empty;

	private string _lockFileName;

	private string _checkStatusURL;

	private string _currentVersion;

	public static void Init(string url, string currentVersion)
	{
		if (_instance == null)
		{
			GameObject gameObject = new GameObject("ForcedUpdateGameObject");
			_instance = gameObject.AddComponent<ForcedUpdate>();
			_instance._checkStatusURL = url;
			_instance._currentVersion = currentVersion;
			_instance._lockFileName = Application.temporaryCachePath + "/fucache" + currentVersion + ".dat";
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static bool CheckUpdateStatus()
	{
		bool result = false;
		if (File.Exists(_instance._lockFileName))
		{
			string text = File.ReadAllText(_instance._lockFileName);
			string[] array = text.Split(';');
			_instance._updateURL = array[0];
			_instance._needToUpdate = bool.Parse(array[1]);
			_instance._needToQuit = bool.Parse(array[2]);
			result = true;
		}
		_instance.CheckForUpdateConfig();
		return result;
	}

	public static bool IsCheckInProgress()
	{
		return _instance._www != null;
	}

	public static bool NeedToUpdate()
	{
		return _instance._needToUpdate;
	}

	public static bool NeedToQuit()
	{
		return _instance._needToQuit;
	}

	public static string GetUpdateURL()
	{
		return _instance._updateURL;
	}

	private void CheckForUpdateConfig()
	{
		if (_www == null)
		{
			_www = new WWW(_checkStatusURL);
			StartCoroutine(WaitForConfigFile());
		}
	}

	private IEnumerator WaitForConfigFile()
	{
		yield return _www;
		string errorMessage = null;
		if (_www != null && _www.error == null && _www.text != null)
		{
			try
			{
				UTF8Encoding utf8 = new UTF8Encoding();
				XmlDocument doc = new XmlDocument();
				doc.XmlResolver = null;
				doc.LoadXml(utf8.GetString(_www.bytes));
				XmlNodeList obsoleteVersionNodes = doc.SelectNodes("forced_update/obsolete_version");
				XmlNodeList deprecatedVersionNodes = doc.SelectNodes("forced_update/deprecated_version");
				bool foundCurrentVersion = false;
				foreach (XmlNode node2 in deprecatedVersionNodes)
				{
					if (_currentVersion.CompareTo(node2.Attributes["version"].Value) == 0)
					{
						_needToUpdate = true;
						_needToQuit = false;
						foundCurrentVersion = true;
						break;
					}
				}
				foreach (XmlNode node in obsoleteVersionNodes)
				{
					if (_currentVersion.CompareTo(node.Attributes["version"].Value) == 0)
					{
						_needToUpdate = true;
						_needToQuit = true;
						foundCurrentVersion = true;
						break;
					}
				}
				if (foundCurrentVersion)
				{
					XmlNodeList updateURLNode = doc.SelectNodes("forced_update/update_url");
					_updateURL = updateURLNode.Item(0).Attributes["url"].Value;
					File.WriteAllText(_lockFileName, _updateURL + ";" + _needToUpdate + ";" + _needToQuit);
				}
				else
				{
					if (File.Exists(_lockFileName))
					{
						File.Delete(_lockFileName);
					}
					_needToUpdate = false;
					_needToQuit = false;
				}
			}
			catch (Exception e)
			{
				errorMessage = (("Update XML file reading failed with exception:" + e.GetType().Name + "\n" + e.Message == null) ? "NULL" : e.Message);
			}
		}
		else
		{
			errorMessage = "WWW failed with error:" + _www.error;
		}
		_www = null;
		if (errorMessage == null)
		{
		}
	}
}
