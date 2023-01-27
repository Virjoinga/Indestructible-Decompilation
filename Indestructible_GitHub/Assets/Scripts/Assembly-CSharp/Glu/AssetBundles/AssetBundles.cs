using System;
using UnityEngine;

namespace Glu.AssetBundles
{
	public static class AssetBundles
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles");
			}
		}

		private static Version m_version;

		public static Version version
		{
			get
			{
				return m_version ?? (m_version = new Version(0, 9, 5));
			}
		}

		public static IndexInfo DownloadAll(params string[] urls)
		{
			return IndexInfo.GetInstance(urls);
		}

		public static bool Contains(string assetPath)
		{
			bool flag = false;
			IndexInfo[] instances = IndexInfo.Instances;
			foreach (IndexInfo indexInfo in instances)
			{
				flag = indexInfo.Contains(assetPath);
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public static UnityEngine.Object Load(string assetPath)
		{
			UnityEngine.Object @object = null;
			if (@object == null)
			{
				IndexInfo[] instances = IndexInfo.Instances;
				foreach (IndexInfo indexInfo in instances)
				{
					@object = indexInfo.Load(assetPath);
					if (@object != null)
					{
						break;
					}
				}
			}
			if (@object == null)
			{
			}
			return @object;
		}

		public static UnityEngine.Object Load(string assetPath, Type assetType)
		{
			UnityEngine.Object @object = null;
			if (@object == null)
			{
				IndexInfo[] instances = IndexInfo.Instances;
				foreach (IndexInfo indexInfo in instances)
				{
					@object = indexInfo.Load(assetPath, assetType);
					if (@object != null)
					{
						break;
					}
				}
			}
			if (@object == null)
			{
			}
			return @object;
		}

		public static void UnloadAll(bool unloadAllLoadedObjects)
		{
			IndexInfo[] instances = IndexInfo.Instances;
			foreach (IndexInfo indexInfo in instances)
			{
				indexInfo.UnloadAll(unloadAllLoadedObjects);
			}
		}
	}
}
