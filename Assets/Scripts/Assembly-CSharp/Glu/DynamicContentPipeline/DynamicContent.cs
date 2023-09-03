using System;
using System.Collections;
using System.Collections.Generic;
using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.UnityBuildSystem;
using UnityEngine;

namespace Glu.DynamicContentPipeline
{
	public static class DynamicContent
	{
		public class CheckReport
		{
			public enum UpdateScope
			{
				All = 0,
				ForcedUpdate = 1,
				ABTesting = 2,
				AssetBundles = 3,
				CustomDynamicContent = 4,
				Nothing = 5
			}

			public UpdateScope updateScope { get; private set; }

			public Glu.ABTesting.Resolution ChangedABTestingResolution { get; private set; }

			public ICustomDynamicContent[] ChangedCustomDynamicContent { get; private set; }

			public CheckReport(UpdateScope updateScope)
			{
				this.updateScope = updateScope;
			}

			public CheckReport(UpdateScope updateScope, Glu.ABTesting.Resolution resolution)
				: this(updateScope)
			{
				ChangedABTestingResolution = resolution;
			}

			public CheckReport(UpdateScope updateScope, ICustomDynamicContent[] cdcs)
				: this(updateScope)
			{
				ChangedCustomDynamicContent = cdcs;
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.DynamicContent");
			}
		}

		public class Impl : MonoBehaviour
		{
			private static class FlurryEvents
			{
				private static long m_nAttempt = 0L;

				private static DateTime m_overallTime0 = DateTime.MaxValue;

				private static DateTime m_forcedUpdateTime0 = DateTime.MaxValue;

				private static DateTime m_aBTestingTime0 = DateTime.MaxValue;

				private static DateTime m_assetBundlesTime0 = DateTime.MaxValue;

				private static DateTime m_customContentTime0 = DateTime.MaxValue;

				private static long m_totalSizeInBytes = 0L;

				public static void LogDCPStarted()
				{
					m_nAttempt++;
					m_overallTime0 = DateTime.UtcNow;
					m_totalSizeInBytes = 0L;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("No.", m_nAttempt);
					CFlurry.LogEvent("DCP_UPDATE_START", dictionary);
				}

				public static void LogDCPFinished(bool success)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", (!success) ? "fail" : "success");
					double spentSeconds = GetSpentSeconds(m_overallTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("download_size_kbytes", m_totalSizeInBytes / 1024);
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_UPDATE_END", dictionary);
				}

				public static void LogForcedUpdateStarted()
				{
					m_forcedUpdateTime0 = DateTime.UtcNow;
					Dictionary<string, object> eventParams = new Dictionary<string, object>();
					CFlurry.LogEvent("DCP_FORCED_UPDATE_START", eventParams);
				}

				public static void LogForcedUpdateFinished(bool firedTimeout)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", ForcedUpdate.NeedToQuit() ? "needToQuit" : ((!ForcedUpdate.NeedToUpdate()) ? "upToDate" : "needToUpdate"));
					dictionary.Add("fired_timeout", firedTimeout);
					double spentSeconds = GetSpentSeconds(m_forcedUpdateTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_FORCED_UPDATE_END", dictionary);
				}

				public static void LogABTestingStarted()
				{
					m_aBTestingTime0 = DateTime.UtcNow;
					Dictionary<string, object> eventParams = new Dictionary<string, object>();
					CFlurry.LogEvent("DCP_ABTESTING_RULES_START", eventParams);
				}

				public static void LogABTestingFinished(Glu.ABTesting.Resolution resolution)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("variant_applied", resolution.IsApplied);
					dictionary.Add("variant_id", resolution.VariantId);
					double spentSeconds = GetSpentSeconds(m_aBTestingTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_ABTESTING_RULES_END", dictionary);
				}

				public static void LogAssetBundlesStarted(string urls)
				{
					m_assetBundlesTime0 = DateTime.UtcNow;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("url", urls ?? "none");
					CFlurry.LogEvent("DCP_BUNDLES_START", dictionary);
				}

