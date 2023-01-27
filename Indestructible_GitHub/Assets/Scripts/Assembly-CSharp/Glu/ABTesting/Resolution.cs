using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.UnityBuildSystem;
using UnityEngine;

namespace Glu.ABTesting
{
	public class Resolution
	{
		public class Resolver : MonoBehaviour
		{
			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.Resolution.Resolver");
				}
			}

			private const string kGameObjectName = "Glu.ABTesting.Resolution.Resolver GameObject";

			private static Resolver m_instance;

			public static void Resolve(Resolution resolution)
			{
				if (!resolution.m_wasPassedToResolver)
				{
					resolution.m_wasPassedToResolver = true;
					GetInstance().StartCoroutine("DoABTesting", resolution);
				}
			}

			private static Resolver GetInstance()
			{
				if (m_instance != null)
				{
					return m_instance;
				}
				ABTesting.Init();
				GameObject gameObject = GameObject.Find("Glu.ABTesting.Resolution.Resolver GameObject");
				if (gameObject != null)
				{
					m_instance = gameObject.GetComponent<GluABTestingResolutionResolver>();
					return m_instance;
				}
				gameObject = new GameObject("Glu.ABTesting.Resolution.Resolver GameObject");
				gameObject.AddComponent<GluABTestingResolutionResolver>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				m_instance = gameObject.GetComponent<GluABTestingResolutionResolver>();
				m_instance.useGUILayout = false;
				m_instance.enabled = false;
				return m_instance;
			}

			private IEnumerator DoABTesting(Resolution resolution)
			{
				string threadId = new System.Random().Next(10, 100).ToString();
				string[] urls = resolution.m_urlsToDecisionTable;
				bool isApplied2 = false;
				IDictionary<string, string> dict = null;
				DecisionTable decisionTable2 = null;
				for (int idx = 0; idx < urls.Length; idx++)
				{
					string url2 = urls[idx];
					string version = null;
					WWW www3 = null;
					url2 = url2.Replace('\\', '/');
					if (!url2.Contains("://"))
					{
						url2 = "file://" + url2;
					}
					if (url2.ToLower().EndsWith("version"))
					{
						www3 = new WWW(url2);
						yield return StartCoroutine(WWWCoroutine(www3));
						if (WWWSucceeded(www3))
						{
							using (StringReader sr = new StringReader(www3.text))
							{
								version = sr.ReadLine().Trim();
								if (version.Contains("<"))
								{
									version = null;
								}
							}
						}
						url2 = url2.Remove(url2.LastIndexOf("version")) + "xml";
					}
					if (!string.IsNullOrEmpty(version))
					{
						DecisionTable cachedTable = DecisionTable.Load(DefaultDecisionTableFilename);
						if (cachedTable != null && !string.IsNullOrEmpty(cachedTable.m_version) && cachedTable.m_version.Equals(version))
						{
							decisionTable2 = cachedTable;
							break;
						}
					}
					www3 = new WWW(url2);
					yield return StartCoroutine(WWWCoroutine(www3));
					if (WWWSucceeded(www3))
					{
						decisionTable2 = DecisionTable.Load(www3.bytes);
						if (decisionTable2 != null)
						{
							if (!string.IsNullOrEmpty(version))
							{
								decisionTable2.m_version = version;
								bool wasCached = decisionTable2.Save(DefaultDecisionTableFilename);
							}
							break;
						}
					}
					if (idx + 1 == urls.Length)
					{
					}
				}
				if (decisionTable2 != null)
				{
					ABTesting.Init(decisionTable2);
					dict = ABTesting.Resolve();
				}
				if (dict == null)
				{
					dict = LoadResolutionData();
					isApplied2 = true;
					if (dict == null)
					{
						byte[] rawBytes = null;
						TextAsset textAsset = Resources.Load("ABTesting") as TextAsset;
						if (textAsset != null)
						{
							rawBytes = textAsset.bytes;
						}
						else
						{
							string filename = Path.Combine(Application.streamingAssetsPath, "ABTesting.xml");
							try
							{
								if (File.Exists(filename))
								{
									rawBytes = File.ReadAllBytes(filename);
								}
							}
							catch (Exception ex)
							{
								Exception e = ex;
							}
						}
						if (rawBytes != null)
						{
							decisionTable2 = DecisionTable.Load(rawBytes);
							if (decisionTable2 != null)
							{
								ABTesting.Init(decisionTable2);
								dict = ABTesting.Resolve();
							}
						}
						if (dict == null)
						{
							dict = resolution.m_dict;
						}
						isApplied2 = false;
					}
				}
				else
				{
					IDictionary<string, string> oldDict = LoadResolutionData();
					if (oldDict != null)
					{
						if (oldDict.Count == dict.Count)
						{
							isApplied2 = true;
							foreach (KeyValuePair<string, string> kvp in oldDict)
							{
								if (dict.ContainsKey(kvp.Key))
								{
									if (!kvp.Value.Equals(dict[kvp.Key]))
									{
										isApplied2 = false;
										break;
									}
									continue;
								}
								isApplied2 = false;
								break;
							}
						}
						else
						{
							isApplied2 = false;
						}
					}
					else
					{
						isApplied2 = false;
					}
				}
				resolution.m_isApplied = isApplied2;
				resolution.m_dict = dict;
				resolution.m_isFinished = true;
			}

			private IEnumerator WWWCoroutine(WWW www)
			{
				DateTime dateTime = DateTime.Now;
				float progress = www.progress;
				while (!www.isDone && string.IsNullOrEmpty(www.error))
				{
					if (www.progress > progress)
					{
						dateTime = DateTime.Now;
						progress = www.progress;
					}
					else if ((DateTime.Now - dateTime).TotalSeconds > 5.0)
					{
						break;
					}
					yield return null;
				}
			}

