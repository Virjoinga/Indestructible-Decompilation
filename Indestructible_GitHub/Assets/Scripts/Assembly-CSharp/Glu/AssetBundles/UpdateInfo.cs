using Glu.AssetBundles.Internal;

namespace Glu.AssetBundles
{
	public class UpdateInfo
	{
		public enum State
		{
			Pending = 0,
			Unreachable = 1,
			UpToDate = 2,
			Outdated = 3
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.IndexInfo.UpdateInfo");
			}
		}

		private State m_state;

		private string m_curIndexHash;

		private DownloadManager.OnDownloadCallbackType m_onDownload;

		public State state
		{
			get
			{
				return m_state;
			}
		}

		private DownloadManager.OnDownloadCallbackType onDownload
		{
			get
			{
				return OnDownload;
			}
		}

		private UpdateInfo(string[] versionUrls, string curIndexHash)
		{
			m_state = State.Pending;
			m_curIndexHash = curIndexHash;
		}

		public static UpdateInfo CreateInstance(string[] versionUrls, string curIndexHash)
		{
			UpdateInfo updateInfo = null;
			if (versionUrls != null && versionUrls.Length > 0 && !string.IsNullOrEmpty(curIndexHash))
			{
				updateInfo = new UpdateInfo(versionUrls, curIndexHash);
				updateInfo.Download(versionUrls);
			}
			return updateInfo;
		}

		private bool OnDownload(DownloadManager.Info downloadInfo)
		{
			bool result = false;
			if (downloadInfo.state == DownloadManager.Info.State.Downloaded)
			{
				result = true;
				if (m_curIndexHash != null)
				{
					string text = downloadInfo.text.Substring(0, 32);
					if (RTUtils.IsHash(text))
					{
						if (m_curIndexHash.Equals(text))
						{
							m_state = State.UpToDate;
						}
						else
						{
							m_state = State.Outdated;
						}
					}
					else
					{
						result = false;
					}
				}
			}
			else
			{
				m_state = State.Unreachable;
			}
			return result;
		}

		private void Download(string[] versionUrls)
		{
			DownloadManager.LoadFromCacheOrDownloadAsync(versionUrls, int.MinValue, onDownload);
		}
	}
}