				public static void LogAssetBundlesFinished(IndexInfo indexInfo)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("result", (indexInfo.state != IndexInfo.State.Succeeded) ? "fail" : "success");
					dictionary.Add("source", indexInfo.source);
					dictionary.Add("loaded_tag", indexInfo.BuildTag ?? "none");
					dictionary.Add("cached_hash", indexInfo.CachedContentHash ?? "none");
					dictionary.Add("loaded_hash", indexInfo.ContentHash ?? "none");
					double spentSeconds = GetSpentSeconds(m_assetBundlesTime0);
					dictionary.Add("time_seconds", Round(spentSeconds));
					dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
					long num = 0L;
					if (indexInfo.assetBundleInfo != null)
					{
						AssetBundleInfo[] assetBundleInfo = indexInfo.assetBundleInfo;
						foreach (AssetBundleInfo assetBundleInfo2 in assetBundleInfo)
						{
							if (assetBundleInfo2.downloadInfo.cacheStatus == DownloadManager.Info.CacheStatus.JustCached)
							{
								num += assetBundleInfo2.Size;
							}
						}
						m_totalSizeInBytes += num;
					}
					dictionary.Add("download_size_kbytes", num / 1024);
					dictionary.Add("network_type", GetNetworkType());
					CFlurry.LogEvent("DCP_BUNDLES_END", dictionary);
				}

				public static void LogCustomContentStarted()
				{
					if (CustomDynamicContent.Count > 0)
					{
						m_customContentTime0 = DateTime.UtcNow;
						Dictionary<string, object> eventParams = new Dictionary<string, object>();
						CFlurry.LogEvent("DCP_CONFIGS_START", eventParams);
					}
				}

				public static void LogCustomContentFinished(bool success, params ICustomDynamicContent[] customDynamicContentsToLog)
				{
					ICustomDynamicContent[] array = customDynamicContentsToLog ?? CustomDynamicContent.ToArray();
					if (array.Length > 0)
					{
						Dictionary<string, object> dictionary = new Dictionary<string, object>();
						dictionary.Add("result", (!success) ? "fail" : "success");
						double spentSeconds = GetSpentSeconds(m_customContentTime0);
						dictionary.Add("time_seconds", Round(spentSeconds));
						dictionary.Add("time_bracket", GetTimeInterval(spentSeconds));
						long num = 0L;
						ICustomDynamicContent[] array2 = array;
						foreach (ICustomDynamicContent customDynamicContent in array2)
						{
							num += customDynamicContent.LastUpdateSize;
						}
						m_totalSizeInBytes += num;
						dictionary.Add("download_size_kbytes", num / 1024);
						dictionary.Add("network_type", GetNetworkType());
						CFlurry.LogEvent("DCP_CONFIGS_END", dictionary);
					}
				}

				private static double GetSpentSeconds(DateTime dateTimeFrom)
				{
					double result = 0.0;
					DateTime utcNow = DateTime.UtcNow;
					if (dateTimeFrom <= utcNow)
					{
						result = (DateTime.UtcNow - dateTimeFrom).TotalSeconds;
					}
					return result;
				}

				private static int Round(double seconds)
				{
					int result = -1;
					try
					{
						result = Convert.ToInt32(seconds);
					}
					catch (Exception)
					{
					}
					return result;
				}

				private static string GetTimeInterval(double seconds)
				{
					string text = null;
					int[] array = new int[10] { 0, 2, 5, 10, 30, 60, 120, 180, 240, 300 };
					for (int i = 1; i < array.Length; i++)
					{
						if (seconds < (double)array[i])
						{
							text = string.Format("{0}:{1}", array[i - 1], array[i]);
							break;
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						text = string.Format("{0}+", array[array.Length - 1]);
					}
					return text;
				}

				private static string GetNetworkType()
				{
					string text = null;
					switch (Application.internetReachability)
					{
					case NetworkReachability.NotReachable:
						return "Offline";
					case NetworkReachability.ReachableViaCarrierDataNetwork:
						return "Cell";
					case NetworkReachability.ReachableViaLocalAreaNetwork:
						return "Wi-Fi";
					default:
						return Application.internetReachability.ToString();
					}
				}
			}

