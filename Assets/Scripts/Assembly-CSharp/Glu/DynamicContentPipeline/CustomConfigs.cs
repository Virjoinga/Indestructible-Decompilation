using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Serialization;
using Glu.ABTesting;
using Glu.AssetBundles;
using UnityEngine;

namespace Glu.DynamicContentPipeline
{
	public class CustomConfigs : ICustomDynamicContent
	{
		public class CheckReport
		{
			public class FileInfo
			{
				public enum Source
				{
					None = 0,
					Online = 1,
					Cache = 2
				}

				public enum State
				{
					Unknown = 0,
					Outdated = 1,
					UpToDate = 2
				}

				public string Filename { get; private set; }

				public string ETag { get; private set; }

				public Source source { get; private set; }

				public State state { get; private set; }

				public FileInfo(string filename, string eTag, Source source, State state)
				{
					Filename = filename;
					ETag = eTag;
					this.source = source;
					this.state = state;
				}
			}

			public FileInfo[] Files { get; protected set; }

			protected CheckReport()
			{
				Files = new FileInfo[0];
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs");
			}
		}

		private class Request
		{
			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Request");
				}
			}

			private static List<Request> m_requests = new List<Request>();

			public static Request[] EntireList
			{
				get
				{
					return m_requests.ToArray();
				}
			}

			public string Filename { get; private set; }

			public string ABTestingKey { get; private set; }

			public string[] Urls { get; private set; }

			public string LocalFilename { get; private set; }

			public bool IsDefaultFilename { get; private set; }

			public string DefaultFilename { get; private set; }

			public WWW www { get; set; }

			private Request(string filename, string abTestingKey, string[] urls, string localFilename, bool isDefaultFilename, string defaultFilename)
			{
				Filename = filename;
				ABTestingKey = abTestingKey;
				Urls = urls;
				LocalFilename = localFilename;
				IsDefaultFilename = isDefaultFilename;
				DefaultFilename = defaultFilename;
				www = null;
			}

			public static bool Create(string filename, string abTestingKey, string[] urls, string localFilename, bool isDefaultFilename, string defaultFilename)
			{
				bool flag = !string.IsNullOrEmpty(localFilename);
				foreach (Request request in m_requests)
				{
					if (!string.IsNullOrEmpty(request.LocalFilename) && !string.IsNullOrEmpty(localFilename) && request.LocalFilename.Equals(localFilename))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Request item = new Request(filename, abTestingKey, urls, localFilename, isDefaultFilename, defaultFilename);
					m_requests.Add(item);
				}
				AddObserverToCustomUpdate();
				return flag;
			}

			public static void ResetAll()
			{
				foreach (Request request in m_requests)
				{
					request.www = null;
				}
			}

			public static void RemoveAll()
			{
				m_requests.Clear();
				RemoveObserverFromCustomUpdate();
			}

			private static void AddObserverToCustomUpdate()
			{
				if (DynamicContent.CustomDynamicContent == null)
				{
					return;
				}
				bool flag = false;
				foreach (ICustomDynamicContent item in DynamicContent.CustomDynamicContent)
				{
					if (item is CustomConfigs)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					DynamicContent.CustomDynamicContent.Add(new CustomConfigs());
				}
			}

