using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Glu.AssetBundles.Internal;
using Glu.UnityBuildSystem;
using UnityEngine;

namespace Glu.AssetBundles
{
	public class IndexInfo
	{
		public enum State
		{
			InProgress = 0,
			Succeeded = 1,
			Failed = 2
		}

		public enum Source
		{
			Online = 0,
			Cache = 1,
			StreamingAssets = 2,
			None = 3
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.IndexInfo");
			}
		}

		private static List<IndexInfo> m_indexInfo = new List<IndexInfo>();

		private string[] m_requestedUrls;

		private string[] m_urls;

		private DownloadManager.Info m_downloadInfo;

		private List<AssetBundleInfo> m_assetBundleInfo;

		private State m_state;

		private Source m_source;

		private string m_indexBuildTag;

		private string m_indexVersionFilename;

		private string m_indexHash;

		private string m_cachedIndexHash;

		private DownloadManager.OnDownloadCallbackType m_onDownloadAssetBundleFile;

		private DownloadManager.OnDownloadCallbackType m_onDownloadIndexFile;

		private DownloadManager.OnDownloadCallbackType m_onDownloadVersionFile;

		private Dictionary<string, AssetBundleInfo> m_assetToBundle;

		public State state
		{
			get
			{
				return m_state;
			}
		}

		public Source source
		{
			get
			{
				return m_source;
			}
		}

		public static IndexInfo[] Instances
		{
			get
			{
				return m_indexInfo.ToArray();
			}
		}

		public string[] Urls
		{
			get
			{
				return m_urls;
			}
		}

		public DownloadManager.Info downloadInfo
		{
			get
			{
				return m_downloadInfo;
			}
		}

		public string BuildTag
		{
			get
			{
				return m_indexBuildTag;
			}
		}

		public string ContentHash
		{
			get
			{
				return m_indexHash;
			}
		}

		public string CachedContentHash
		{
			get
			{
				return m_cachedIndexHash;
			}
		}

		public AssetBundleInfo[] assetBundleInfo
		{
			get
			{
				return m_assetBundleInfo.ToArray();
			}
		}

		private DownloadManager.OnDownloadCallbackType onDownloadVersionFile
		{
			get
			{
				return OnDownloadVersionFile;
			}
		}

		private DownloadManager.OnDownloadCallbackType onDownloadIndexFile
		{
			get
			{
				return OnDownloadIndexFile;
			}
		}

		private DownloadManager.OnDownloadCallbackType onDownloadAssetBundleFile
		{
			get
			{
				return OnDownloadAssetBundleFile;
			}
		}

		private IndexInfo(string[] urls)
		{
			m_requestedUrls = urls;
			m_urls = null;
			m_downloadInfo = null;
			m_assetBundleInfo = new List<AssetBundleInfo>();
			m_assetBundleInfo.Clear();
			m_state = State.Failed;
			m_source = Source.None;
			m_indexBuildTag = null;
			m_indexVersionFilename = null;
			m_indexHash = null;
			m_cachedIndexHash = null;
			m_assetToBundle = new Dictionary<string, AssetBundleInfo>();
			m_assetToBundle.Clear();
		}

		public static IndexInfo GetInstance(params string[] urls)
		{
			IndexInfo indexInfo = null;
			if (urls != null && urls.Length > 0)
			{
				for (int i = 0; i < urls.Length; i++)
				{
					urls[i] = urls[i].Replace('\\', '/');
					if (urls[i].LastIndexOf('/') == urls[i].Length - 1)
					{
						urls[i] = urls[i].Remove(urls[i].Length - 1);
					}
					if (urls[i].LastIndexOf('/') < urls[i].LastIndexOf("."))
					{
						urls[i] = urls[i].Remove(urls[i].LastIndexOf("."));
					}
					if (!urls[i].Contains("://"))
					{
						urls[i] = "file://" + urls[i];
					}
				}
				indexInfo = FindIndexInfoByUrls(urls);
				if (indexInfo == null)
				{
					indexInfo = new IndexInfo(urls);
					indexInfo.DownloadAll();
					m_indexInfo.Add(indexInfo);
				}
				else
				{
					indexInfo.m_requestedUrls = urls;
					if (indexInfo.m_state != 0)
					{
						indexInfo.DownloadAll();
					}
				}
			}
			return indexInfo;
		}

		public void DownloadAll()
		{
			UnloadAll(false);
			m_downloadInfo = null;
			m_assetBundleInfo.Clear();
			m_assetToBundle.Clear();
			m_urls = m_requestedUrls;
			m_indexBuildTag = null;
			m_indexVersionFilename = null;
			m_indexHash = null;
			m_cachedIndexHash = null;
			m_state = State.InProgress;
			m_source = Source.Online;
			DownloadVersionFile();
		}