			private bool WWWSucceeded(WWW www)
			{
				bool flag = www.isDone && string.IsNullOrEmpty(www.error);
				if (flag)
				{
					try
					{
						Dictionary<string, string> responseHeaders = www.responseHeaders;
						string text = responseHeaders["STATUS"];
						string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (!array[0].ToUpper().Contains("HTTP"))
						{
							return flag;
						}
						int num = (int)Convert.ChangeType(array[1], typeof(int));
						flag = num < 400;
						return flag;
					}
					catch (Exception)
					{
						return flag;
					}
				}
				return flag;
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.Resolution");
			}
		}

		private const string kDefaultDecisionTableFilename = "ABTestingDecisionTable.dat";

		private const string kDefaultResolutionDataFilename = "ABTestingResolutionData.dat";

		private string[] m_urlsToDecisionTable;

		private bool m_wasPassedToResolver;

		private bool m_isFinished;

		private bool m_isApplied;

		private IDictionary<string, string> m_dict;

		public bool Ready
		{
			get
			{
				return m_isFinished;
			}
		}

		public bool IsApplied
		{
			get
			{
				return Ready && m_isApplied;
			}
		}

		public IDictionary<string, string> Data
		{
			get
			{
				object result;
				if (Ready)
				{
					IDictionary<string, string> dict = m_dict;
					result = dict;
				}
				else
				{
					result = null;
				}
				return (IDictionary<string, string>)result;
			}
		}

		public string VariantId
		{
			get
			{
				string value = null;
				if (Data != null && !Data.TryGetValue("VariantId", out value))
				{
					value = null;
				}
				return value;
			}
		}

		private static string DefaultPathname
		{
			get
			{
				return (!string.IsNullOrEmpty(BuildInfo.buildTag)) ? Path.Combine(Application.temporaryCachePath, BuildInfo.buildTag) : Application.temporaryCachePath;
			}
		}

		private static string DefaultDecisionTableFilename
		{
			get
			{
				return Path.Combine(DefaultPathname, "ABTestingDecisionTable.dat");
			}
		}

		private static string DefaultResolutionDataFilename
		{
			get
			{
				return Path.Combine(DefaultPathname, "ABTestingResolutionData.dat");
			}
		}

		private Resolution(string[] urlsToDecisionTable, IDictionary<string, string> defaultResolutionData)
		{
			m_urlsToDecisionTable = urlsToDecisionTable;
			m_wasPassedToResolver = false;
			m_isFinished = false;
			m_isApplied = false;
			m_dict = defaultResolutionData;
		}

		public static Resolution Retrieve(IDictionary<string, string> initialResolutionData, params string[] urlsToDecisionTable)
		{
			Resolution resolution = null;
			if (urlsToDecisionTable != null && urlsToDecisionTable.Length > 0)
			{
				resolution = new Resolution(urlsToDecisionTable, initialResolutionData);
				Resolver.Resolve(resolution);
			}
			return resolution;
		}

		public bool Commit()
		{
			bool result = false;
			if (Ready)
			{
				if (!IsApplied && SaveResolutionData(Data))
				{
					m_isApplied = true;
				}
				result = IsApplied;
				bool flag = Data.Count <= 9;
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Clear();
				if (!flag || !Data.ContainsKey("VariantId"))
				{
					dictionary.Add("VariantId", VariantId ?? "null");
				}
				if (flag)
				{
					foreach (KeyValuePair<string, string> datum in Data)
					{
						dictionary.Add(datum.Key, datum.Value);
					}
				}
				CFlurry.LogEvent("ABTesting", dictionary);
			}
			return result;
		}

		private static IDictionary<string, string> LoadResolutionData()
		{
			IDictionary<string, string> dictionary = null;
			try
			{
				IList<KeyValuePair<string, string>> list = StorageManager.ReadXmlFromLocation(DefaultResolutionDataFilename, typeof(List<KeyValuePair<string, string>>)) as List<KeyValuePair<string, string>>;
				if (list != null)
				{
					dictionary = new Dictionary<string, string>();
					dictionary.Clear();
					{
						foreach (KeyValuePair<string, string> item in list)
						{
							dictionary.Add(item.Key, item.Value);
						}
						return dictionary;
					}
				}
				return dictionary;
			}
			catch (Exception)
			{
				return dictionary;
			}
		}

		private static bool SaveResolutionData(IDictionary<string, string> dict)
		{
			bool result = false;
			if (dict != null)
			{
				IList<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
				list.Clear();
				foreach (KeyValuePair<string, string> item in dict)
				{
					list.Add(item);
				}
				try
				{
					string directoryName = Path.GetDirectoryName(DefaultResolutionDataFilename);
					if (!Directory.Exists(directoryName))
					{
						try
						{
							Directory.CreateDirectory(directoryName);
						}
						catch (Exception)
						{
						}
					}
					StorageManager.WriteXmlToLocation(DefaultResolutionDataFilename, DefaultResolutionDataFilename, list);
					result = true;
					return result;
				}
				catch (Exception)
				{
					return result;
				}
			}
			if (File.Exists(DefaultResolutionDataFilename))
			{
				try
				{
					File.Delete(DefaultResolutionDataFilename);
					File.Delete(DefaultResolutionDataFilename + ".check");
					return result;
				}
				catch (Exception)
				{
					return result;
				}
			}
			return result;
		}
	}
}