			private static void RemoveObserverFromCustomUpdate()
			{
				if (DynamicContent.CustomDynamicContent == null || DynamicContent.CustomDynamicContent.Count <= 0)
				{
					return;
				}
				for (int num = DynamicContent.CustomDynamicContent.Count - 1; num >= 0; num--)
				{
					if (DynamicContent.CustomDynamicContent[num] is CustomConfigs)
					{
						DynamicContent.CustomDynamicContent.RemoveAt(num);
					}
				}
			}
		}

		public class Impl : MonoBehaviour, ICustomDynamicContent
		{
			private enum Task
			{
				None = 0,
				Update = 1,
				Check = 2
			}

			public static class AmazonResponseTester
			{
				private class Logger : LoggerSingleton<Logger>
				{
					public Logger()
					{
						LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl.AmazonResponseTester");
					}
				}

				[XmlType("Error")]
				public class AmazonError
				{
					private class Logger : LoggerSingleton<Logger>
					{
						public Logger()
						{
							LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl.AmazonResponseTester.AmazonError");
						}
					}

					[XmlAttribute("Code")]
					public string m_code;

					[XmlAttribute("Message")]
					public string m_message;

					[XmlAttribute("Key")]
					public string m_key;

					[XmlAttribute("RequestId")]
					public string m_requestId;

					[XmlAttribute("HostId")]
					public string m_hostId;

					private static XmlSerializer m_xmlSerializer;

					private static XmlSerializer xmlSerializer
					{
						get
						{
							if (m_xmlSerializer != null)
							{
								return m_xmlSerializer;
							}
							m_xmlSerializer = new XmlSerializer(typeof(AmazonError));
							return m_xmlSerializer;
						}
					}

					public AmazonError()
					{
						m_code = null;
						m_message = null;
						m_key = null;
						m_requestId = null;
						m_hostId = null;
					}

					public static AmazonError LoadInstance(Stream stream)
					{
						AmazonError amazonError = null;
						try
						{
							return xmlSerializer.Deserialize(stream) as AmazonError;
						}
						catch (Exception)
						{
							return null;
						}
					}
				}

				public static bool IsJunk(byte[] bytes)
				{
					bool result = false;
					try
					{
						using (Stream stream = new MemoryStream(bytes, false))
						{
							result = AmazonError.LoadInstance(stream) != null;
						}
					}
					catch (Exception)
					{
					}
					return result;
				}
			}

			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.DynamicContentPipeline.CustomConfigs.Impl");
				}
			}

			private class CheckReportWriter : CheckReport
			{
				private List<FileInfo> m_fileInfo;

				public CheckReportWriter()
				{
					m_fileInfo = new List<FileInfo>();
				}

				public void Add(FileInfo fileInfo)
				{
					m_fileInfo.Add(fileInfo);
					base.Files = m_fileInfo.ToArray();
				}
			}

			private class Source
			{
				private static MD5 m_md5;

				public byte[] Bytes { get; private set; }

				public string ETag { get; private set; }

				public Source InnerSource { get; private set; }

				private static MD5 md5
				{
					get
					{
						return m_md5 ?? (m_md5 = new MD5CryptoServiceProvider());
					}
				}

				protected Source(byte[] bytes)
					: this(bytes, (bytes == null) ? null : ComputeHash(bytes))
				{
				}

				protected Source(byte[] bytes, string eTag)
				{
					Bytes = bytes;
					ETag = StripETag(eTag);
					InnerSource = null;
				}

				protected Source(Source realSource)
					: this(realSource.Bytes, realSource.ETag)
				{
					InnerSource = realSource;
				}

				public bool BytesEqual(Source src)
				{
					bool result = false;
					byte[] bytes = Bytes;
					byte[] bytes2 = src.Bytes;
					if (bytes == null && bytes2 == null)
					{
						result = true;
					}
					if (bytes != null && bytes2 != null && bytes.Length == bytes2.Length)
					{
						result = true;
						for (int i = 0; i < bytes.Length; i++)
						{
							if (bytes[i] != bytes2[i])
							{
								result = false;
								break;
							}
						}
					}
					return result;
				}

				public bool ETagEquals(string eTag)
				{
					eTag = StripETag(eTag);
					return !string.IsNullOrEmpty(ETag) && !string.IsNullOrEmpty(eTag) && ETag.ToLower().Equals(eTag.ToLower());
				}

				private static string BytesToString(byte[] bytes)
				{
					string text = string.Empty;
					foreach (byte b in bytes)
					{
						text += b.ToString("x2").ToLower();
					}
					return text;
				}

				private static string ComputeHash(byte[] bytes)
				{
					return BytesToString(md5.ComputeHash(bytes));
				}

				private static string StripETag(string eTag)
				{
					if (!string.IsNullOrEmpty(eTag) && eTag.Contains("\""))
					{
						try
						{
							int num = eTag.IndexOf("\"") + 1;
							int num2 = eTag.LastIndexOf("\"");
							int length = num2 - num;
							eTag = eTag.Substring(num, length);
						}
						catch (Exception)
						{
						}
					}
					return eTag;
				}
			}

			private class Online : Source
			{
				public Online(byte[] bytes, string eTag)
					: base(bytes, eTag)
				{
				}

				public Online(Source realSource)
					: base(realSource)
				{
				}
			}

			private class Cache : Source
			{
				public Cache(byte[] bytes)
					: base(bytes)
				{
				}

				public Cache(byte[] bytes, string eTag)
					: base(bytes, eTag)
				{
				}
			}

			private class Default : Source
			{
				public Default(byte[] bytes)
					: base(bytes)
				{
				}
			}

			private class HttpHeadRequestData
			{
				private object m_locker = new object();

				private string m_url;

				private bool m_succeeded;

				private string m_eTag;

				public string Url
				{
					get
					{
						lock (m_locker)
						{
							return m_url;
						}
					}
					private set
					{
						lock (m_locker)
						{
							m_url = value;
						}
					}
				}

				public bool Succeeded
				{
					get
					{
						lock (m_locker)
						{
							return m_succeeded;
						}
					}
					set
					{
						lock (m_locker)
						{
							m_succeeded = value;
						}
					}
				}

				public string ETag
				{
					get
					{
						lock (m_locker)
						{
							return m_eTag;
						}
					}
					set
					{
						lock (m_locker)
						{
							m_eTag = value;
						}
					}
				}

				public HttpHeadRequestData(string url)
				{
					Url = url;
					Succeeded = false;
					ETag = null;
				}
			}

			private const string kGameObjectName = "Glu.DynamicContentPipeline.CustomConfigs GameObject";

			private static Impl m_instance;

			private static int m_threadCount = 1;

			private static Task m_task;

			private static Glu.ABTesting.Resolution m_resolution;

			private static CheckReportWriter m_report;

			private static bool m_finished = true;

			private static bool m_result;

			private static long m_lastUpdateSize;

			private static int m_processedFiles;

			private static int m_runCoroutinesCount;

			float ICustomDynamicContent.Progress
			{
				get
				{
					float num = 0f;
					if (((ICustomDynamicContent)this).IsInProgress && (m_task == Task.Update || m_task == Task.None))
					{
						if (Request.EntireList.Length > 0)
						{
							num += (float)m_processedFiles;
							Request[] entireList = Request.EntireList;
							foreach (Request request in entireList)
							{
								num += ((request.www == null) ? 0f : request.www.progress);
							}
							num /= (float)Request.EntireList.Length;
						}
						else
						{
							num = 1f;
						}
					}
					return num;
				}
			}

			bool ICustomDynamicContent.IsInProgress
			{
				get
				{
					return !m_finished;
				}
			}

			bool ICustomDynamicContent.Result
			{
				get
				{
					return m_result;
				}
			}

			long ICustomDynamicContent.LastUpdateSize
			{
				get
				{
					return m_lastUpdateSize;
				}
			}

			public static int ThreadCount
			{
				get
				{
					return m_threadCount;
				}
				set
				{
					m_threadCount = value;
				}
			}

			public CheckReport LastCheckReport { get; private set; }

			void ICustomDynamicContent.StartContentUpdate(Glu.ABTesting.Resolution resolution)
			{
				if (m_task == Task.None)
				{
					m_task = Task.Update;
					m_resolution = resolution;
					m_finished = false;
					m_result = true;
					m_lastUpdateSize = 0L;
					m_processedFiles = 0;
					StartCoroutine(ManageRequests());
				}
			}

			void ICustomDynamicContent.CheckForUpdates(Glu.ABTesting.Resolution resolution)
			{
				if (m_task == Task.None)
				{
					m_task = Task.Check;
					m_resolution = resolution;
					m_finished = false;
					m_result = false;
					m_lastUpdateSize = 0L;
					StartCoroutine(ManageRequests());
				}
			}

			void ICustomDynamicContent.InvalidateLastCheckReport()
			{
				LastCheckReport = null;
			}

			public static Impl GetInstance()
			{
				if (m_instance != null)
				{
					return m_instance;
				}
				GameObject gameObject = GameObject.Find("Glu.DynamicContentPipeline.CustomConfigs GameObject");
				if (gameObject != null)
				{
					m_instance = gameObject.GetComponent<GluDynamicContentPipelineCustomConfigsImpl>();
					return m_instance;
				}
				gameObject = new GameObject("Glu.DynamicContentPipeline.CustomConfigs GameObject");
				gameObject.AddComponent<GluDynamicContentPipelineCustomConfigsImpl>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				m_instance = gameObject.GetComponent<GluDynamicContentPipelineCustomConfigsImpl>();
				m_instance.useGUILayout = false;
				m_instance.enabled = false;
				return m_instance;
			}

			private IEnumerator ManageRequests()
			{
				m_report = new CheckReportWriter();
				m_runCoroutinesCount = 0;
				Request.ResetAll();
				Request[] requests = Request.EntireList;
				int idx = 0;
				bool warnedAboutDelay = false;
				while (idx < requests.Length || m_runCoroutinesCount > 0)
				{
					if (idx < requests.Length)
					{
						Request request = requests[idx];
						if (ThreadCount > m_runCoroutinesCount)
						{
							bool processRequest = (m_task == Task.Update && LastCheckReport == null) || m_task == Task.Check;
							if (m_task == Task.Update && LastCheckReport != null)
							{
								CheckReport.FileInfo[] files = LastCheckReport.Files;
								foreach (CheckReport.FileInfo fileInfo in files)
								{
									if (fileInfo.Filename.Equals(request.LocalFilename) && fileInfo.state == CheckReport.FileInfo.State.Outdated)
									{
										processRequest = true;
										break;
									}
								}
							}
							if (processRequest)
							{
								m_runCoroutinesCount++;
								StartCoroutine(DoProcess(request));
							}
							idx++;
							warnedAboutDelay = false;
						}
						else if (!warnedAboutDelay)
						{
							warnedAboutDelay = true;
						}
					}
					yield return null;
				}
				if ((m_task == Task.Update && m_result) || m_task == Task.Check)
				{
					LastCheckReport = m_report;
				}
				m_task = Task.None;
				m_resolution = null;
				m_report = null;
				m_finished = true;
			}

			private IEnumerator DoProcess(Request request)
			{
				try
				{
					Source source = null;
					Online online = null;
					Cache cache = null;
					Default @default = null;
					List<string> urls = new List<string>();
					if (!string.IsNullOrEmpty(request.Filename))
					{
						throw new NotImplementedException();
					}
					if (!string.IsNullOrEmpty(request.ABTestingKey))
					{
						if (m_resolution != null && m_resolution.Data != null && m_resolution.Data.ContainsKey(request.ABTestingKey))
						{
							urls.AddRange(m_resolution.Data[request.ABTestingKey].Split(new string[3] { " ", ",", ";" }, StringSplitOptions.RemoveEmptyEntries));
						}
					}
					else if (request.Urls != null && request.Urls.Length > 0)
					{
						urls.AddRange(request.Urls);
					}
					try
					{
						byte[] bytes = StorageManager.ReadFromLocation(request.LocalFilename);
						string eTag = null;
						try
						{
							using (StreamReader sr = new StreamReader(request.LocalFilename + ".version"))
							{
								eTag = sr.ReadLine();
							}
						}
						catch (Exception ex)
						{
							Exception e2 = ex;
						}
						if (bytes != null)
						{
							cache = (string.IsNullOrEmpty(eTag) ? new Cache(bytes) : new Cache(bytes, eTag));
						}
					}
					catch (Exception)
					{
					}
					if (cache == null && request.IsDefaultFilename)
					{
						TextAsset textAsset = null;
						if (string.IsNullOrEmpty(Path.GetExtension(request.DefaultFilename)))
						{
							textAsset = Resources.Load(request.DefaultFilename) as TextAsset;
						}
						if (textAsset == null && request.DefaultFilename.ToLower().StartsWith("assets"))
						{
							textAsset = Glu.AssetBundles.AssetBundles.Load(request.DefaultFilename) as TextAsset;
						}
						if (textAsset != null)
						{
							@default = new Default(textAsset.bytes);
						}
					}
					bool wasHttpHeadRequestSuccessful = false;
					if (!wasHttpHeadRequestSuccessful)
					{
						string eTag3 = null;
						if (urls.Count > 0)
						{
							for (int idx2 = 0; idx2 < urls.Count; idx2++)
							{
								HttpHeadRequestData headRequestData = new HttpHeadRequestData(SpecializeURL(urls[idx2]));
								Thread thread = new Thread(delegate(object obj)
								{
									try
									{
										HttpHeadRequestData httpHeadRequestData = (HttpHeadRequestData)obj;
										HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpHeadRequestData.Url);
										httpWebRequest.Method = "HEAD";
										try
										{
											using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
											{
												try
												{
													switch (httpWebResponse.StatusCode)
													{
													case HttpStatusCode.OK:
													case HttpStatusCode.Created:
													case HttpStatusCode.Accepted:
													case HttpStatusCode.NonAuthoritativeInformation:
													case HttpStatusCode.NoContent:
													case HttpStatusCode.ResetContent:
													case HttpStatusCode.PartialContent:
														httpHeadRequestData.Succeeded = true;
														break;
													default:
														httpHeadRequestData.Succeeded = false;
														break;
													}
													httpHeadRequestData.ETag = httpWebResponse.Headers[HttpResponseHeader.ETag];
												}
												catch (Exception)
												{
												}
											}
										}
										catch (Exception)
										{
										}
									}
									catch (Exception)
									{
									}
								});
								thread.Name = string.Format("Package.DynamicContentPipeline.CustomConfigs.Impl.DoProcess #{0}", Guid.NewGuid().ToString());
								thread.IsBackground = true;
								thread.Start(headRequestData);
								bool firedTimeout = false;
								DateTime dateTime = DateTime.UtcNow;
								while (thread.IsAlive)
								{
									if ((DateTime.UtcNow - dateTime).TotalSeconds > 5.0)
									{
										firedTimeout = true;
										break;
									}
									yield return null;
								}
								if (!firedTimeout)
								{
									wasHttpHeadRequestSuccessful = headRequestData.Succeeded;
									if (wasHttpHeadRequestSuccessful)
									{
										eTag3 = headRequestData.ETag;
									}
								}
								if (!string.IsNullOrEmpty(eTag3))
								{
									break;
								}
								yield return null;
							}
						}
						if (wasHttpHeadRequestSuccessful && !string.IsNullOrEmpty(eTag3))
						{
							if (cache != null && cache.ETagEquals(eTag3))
							{
								source = new Online(cache);
							}
							else if (@default != null && @default.ETagEquals(eTag3))
							{
								source = new Online(@default);
							}
							else
							{
								online = new Online(null, eTag3);
							}
						}
					}
					if (source == null && wasHttpHeadRequestSuccessful && urls.Count > 0)
					{
						for (int idx = 0; idx < urls.Count; idx++)
						{
							request.www = new WWW(SpecializeURL(urls[idx]));
							yield return StartCoroutine(WWWCoroutine(request.www));
							if (WWWSucceeded(request.www))
							{
								byte[] bytes2 = request.www.bytes;
								string eTag2 = null;
								try
								{
									eTag2 = request.www.responseHeaders["ETAG"];
								}
								catch (Exception ex2)
								{
									Exception e = ex2;
								}
								if (string.IsNullOrEmpty(eTag2) && online != null && !string.IsNullOrEmpty(online.ETag))
								{
									eTag2 = online.ETag;
								}
								online = new Online(bytes2, eTag2);
								source = online;
								request.www = null;
								break;
							}
							request.www = null;
						}
					}
					if (source == null && cache != null)
					{
						source = cache;
					}
					if (source == null && @default != null)
					{
						source = @default;
					}
					if (request.IsDefaultFilename)
					{
					}
					if (source != null && source is Online && source.InnerSource == null)
					{
						m_lastUpdateSize += source.Bytes.Length;
					}
					string fiETag = ((source == null) ? null : source.ETag);
					CheckReport.FileInfo.Source fiSource = ((source != null) ? ((source is Online) ? CheckReport.FileInfo.Source.Online : CheckReport.FileInfo.Source.Cache) : CheckReport.FileInfo.Source.None);
					CheckReport.FileInfo.State fiState = ((source != null && source is Online) ? ((source.InnerSource == null) ? CheckReport.FileInfo.State.Outdated : CheckReport.FileInfo.State.UpToDate) : CheckReport.FileInfo.State.Unknown);
					m_report.Add(new CheckReport.FileInfo(request.LocalFilename, fiETag, fiSource, fiState));
					if (source != null)
					{
						switch (m_task)
						{
						case Task.Update:
							DoContentUpdate(request, source);
							break;
						case Task.Check:
							DoCheckForUpdates(request, source);
							break;
						}
					}
					if (m_task == Task.Update)
					{
						m_processedFiles += (m_result ? 1 : 0);
					}
				}
				finally
				{
					m_runCoroutinesCount--;
				}
			}

			private void DoContentUpdate(Request request, Source source)
			{
				if (source != null)
				{
					bool flag = !File.Exists(request.LocalFilename);
					bool flag2 = !File.Exists(request.LocalFilename + ".version");
					if (source.InnerSource == null)
					{
						flag = true;
						flag2 = true;
					}
					else
					{
						flag |= !source.BytesEqual(source.InnerSource);
						flag2 |= !source.ETagEquals(source.InnerSource.ETag);
						source = source.InnerSource;
					}
					if (source is Cache)
					{
						return;
					}
					try
					{
						try
						{
							string directoryName = Path.GetDirectoryName(request.LocalFilename);
							if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
							{
								Directory.CreateDirectory(directoryName);
							}
						}
						catch (Exception)
						{
						}
						if (flag)
						{
							StorageManager.WriteToLocation(request.LocalFilename, request.LocalFilename, source.Bytes);
						}
						if (!flag2)
						{
							return;
						}
						try
						{
							string path = request.LocalFilename + ".version";
							if (!string.IsNullOrEmpty(source.ETag))
							{
								using (StreamWriter streamWriter = new StreamWriter(path))
								{
									streamWriter.WriteLine(source.ETag);
									return;
								}
							}
							if (File.Exists(path))
							{
								File.Delete(path);
							}
							return;
						}
						catch (Exception)
						{
							return;
						}
					}
					catch (Exception)
					{
						m_result = false;
						return;
					}
				}
				m_result = false;
			}

			private void DoCheckForUpdates(Request request, Source source)
			{
				if (source != null)
				{
					bool flag = true;
					if (source.InnerSource != null)
					{
						flag = !source.BytesEqual(source.InnerSource);
						source = source.InnerSource;
					}
					if (!(source is Cache) && flag)
					{
						m_result = true;
					}
				}
			}

			private string SpecializeURL(string url)
			{
				if (!url.ToLower().Contains("file://"))
				{
					bool flag = url.Contains("?");
					url += string.Format("{0}ref={1}", (!flag) ? "?" : "&", Guid.NewGuid().ToString("N"));
				}
				return url;
			}

			private IEnumerator WWWCoroutine(WWW www)
			{
				DateTime dateTime = DateTime.UtcNow;
				float progress = www.progress;
				while (!www.isDone && string.IsNullOrEmpty(www.error))
				{
					if (www.progress > progress)
					{
						dateTime = DateTime.UtcNow;
						progress = www.progress;
					}
					else if (www.progress < 1f && (DateTime.UtcNow - dateTime).TotalSeconds > 5.0)
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
					bool flag2 = false;
					try
					{
						Dictionary<string, string> responseHeaders = www.responseHeaders;
						string text = responseHeaders["STATUS"];
						string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (array[0].ToUpper().Contains("HTTP"))
						{
							int num = (int)Convert.ChangeType(array[1], typeof(int));
							flag = num < 400;
							flag2 = true;
						}
					}
					catch (Exception)
					{
					}
					if (!flag2)
					{
						bool flag3 = AmazonResponseTester.IsJunk(www.bytes);
						flag = !flag3;
						if (!flag3)
						{
						}
					}
				}
				return flag;
			}
		}

		float ICustomDynamicContent.Progress
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).Progress;
			}
		}

		bool ICustomDynamicContent.IsInProgress
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).IsInProgress;
			}
		}

		bool ICustomDynamicContent.Result
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).Result;
			}
		}

		long ICustomDynamicContent.LastUpdateSize
		{
			get
			{
				return ((ICustomDynamicContent)Impl.GetInstance()).LastUpdateSize;
			}
		}

		public static int ThreadCount
		{
			get
			{
				return Impl.ThreadCount;
			}
			set
			{
				Impl.ThreadCount = value;
			}
		}

		public CheckReport LastCheckReport
		{
			get
			{
				return Impl.GetInstance().LastCheckReport;
			}
		}

		private CustomConfigs()
		{
		}

		void ICustomDynamicContent.StartContentUpdate(Glu.ABTesting.Resolution resolution)
		{
			((ICustomDynamicContent)Impl.GetInstance()).StartContentUpdate(resolution);
		}

		void ICustomDynamicContent.CheckForUpdates(Glu.ABTesting.Resolution resolution)
		{
			((ICustomDynamicContent)Impl.GetInstance()).CheckForUpdates(resolution);
		}

		void ICustomDynamicContent.InvalidateLastCheckReport()
		{
			((ICustomDynamicContent)Impl.GetInstance()).InvalidateLastCheckReport();
		}

		public static bool RegisterABTestedDownloadableFile(string abTestingKey, string localFilename)
		{
			return Request.Create(null, abTestingKey, null, localFilename, false, null);
		}

		public static bool RegisterABTestedDownloadableFile(string abTestingKey, string localFilename, string defaultFilename)
		{
			return Request.Create(null, abTestingKey, null, localFilename, true, defaultFilename);
		}

		public static bool RegisterDownloadableFile(string[] urls, string localFilename)
		{
			return Request.Create(null, null, urls, localFilename, false, null);
		}

		public static bool RegisterDownloadableFile(string[] urls, string localFilename, string defaultFilename)
		{
			return Request.Create(null, null, urls, localFilename, true, defaultFilename);
		}

		public static void UnregisterAll()
		{
			Request.RemoveAll();
		}
	}
}