		public UpdateInfo CheckForUpdates()
		{
			return UpdateInfo.CreateInstance(GetArrayOfVersions(), m_indexHash);
		}

		public bool Contains(string assetPath)
		{
			bool flag = false;
			if (m_state == State.Succeeded)
			{
				AssetBundleInfo value = null;
				assetPath = assetPath.Replace('\\', '/').ToLower();
				if (m_assetToBundle.TryGetValue(assetPath, out value) && value != null)
				{
					AssetBundle assetBundle = value.assetBundle;
					if (assetBundle != null)
					{
						flag = assetBundle.Contains(assetPath);
					}
				}
				if (flag)
				{
				}
			}
			return flag;
		}

		public UnityEngine.Object Load(string assetPath)
		{
			UnityEngine.Object @object = null;
			if (m_state == State.Succeeded)
			{
				if (@object == null)
				{
					AssetBundleInfo value = null;
					assetPath = assetPath.Replace('\\', '/').ToLower();
					if (m_assetToBundle.TryGetValue(assetPath, out value))
					{
						if (value != null)
						{
							AssetBundle assetBundle = value.assetBundle;
							if (assetBundle != null)
							{
								@object = assetBundle.Load(assetPath);
							}
						}
						if (!(@object == null))
						{
						}
					}
				}
				if (!(@object == null))
				{
				}
			}
			return @object;
		}

		public UnityEngine.Object Load(string assetPath, Type assetType)
		{
			UnityEngine.Object @object = null;
			if (m_state == State.Succeeded)
			{
				if (@object == null)
				{
					AssetBundleInfo value = null;
					assetPath = assetPath.Replace('\\', '/').ToLower();
					if (m_assetToBundle.TryGetValue(assetPath, out value))
					{
						if (value != null)
						{
							AssetBundle assetBundle = value.assetBundle;
							if (assetBundle != null)
							{
								@object = assetBundle.Load(assetPath, assetType);
							}
						}
						if (!(@object == null))
						{
						}
					}
				}
				if (!(@object == null))
				{
				}
			}
			return @object;
		}

		public void UnloadAll(bool unloadAllLoadedObjects)
		{
			AssetBundleInfo[] array = this.assetBundleInfo;
			foreach (AssetBundleInfo assetBundleInfo in array)
			{
				DownloadManager.Info info = assetBundleInfo.downloadInfo;
				info.Skip();
				AssetBundle assetBundle = info.assetBundle;
				if (assetBundle != null)
				{
					try
					{
						assetBundle.Unload(unloadAllLoadedObjects);
					}
					catch (Exception)
					{
					}
				}
			}
			if (downloadInfo == null || !(downloadInfo.assetBundle != null))
			{
				return;
			}
			downloadInfo.Skip();
			AssetBundle assetBundle2 = downloadInfo.assetBundle;
			if (assetBundle2 != null)
			{
				try
				{
					assetBundle2.Unload(unloadAllLoadedObjects);
				}
				catch (Exception)
				{
				}
			}
		}

		private static IndexInfo FindIndexInfoByUrls(params string[] urls)
		{
			IndexInfo indexInfo = null;
			foreach (IndexInfo item in m_indexInfo)
			{
				string[] urls2 = item.Urls;
				foreach (string path in urls2)
				{
					string fileName = Path.GetFileName(path);
					foreach (string path2 in urls)
					{
						string fileName2 = Path.GetFileName(path2);
						if (fileName.ToLower().Equals(fileName2.ToLower()))
						{
							indexInfo = item;
							break;
						}
					}
					if (indexInfo != null)
					{
						break;
					}
				}
				if (indexInfo != null)
				{
					break;
				}
			}
			return indexInfo;
		}

		private static void ParseUrl(string url, out string baseUrl, out string filename)
		{
			if (!string.IsNullOrEmpty(url))
			{
				int num = url.LastIndexOf('/');
				baseUrl = url.Remove(num + 1);
				filename = url.Substring(num + 1);
			}
			else
			{
				baseUrl = null;
				filename = null;
			}
		}

		private static string GetPathToCache()
		{
			string text = Application.temporaryCachePath;
			if (string.IsNullOrEmpty(text))
			{
				text = ".";
			}
			text += '/';
			if (!string.IsNullOrEmpty(BuildInfo.buildTag))
			{
				text = text + BuildInfo.buildTag + '/';
			}
			return text;
		}

		private static string GetBaseUrlToCache()
		{
			return "file://" + GetPathToCache();
		}

