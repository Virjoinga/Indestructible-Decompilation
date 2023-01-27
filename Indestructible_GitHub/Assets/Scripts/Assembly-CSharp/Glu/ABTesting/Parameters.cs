using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Glu.ABTesting
{
	public class Parameters
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.Parameters");
			}
		}

		public const string PARAM_DATE_TIME = "__DateTime";

		public const string PARAM_RAW_ID = "__RawId";

		public const string PARAM_RANDOM_ID = "__RandomId";

		private string m_filename;

		private IList<KeyValuePair<string, object>> m_data;

		private int m_dataEditing;

		public object this[string key]
		{
			get
			{
				object val = null;
				return (!TryGetValue(key, out val)) ? null : val;
			}
			set
			{
				Set(key, value);
			}
		}

		public Parameters(params string[] filename)
		{
			m_filename = ((filename == null || filename.Length <= 0) ? null : filename[0]);
			m_data = new List<KeyValuePair<string, object>>();
			m_dataEditing = 0;
			Reset();
		}

		public void Set(string key, object val)
		{
			StartEditing();
			object val2 = null;
			if (TryGetValue(key, out val2))
			{
				for (int i = 0; i < m_data.Count; i++)
				{
					if (m_data[i].Key.Equals(key))
					{
						m_data.RemoveAt(i);
						break;
					}
				}
			}
			m_data.Add(new KeyValuePair<string, object>(key, val));
			StopEditing();
		}

		public bool TryGetValue(string key, out object val)
		{
			bool result = false;
			val = null;
			foreach (KeyValuePair<string, object> datum in m_data)
			{
				if (datum.Key.Equals(key))
				{
					val = datum.Value;
					return true;
				}
			}
			return result;
		}

		private void Reset()
		{
			m_data.Clear();
			object val = null;
			if (LoadFromFile() && TryGetValue("__DateTime", out val) && TryGetValue("__RawId", out val) && TryGetValue("__RandomId", out val))
			{
				return;
			}
			StartEditing();
			if (!TryGetValue("__DateTime", out val))
			{
				Set("__DateTime", DateTime.Now);
			}
			if (!TryGetValue("__RawId", out val))
			{
				if (TryGetValue("__RandomId", out val))
				{
					Set("__RawId", this["__RandomId"]);
				}
				else
				{
					Set("__RawId", new System.Random().Next(1, 101));
				}
			}
			if (!TryGetValue("__RandomId", out val))
			{
				Set("__RandomId", this["__RawId"]);
			}
			StopEditing();
		}

		private void StartEditing()
		{
			m_dataEditing++;
		}

		private void StopEditing()
		{
			m_dataEditing--;
			if (m_dataEditing <= 0)
			{
				SaveToFile();
			}
		}

		private bool LoadFromFile()
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(m_filename))
			{
				try
				{
					m_data = StorageManager.ReadXmlFromLocation(m_filename, m_data.GetType()) as List<KeyValuePair<string, object>>;
					flag = true;
				}
				catch (Exception)
				{
				}
				if (!flag)
				{
					string pathName = Path.Combine(Application.temporaryCachePath, Path.GetFileName(m_filename));
					try
					{
						m_data = StorageManager.ReadXmlFromLocation(pathName, m_data.GetType()) as List<KeyValuePair<string, object>>;
						return flag;
					}
					catch (Exception)
					{
						return flag;
					}
				}
			}
			return flag;
		}

		private bool SaveToFile()
		{
			bool result = false;
			if (!string.IsNullOrEmpty(m_filename))
			{
				try
				{
					string directoryName = Path.GetDirectoryName(m_filename);
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
					StorageManager.WriteXmlToLocation(m_filename, m_filename, m_data);
					result = true;
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