			private const string kGameObjectName = "Glu.DynamicContentPipeline.DynamicContent GameObject";

			private const double kForcedUpdateTimeOut = 5.0;

			private static Impl m_instance;

			private static Impl Instance
			{
				get
				{
					if (m_instance != null)
					{
						return m_instance;
					}
					GameObject gameObject = GameObject.Find("Glu.DynamicContentPipeline.DynamicContent GameObject");
					if (gameObject != null)
					{
						m_instance = gameObject.GetComponent<GluDynamicContentPipelineDynamicContentImpl>();
						return m_instance;
					}
					gameObject = new GameObject("Glu.DynamicContentPipeline.DynamicContent GameObject");
					gameObject.AddComponent<GluDynamicContentPipelineDynamicContentImpl>();
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					m_instance = gameObject.GetComponent<GluDynamicContentPipelineDynamicContentImpl>();
					m_instance.useGUILayout = false;
					m_instance.enabled = false;
					return m_instance;
				}
			}

			private static string BuildTag
			{
				get
				{
					return BuildInfo.buildTag;
				}
			}

			private static string Platform
			{
				get
				{
					string result = string.Empty;
					switch (Application.platform)
					{
					case RuntimePlatform.Android:
						result = "android";
						break;
					case RuntimePlatform.IPhonePlayer:
						result = "ios";
						break;
					case RuntimePlatform.OSXPlayer:
						result = "mac";
						break;
					case RuntimePlatform.WindowsPlayer:
						result = "win";
						break;
					}
					return result;
				}
			}