		private static string GetBaseUrlToStreamingAssets()
		{
			string result = null;
			switch (Application.platform)
			{
			case RuntimePlatform.Android:
				result = "jar:file://" + Application.dataPath + "!/assets/AssetBundles/";
				break;
			case RuntimePlatform.IPhonePlayer:
				result = "file://" + Application.dataPath + "/Raw/AssetBundles/";
				break;
			case RuntimePlatform.OSXPlayer:
				result = "file://" + Application.dataPath + "/Data/StreamingAssets/AssetBundles/";
				break;
			case RuntimePlatform.WindowsPlayer:
				result = "file://" + Application.dataPath + "/StreamingAssets/AssetBundles/";
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				result = "file://" + Application.dataPath + "/StreamingAssets/AssetBundles/";
				break;
			}
			return result;
		}

		private static string[] AdjustUrlsToSource(Source source, params string[] urls)
		{
			string[] result = null;
			string baseUrl;
			string filename;
			switch (source)
			{
			case Source.Online:
				result = urls;
				break;
			case Source.Cache:
				ParseUrl(urls[0], out baseUrl, out filename);
				result = new string[1] { GetBaseUrlToCache() + filename };
				break;
			case Source.StreamingAssets:
				ParseUrl(urls[0], out baseUrl, out filename);
				result = new string[1] { GetBaseUrlToStreamingAssets() + filename };
				break;
			case Source.None:
				result = urls;
				break;
			}
			return result;
		}

		private string[] GetArrayOfAssetBundles(string relativePath)
		{
			string[] array = new string[m_urls.Length];
			for (int i = 0; i < m_urls.Length; i++)
			{
				string baseUrl;
				string filename;
				ParseUrl(m_urls[i], out baseUrl, out filename);
				array[i] = baseUrl + relativePath;
			}
			return array;
		}

		private string[] GetArrayOfIndices()
		{
			string[] array = new string[m_urls.Length];
			for (int i = 0; i < m_urls.Length; i++)
			{
				array[i] = m_urls[i] + "_" + m_indexHash + ".unity3d";
			}
			return array;
		}

		private string[] GetArrayOfVersions()
		{
			string[] array = new string[m_urls.Length];
			for (int i = 0; i < m_urls.Length; i++)
			{
				array[i] = m_urls[i] + ".version";
			}
			return array;
		}

		private void SaveIndexVersionToCache()
		{
			if (m_indexHash == null)
			{
				return;
			}
			try
			{
				string text = GetPathToCache() + m_indexVersionFilename;
				string directoryName = Path.GetDirectoryName(text);
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
				byte[] bytes = new ASCIIEncoding().GetBytes(m_indexHash);
				StorageManager.WriteToLocation(text, text, bytes);
			}
			catch (Exception)
			{
			}
		}

		private void OnEntireSuccess()
		{
			if (m_source != Source.Cache)
			{
				SaveIndexVersionToCache();
			}
			m_state = State.Succeeded;
		}

		private void OnEntireFail()
		{
			UnloadAll(true);
			m_downloadInfo = null;
			m_assetBundleInfo.Clear();
			m_assetToBundle.Clear();
			m_indexBuildTag = null;
			m_indexVersionFilename = null;
			m_indexHash = null;
			switch (m_source)
			{
			case Source.Online:
				m_source = Source.Cache;
				DownloadVersionFile();
				break;
			case Source.Cache:
				m_source = Source.StreamingAssets;
				DownloadVersionFile();
				break;
			case Source.StreamingAssets:
				m_source = Source.None;
				m_state = State.Failed;
				break;
			case Source.None:
				break;
			}
		}

		private bool OnDownloadAssetBundleFile(DownloadManager.Info downloadInfo)
		{
			bool result = false;
			if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
			{
				result = true;
			}
			else
			{
				AssetBundleInfo[] array = this.assetBundleInfo;
				foreach (AssetBundleInfo assetBundleInfo in array)
				{
					assetBundleInfo.downloadInfo.Skip();
				}
			}
			bool flag = true;
			bool flag2 = true;
			AssetBundleInfo[] array2 = this.assetBundleInfo;
			foreach (AssetBundleInfo assetBundleInfo2 in array2)
			{
				switch (assetBundleInfo2.downloadInfo.state)
				{
				case DownloadManager.Info.State.NotStarted:
				case DownloadManager.Info.State.Pending:
				case DownloadManager.Info.State.Sending:
				case DownloadManager.Info.State.Receiving:
				case DownloadManager.Info.State.WaitingBeforeRetry:
					flag = false;
					flag2 = false;
					break;
				case DownloadManager.Info.State.Abandoned:
				case DownloadManager.Info.State.DoNotDownload:
					flag2 = false;
					break;
				}
			}
			if (flag)
			{
				if (flag2)
				{
					OnEntireSuccess();
				}
				else
				{
					OnEntireFail();
				}
			}
			return result;
		}

