using Glu.AssetBundles.Internal;
using UnityEngine;

namespace Glu.AssetBundles
{
	public class AssetBundleInfo
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.IndexInfo.AssetBundleInfo");
			}
		}

		private DownloadManager.Info m_downloadInfo;

		private string m_filename;

		private string m_internalName;

		private long m_size;

		private string m_type;

		public DownloadManager.Info downloadInfo
		{
			get
			{
				return m_downloadInfo;
			}
		}

		public AssetBundle assetBundle
		{
			get
			{
				return downloadInfo.assetBundle;
			}
		}

		public string Filename
		{
			get
			{
				return m_filename;
			}
		}

		public long Size
		{
			get
			{
				return m_size;
			}
		}

		public string Type
		{
			get
			{
				return m_type;
			}
		}

		public AssetBundleInfo(string filename, long size, string contentHash, string type, string[] urls, DownloadManager.OnDownloadCallbackType onDownloadCallback)
		{
			m_filename = filename;
			m_size = size;
			int version = RTUtils.HashToVersion(contentHash);
			m_type = type;
			m_downloadInfo = DownloadManager.LoadFromCacheOrDownloadAsync(urls, version, onDownloadCallback);
		}
	}
}
