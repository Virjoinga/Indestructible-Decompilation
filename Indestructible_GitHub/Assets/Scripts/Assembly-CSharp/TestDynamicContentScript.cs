using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;
using UnityEngine;

public class TestDynamicContentScript : MonoBehaviour
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("App.TestDynamicContentScript");
		}
	}

	private class CustomDynamicContent : ICustomDynamicContent
	{
		private bool m_result;

		public float Progress
		{
			get
			{
				return 1f;
			}
		}

		public bool IsInProgress
		{
			get
			{
				return false;
			}
		}

		public bool Result
		{
			get
			{
				return m_result;
			}
		}

		public long LastUpdateSize
		{
			get
			{
				return 0L;
			}
		}

		public void StartContentUpdate(Glu.ABTesting.Resolution resolution)
		{
			m_result = true;
		}

		public void CheckForUpdates(Glu.ABTesting.Resolution resolution)
		{
			m_result = false;
		}

		public void InvalidateLastCheckReport()
		{
		}
	}

	private void Start()
	{
		DownloadManager.ThreadCount = 3;
		DynamicContent.OnForcedBinariesUpdate = delegate
		{
		};
		DynamicContent.OnAvailableBinariesUpdate = () => false;
		DynamicContent.OnFail = delegate
		{
		};
		DynamicContent.OnSuccess = delegate
		{
		};
		DynamicContent.CustomDynamicContent.Add(new CustomDynamicContent());
		DynamicContent.Init("http://glusvn.s3.amazonaws.com/mygame-stage");
		DynamicContent.StartContentUpdate();
	}

	private void Update()
	{
	}
}