		private bool OnDownloadIndexFile(DownloadManager.Info downloadInfo)
		{
			bool result = false;
			if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
			{
				string baseUrl;
				string filename;
				ParseUrl(downloadInfo.LatestUrl, out baseUrl, out filename);
				Index index = null;
				if (downloadInfo.assetBundle != null)
				{
					try
					{
						TextAsset textAsset = downloadInfo.assetBundle.mainAsset as TextAsset;
						if (textAsset != null)
						{
							using (Stream stream = new MemoryStream(textAsset.bytes))
							{
								index = Index.LoadInstance(stream);
							}
						}
						downloadInfo.assetBundle.Unload(true);
					}
					catch (Exception)
					{
					}
				}
				if (index != null)
				{
					m_indexBuildTag = index.m_buildTag;
					foreach (Index.AssetBundle assetBundle in index.m_assetBundles)
					{
						List<string> list = new List<string>(assetBundle.m_urls.Length + 1);
						list.Clear();
						list.AddRange(AdjustUrlsToSource(Source.StreamingAssets, assetBundle.m_urls));
						string[] urls = assetBundle.m_urls;
						foreach (string text in urls)
						{
							if (text.IndexOf(".") == 0)
							{
								if (!string.IsNullOrEmpty(baseUrl))
								{
									string item = baseUrl + text.Substring(2);
									if (!list.Contains(item))
									{
										list.Add(item);
									}
								}
								string[] arrayOfAssetBundles = GetArrayOfAssetBundles(text.Substring(2));
								foreach (string item2 in arrayOfAssetBundles)
								{
									if (!list.Contains(item2))
									{
										list.Add(item2);
									}
								}
							}
							else if (!list.Contains(text))
							{
								list.Add(text);
							}
						}
						AssetBundleInfo assetBundleInfo = new AssetBundleInfo(assetBundle.m_filename, assetBundle.Size, assetBundle.m_contentHash, assetBundle.m_type.ToString(), list.ToArray(), onDownloadAssetBundleFile);
						m_assetBundleInfo.Add(assetBundleInfo);
						foreach (Index.AssetBundle.Asset asset in assetBundle.m_assets)
						{
							try
							{
								string key = asset.m_filename.ToLower();
								if (m_assetToBundle.ContainsKey(key))
								{
									m_assetToBundle[key] = assetBundleInfo;
								}
								else
								{
									m_assetToBundle.Add(key, assetBundleInfo);
								}
							}
							catch (Exception)
							{
							}
						}
					}
					result = true;
				}
			}
			else
			{
				OnEntireFail();
			}
			return result;
		}

		private bool OnDownloadVersionFile(DownloadManager.Info downloadInfo)
		{
			bool result = false;
			if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
			{
				string baseUrl;
				string filename;
				ParseUrl(downloadInfo.LatestUrl, out baseUrl, out filename);
				string text = downloadInfo.text.Substring(0, 32);
				if (RTUtils.IsHash(text))
				{
					m_indexVersionFilename = filename;
					m_indexHash = text;
					try
					{
						string pathName = GetPathToCache() + m_indexVersionFilename;
						byte[] array = StorageManager.ReadFromLocation(pathName);
						if (array != null && array.Length > 0)
						{
							m_cachedIndexHash = new ASCIIEncoding().GetString(array);
						}
					}
					catch (Exception)
					{
					}
					List<string> list = new List<string>();
					list.AddRange(GetArrayOfIndices());
					if (m_source == Source.Online && list.Count > 1)
					{
						for (int i = 0; i < list.Count; i++)
						{
							if (list[i].StartsWith(baseUrl))
							{
								string item = list[i];
								list.RemoveAt(i);
								list.Insert(0, item);
								break;
							}
						}
					}
					List<string> list2 = new List<string>(list.Count + 1);
					list2.Clear();
					list2.AddRange(AdjustUrlsToSource(Source.StreamingAssets, list.ToArray()));
					list2.AddRange(list);
					int version = RTUtils.HashToVersion(text);
					m_downloadInfo = DownloadManager.LoadFromCacheOrDownloadAsync(list2.ToArray(), version, onDownloadIndexFile);
					result = true;
				}
			}
			else
			{
				OnEntireFail();
			}
			return result;
		}

		private void DownloadVersionFile()
		{
			if (m_source != Source.None)
			{
				string[] arrayOfVersions = GetArrayOfVersions();
				arrayOfVersions = AdjustUrlsToSource(m_source, arrayOfVersions);
				DownloadManager.LoadFromCacheOrDownloadAsync(arrayOfVersions, int.MinValue, onDownloadVersionFile);
			}
		}
	}
}
