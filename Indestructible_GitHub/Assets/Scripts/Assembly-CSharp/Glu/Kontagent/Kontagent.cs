using System.Collections.Generic;

namespace Glu.Kontagent
{
	public static class Kontagent
	{
		private static bool sIsTrusted = true;

		public static void StartSession(string apiKey)
		{
			sIsTrusted = !AJavaTools.DeviceInfo.IsDeviceRooted();
			AStats.Kontagent.StartSession();
		}

		public static void StopSession()
		{
			AStats.Kontagent.EndSession();
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val)
		{
			LogEvent(name, st1, st2, st3, level, val, null);
		}

		public static void LogEvent(string name, string st1, string st2, string st3, int? level, int? val, Dictionary<string, string> data)
		{
			List<string> list = new List<string>();
			string text = ((!sIsTrusted) ? "ZJB_" : string.Empty);
			list.Add("st1");
			list.Add(text + st1);
			if (st2 != null && st2.Length > 0)
			{
				list.Add("st2");
				list.Add(st2);
			}
			if (st3 != null && st3.Length > 0)
			{
				list.Add("st3");
				list.Add(st3);
			}
			if (level.HasValue && level.Value.ToString().Length > 0)
			{
				list.Add("l");
				list.Add(level.Value.ToString());
			}
			if (val.HasValue && val.Value.ToString().Length > 0)
			{
				list.Add("v");
				list.Add(val.Value.ToString());
			}
			string text2 = dictionaryToJSON(data);
			if (text2 != null && text2.Length > 0)
			{
				list.Add("data");
				list.Add(text2);
			}
			AStats.Kontagent.LogEvent(name, list.ToArray());
		}

		public static void RevenueTracking(int val)
		{
			AStats.Kontagent.RevenueTracking(val);
		}

		private static string dictionaryToJSON(Dictionary<string, string> dict)
		{
			if (dict == null)
			{
				return null;
			}
			string text = "{";
			foreach (KeyValuePair<string, string> item in dict)
			{
				if (text != "{")
				{
					text += ",";
				}
				text += "\"";
				text += item.Key;
				text += "\":\"";
				text += item.Value;
				text += "\"";
			}
			text += "}";
			if (text == "{}")
			{
				text = null;
			}
			return text;
		}
	}
}