			private static string[] UrlsToForcedUpdate
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/ForcedUpdate.xml", m_baseUrls[i], Platform);
					}
					return array;
				}
			}

			private static string[] UrlsToABTestingDecisionTable
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/{2}/ABTesting.xml", m_baseUrls[i], Platform, BuildTag);
					}
					return array;
				}
			}

			private static string[] UrlsToDefaultAssetBundles
			{
				get
				{
					string[] array = new string[m_baseUrls.Length];
					for (int i = 0; i < m_baseUrls.Length; i++)
					{
						array[i] = string.Format("{0}/{1}/{2}/AssetBundles/{3}.version", m_baseUrls[i], Platform, BuildTag, AssetBundlesIndexName);
					}
					return array;
				}
			}

			public static void StartContentUpdate()
			{
				if (m_isLoadingInProgress)
				{
					m_isLoadingInProgress = false;
					Instance.StopCoroutine("DoContentUpdate");
				}
				if (m_isCheckingInProgress)
				{
					m_isCheckingInProgress = false;
					Instance.StopCoroutine("DoCheckForUpdates");
				}
				Reset(true);
				m_isLoadingInProgress = true;
				m_startedProgress = LastCheckReport.updateScope;
				m_currentProgress = m_startedProgress;
				Instance.StartCoroutine("DoContentUpdate");
			}

			public static bool IsUpdateInProgress()
			{
				return m_isLoadingInProgress;
			}

			public static void CheckForUpdates()
			{
				if (!m_isLoadingInProgress && !m_isCheckingInProgress)
				{
					Reset(false);
					m_isCheckingInProgress = true;
					Instance.StartCoroutine("DoCheckForUpdates");
				}
			}

			private static void Reset(bool resetAll)
			{
				if (resetAll)
				{
					if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting)
					{
						m_resolution = null;
					}
					if (LastCheckReport.updateScope <= CheckReport.UpdateScope.AssetBundles)
					{
						m_isIndexInfo = false;
					}
				}
				m_updateInfo = null;
			}

			private IEnumerator DoContentUpdate()
			{
				FlurryEvents.LogDCPStarted();
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ForcedUpdate)
				{
					m_currentProgress = CheckReport.UpdateScope.ForcedUpdate;
					FlurryEvents.LogForcedUpdateStarted();
					ForcedUpdate.Init(UrlsToForcedUpdate[0], BuildTag);
					ForcedUpdate.CheckUpdateStatus();
					DateTime dateTime0 = DateTime.UtcNow;
					bool firedTimeout = false;
					while (ForcedUpdate.IsCheckInProgress())
					{
						if ((DateTime.UtcNow - dateTime0).TotalSeconds > 5.0)
						{
							firedTimeout = true;
							break;
						}
						yield return null;
					}
					FlurryEvents.LogForcedUpdateFinished(firedTimeout);
					if (ForcedUpdate.NeedToQuit())
					{
						LastCheckReport = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
						if (OnForcedBinariesUpdate != null)
						{
							try
							{
								OnForcedBinariesUpdate();
							}
							catch (Exception ex)
							{
								Exception e3 = ex;
							}
						}
						m_isLoadingInProgress = false;
						yield break;
					}
					if (ForcedUpdate.NeedToUpdate() && OnAvailableBinariesUpdate != null)
					{
						try
						{
							if (OnAvailableBinariesUpdate())
							{
								LastCheckReport = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
								m_isLoadingInProgress = false;
								yield break;
							}
						}
						catch (Exception ex2)
						{
							Exception e4 = ex2;
						}
					}
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting)
				{
					m_currentProgress = CheckReport.UpdateScope.ABTesting;
					if (LastCheckReport.ChangedABTestingResolution == null)
					{
						Dictionary<string, string> initialVariantData = new Dictionary<string, string> { { "VariantId", "DefaultOffline" } };
						string str = null;
						string[] urlsToDefaultAssetBundles = UrlsToDefaultAssetBundles;
						foreach (string url in urlsToDefaultAssetBundles)
						{
							str = ((str == null) ? url : (str + ";" + url));
						}
						initialVariantData.Add("AssetBundlesUrls", str);
						FlurryEvents.LogABTestingStarted();
						m_resolution = Glu.ABTesting.Resolution.Retrieve(initialVariantData, UrlsToABTestingDecisionTable);
						while (!m_resolution.Ready)
						{
							yield return null;
						}
					}
					else
					{
						m_resolution = LastCheckReport.ChangedABTestingResolution;
					}
					FlurryEvents.LogABTestingFinished(m_resolution);
				}
				bool success = true;
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.AssetBundles)
				{
					m_currentProgress = CheckReport.UpdateScope.AssetBundles;
					if (m_indexInfo != null)
					{
						m_indexInfo.UnloadAll(true);
					}
					if (m_resolution.Data != null && m_resolution.Data.ContainsKey("AssetBundlesUrls"))
					{
						FlurryEvents.LogAssetBundlesStarted(m_resolution.Data["AssetBundlesUrls"]);
						m_indexInfo = Glu.AssetBundles.AssetBundles.DownloadAll(m_resolution.Data["AssetBundlesUrls"].Split(new string[3] { " ", ",", ";" }, StringSplitOptions.RemoveEmptyEntries));
						m_isIndexInfo = true;
						while (m_indexInfo.state == IndexInfo.State.InProgress)
						{
							yield return null;
						}
						FlurryEvents.LogAssetBundlesFinished(m_indexInfo);
						switch (m_indexInfo.state)
						{
						case IndexInfo.State.Failed:
							success = false;
							break;
						case IndexInfo.State.Succeeded:
							success = true;
							break;
						}
					}
					else
					{
						m_indexInfo = null;
						success = true;
					}
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.CustomDynamicContent && success)
				{
					m_currentProgress = CheckReport.UpdateScope.CustomDynamicContent;
					m_normalizedProgressForCustomDynamicContent = 0f;
					ICustomDynamicContent[] cdcsToUpdate = LastCheckReport.ChangedCustomDynamicContent ?? CustomDynamicContent.ToArray();
					if (cdcsToUpdate.Length > 0)
					{
						FlurryEvents.LogCustomContentStarted();
						float finished = 0f;
						ICustomDynamicContent[] array = cdcsToUpdate;
						foreach (ICustomDynamicContent customDynamicContent in array)
						{
							if (customDynamicContent.IsInProgress)
							{
								success = false;
								foreach (ICustomDynamicContent temp in CustomDynamicContent)
								{
									temp.InvalidateLastCheckReport();
								}
								break;
							}
							customDynamicContent.StartContentUpdate(m_resolution);
							while (customDynamicContent.IsInProgress)
							{
								m_normalizedProgressForCustomDynamicContent = (finished + customDynamicContent.Progress) / (float)cdcsToUpdate.Length;
								yield return null;
							}
							success &= customDynamicContent.Result;
							finished += ((!success) ? 0f : 1f);
						}
						FlurryEvents.LogCustomContentFinished(success, cdcsToUpdate);
					}
					else
					{
						success = true;
					}
					m_normalizedProgressForCustomDynamicContent = 1f;
				}
				if (LastCheckReport.updateScope <= CheckReport.UpdateScope.ABTesting && success)
				{
					success = m_resolution.Commit();
				}
				FlurryEvents.LogDCPFinished(success);
				if (success)
				{
					m_currentProgress = CheckReport.UpdateScope.Nothing;
					LastCheckReport = new CheckReport(CheckReport.UpdateScope.Nothing);
					if (OnSuccess != null)
					{
						try
						{
							OnSuccess();
						}
						catch (Exception ex3)
						{
							Exception e2 = ex3;
						}
					}
				}
				else if (OnFail != null)
				{
					try
					{
						OnFail();
					}
					catch (Exception ex4)
					{
						Exception e = ex4;
					}
				}
				m_isLoadingInProgress = false;
			}

			private IEnumerator DoCheckForUpdates()
			{
				yield return null;
				CheckReport report = new CheckReport(CheckReport.UpdateScope.Nothing);
				Glu.ABTesting.Resolution resolution2 = null;
				List<ICustomDynamicContent> cdcsToUpdate = new List<ICustomDynamicContent>();
				if (m_isInitialised)
				{
					ForcedUpdate.Init(UrlsToForcedUpdate[0], BuildTag);
					ForcedUpdate.CheckUpdateStatus();
					DateTime dateTime0 = DateTime.UtcNow;
					while (ForcedUpdate.IsCheckInProgress() && !((DateTime.UtcNow - dateTime0).TotalSeconds > 5.0))
					{
						yield return null;
					}
					if (ForcedUpdate.NeedToQuit())
					{
						report = new CheckReport(CheckReport.UpdateScope.ForcedUpdate);
					}
					else
					{
						resolution2 = Glu.ABTesting.Resolution.Retrieve(null, UrlsToABTestingDecisionTable);
						while (!resolution2.Ready)
						{
							yield return null;
						}
						if (!resolution2.IsApplied)
						{
							report = new CheckReport(CheckReport.UpdateScope.ABTesting, resolution2);
						}
						else
						{
							if (m_indexInfo != null)
							{
								m_updateInfo = m_indexInfo.CheckForUpdates();
								while (m_updateInfo.state == UpdateInfo.State.Pending)
								{
									yield return null;
								}
								if (m_updateInfo.state == UpdateInfo.State.Outdated)
								{
									report = new CheckReport(CheckReport.UpdateScope.AssetBundles);
								}
							}
							if (report.updateScope == CheckReport.UpdateScope.Nothing && CustomDynamicContent != null && CustomDynamicContent.Count > 0)
							{
								foreach (ICustomDynamicContent customDynamicContent2 in CustomDynamicContent)
								{
									if (customDynamicContent2.IsInProgress)
									{
										cdcsToUpdate.Clear();
										foreach (ICustomDynamicContent temp in CustomDynamicContent)
										{
											temp.InvalidateLastCheckReport();
										}
										break;
									}
									customDynamicContent2.CheckForUpdates(resolution2);
									while (customDynamicContent2.IsInProgress)
									{
										yield return null;
									}
									if (customDynamicContent2.Result)
									{
										cdcsToUpdate.Add(customDynamicContent2);
									}
								}
								if (cdcsToUpdate.Count > 0)
								{
									report = new CheckReport(CheckReport.UpdateScope.CustomDynamicContent, cdcsToUpdate.ToArray());
								}
							}
						}
					}
					if (report.updateScope < CheckReport.UpdateScope.CustomDynamicContent)
					{
						foreach (ICustomDynamicContent customDynamicContent in CustomDynamicContent)
						{
							customDynamicContent.InvalidateLastCheckReport();
						}
					}
				}
				LastCheckReport = report;
				yield return new WaitForSeconds(TimeInterval);
				m_isCheckingInProgress = false;
			}
		}

		public delegate void OnVoidEvent();

		public delegate bool OnBoolEvent();

		private static Version m_version;

		private static int m_timeInterval;

		private static string m_assetBundlesIndexName;

		private static List<ICustomDynamicContent> m_customDynamicContent;

		private static string[] m_baseUrls;

		private static bool m_isInitialised;

		private static Dictionary<CheckReport.UpdateScope, float> m_progressMeterSplitting;

		private static CheckReport.UpdateScope m_startedProgress;

		private static CheckReport.UpdateScope m_currentProgress;

		private static float m_normalizedProgressForCustomDynamicContent;

		private static bool m_isLoadingInProgress;

		private static bool m_isCheckingInProgress;

		private static Glu.ABTesting.Resolution m_resolution;

		private static bool m_isIndexInfo;

		private static IndexInfo m_indexInfo;

		private static UpdateInfo m_updateInfo;

		public static Version version
		{
			get
			{
				return m_version ?? (m_version = new Version(1, 1, 7));
			}
		}

		public static int ThreadCount
		{
			set
			{
				DownloadManager.ThreadCount = value;
				CustomConfigs.ThreadCount = value;
			}
		}

		public static int TimeInterval
		{
			get
			{
				return m_timeInterval;
			}
			set
			{
				m_timeInterval = value;
			}
		}

		public static string AssetBundlesIndexName
		{
			get
			{
				return m_assetBundlesIndexName;
			}
			set
			{
				m_assetBundlesIndexName = value;
			}
		}

		public static Dictionary<CheckReport.UpdateScope, float> ProgressMeterSplitting
		{
			get
			{
				return m_progressMeterSplitting;
			}
		}

		public static float Progress
		{
			get
			{
				Array values = Enum.GetValues(typeof(CheckReport.UpdateScope));
				CheckReport.UpdateScope key = (CheckReport.UpdateScope)(int)values.GetValue(0);
				CheckReport.UpdateScope updateScope = (CheckReport.UpdateScope)(int)values.GetValue(values.Length - 1);
				float num = 0f;
				switch (m_currentProgress)
				{
				case CheckReport.UpdateScope.AssetBundles:
					if (m_isIndexInfo && m_indexInfo != null && m_indexInfo.downloadInfo != null && m_indexInfo.downloadInfo.state == DownloadManager.Info.State.Downloaded)
					{
						AssetBundleInfo[] assetBundleInfo = m_indexInfo.assetBundleInfo;
						long num2 = 0L;
						float num3 = 0f;
						AssetBundleInfo[] array = assetBundleInfo;
						foreach (AssetBundleInfo assetBundleInfo2 in array)
						{
							num2 += assetBundleInfo2.Size;
							num3 += assetBundleInfo2.downloadInfo.ReceivingProgress * (float)assetBundleInfo2.Size;
						}
						num = ((num2 <= 0) ? 0f : (num3 / (float)num2));
					}
					break;
				case CheckReport.UpdateScope.CustomDynamicContent:
					num = m_normalizedProgressForCustomDynamicContent;
					break;
				}
				float num4 = ProgressMeterSplitting[m_currentProgress];
				if (m_currentProgress < updateScope)
				{
					float num5 = ProgressMeterSplitting[m_currentProgress];
					float num6 = ProgressMeterSplitting[m_currentProgress + 1];
					num4 = num5 + num * (num6 - num5);
				}
				float num7 = 1f;
				if (m_startedProgress < updateScope)
				{
					float num8 = ProgressMeterSplitting[m_startedProgress];
					float num9 = ProgressMeterSplitting[updateScope];
					num7 = ((!(num9 - num8 > float.MinValue)) ? num7 : ((num4 - num8) / (num9 - num8)));
				}
				float num10 = ProgressMeterSplitting[key];
				float num11 = ProgressMeterSplitting[updateScope];
				return num10 + num7 * (num11 - num10);
			}
		}

		public static IndexInfo AssetBundlesIndexInfo
		{
			get
			{
				return (!m_isIndexInfo) ? null : m_indexInfo;
			}
		}

		public static Glu.ABTesting.Resolution ABTestingResolution
		{
			get
			{
				return m_resolution;
			}
		}

		public static OnVoidEvent OnForcedBinariesUpdate { get; set; }

		public static OnBoolEvent OnAvailableBinariesUpdate { get; set; }

		public static OnVoidEvent OnFail { get; set; }

		public static OnVoidEvent OnSuccess { get; set; }

		public static List<ICustomDynamicContent> CustomDynamicContent
		{
			get
			{
				return m_customDynamicContent;
			}
		}

		public static CheckReport LastCheckReport { get; private set; }

		static DynamicContent()
		{
			m_version = null;
			m_timeInterval = 300;
			m_assetBundlesIndexName = "index";
			m_customDynamicContent = new List<ICustomDynamicContent>();
			m_baseUrls = null;
			m_isInitialised = false;
			m_progressMeterSplitting = new Dictionary<CheckReport.UpdateScope, float>();
			m_isLoadingInProgress = false;
			m_isCheckingInProgress = false;
			m_resolution = null;
			m_isIndexInfo = false;
			m_indexInfo = null;
			m_updateInfo = null;
			LastCheckReport = new CheckReport(CheckReport.UpdateScope.Nothing);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.All, 0f);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.ForcedUpdate, 0f);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.ABTesting, 0.05f);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.AssetBundles, 0.1f);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.CustomDynamicContent, 0.9f);
			m_progressMeterSplitting.Add(CheckReport.UpdateScope.Nothing, 1f);
		}

		public static void Init(params string[] baseUrls)
		{
			if (baseUrls != null && baseUrls.Length > 0)
			{
				for (int i = 0; i < baseUrls.Length; i++)
				{
					while (baseUrls[i].EndsWith("/"))
					{
						baseUrls[i] = baseUrls[i].Remove(baseUrls[i].Length - 1);
					}
				}
				m_baseUrls = baseUrls;
			}
			else
			{
				m_baseUrls = new string[0];
			}
			if (!m_isInitialised)
			{
				m_isInitialised = m_baseUrls.Length > 0;
				if (m_isInitialised)
				{
					LastCheckReport = new CheckReport(CheckReport.UpdateScope.All);
				}
			}
		}

		public static void StartContentUpdate()
		{
			Impl.StartContentUpdate();
		}

		public static bool IsUpdateInProgress()
		{
			return Impl.IsUpdateInProgress();
		}

		public static bool CheckForUpdates()
		{
			Impl.CheckForUpdates();
			return LastCheckReport.updateScope < CheckReport.UpdateScope.Nothing;
		}
	}
}
