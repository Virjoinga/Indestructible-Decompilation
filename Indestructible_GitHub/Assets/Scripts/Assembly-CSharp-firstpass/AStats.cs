using System;
using System.Collections.Generic;
using UnityEngine;

public class AStats
{
	public class Flurry
	{
		public static void SetExtras(params string[] parameters)
		{
			Flurry_SetExtras(parameters);
		}

		public static void StartSession()
		{
			Flurry_StartSession(AJavaTools.Properties.GetFlurryKey());
		}

		public static void LogEvent(string eventId)
		{
			Flurry_LogEvent(eventId, false);
		}

		public static void LogEventTimed(string eventId)
		{
			Flurry_LogEvent(eventId, true);
		}

		public static void LogEvent(string eventId, string info)
		{
			LogEvent(eventId, "info", info);
		}

		public static void LogEventTimed(string eventId, string info)
		{
			LogEventTimed(eventId, "info", info);
		}

		public static void LogEvent(string eventId, params string[] parameters)
		{
			Flurry_LogEvent(eventId, parameters, false);
		}

		public static void LogEventTimed(string eventId, params string[] parameters)
		{
			Flurry_LogEvent(eventId, parameters, true);
		}

		public static void LogEvent(string eventId, Dictionary<string, string> dict)
		{
			Flurry_LogEvent(eventId, dict, false);
		}

		public static void LogEventTimed(string eventId, Dictionary<string, string> dict)
		{
			Flurry_LogEvent(eventId, dict, true);
		}

		public static void EndTimedEvent(string eventId)
		{
			Flurry_EndTimedEvent(eventId);
		}

		public static void EndSession()
		{
			Flurry_EndSession();
		}
	}

	public class Kontagent
	{
		public static void StartSession()
		{
			Kontagent_StartSession(AJavaTools.Properties.GetKontagentKey());
		}

		public static void LogEvent(string eventId)
		{
			Kontagent_LogEvent(eventId);
		}

		public static void LogEvent(string eventId, string info)
		{
			LogEvent(eventId, "info", info);
		}

		public static void LogEvent(string eventId, params string[] parameters)
		{
			Kontagent_LogEvent(eventId, parameters);
		}

		public static void LogEvent(string eventId, Dictionary<string, string> dict)
		{
			Kontagent_LogEvent(eventId, dict);
		}

		public static void RevenueTracking(int val)
		{
			Kontagent_RevenueTracking(val);
		}

		public static void RevenueTracking(int val, string info)
		{
			RevenueTracking(val, "info", info);
		}

		public static void RevenueTracking(int val, params string[] parameters)
		{
			Kontagent_RevenueTracking(val, parameters);
		}

		public static void RevenueTracking(int val, Dictionary<string, string> dict)
		{
			Kontagent_RevenueTracking(val, dict);
		}

		public static void EndSession()
		{
			Kontagent_EndSession();
		}
	}

	public class MobileAppTracking
	{
		public static void Init()
		{
			MAT_Init(AJavaTools.Properties.GetMobileAppTrackingPackage(), AJavaTools.Properties.GetMobileAppTrackingKey());
		}

		public static void TrackAction(string eventId, params string[] parameters)
		{
			MAT_TrackAction(eventId, parameters);
		}

		public static void RevenueTracking(string eventId, float price, string currency)
		{
			MAT_TrackAction(eventId, price, (!string.IsNullOrEmpty(currency)) ? "USD" : currency);
		}
	}

	public class GoogleAnalytics
	{
		public static void StartSession()
		{
			GA_StartSession(AJavaTools.Properties.GetGATrackingID());
		}

		public static void LogEvent(string category, string action, string label, long val = 0)
		{
			GA_SendEvent(category, action, label, val);
		}
	}

	private static AndroidJavaClass _astats;

	private static AndroidJavaClass _flurry;

	private static AndroidJavaClass _kontagent;

	private static AndroidJavaClass _mat;

	private static AndroidJavaClass _ga;

	public static AndroidJavaClass astats
	{
		get
		{
			if (_astats == null)
			{
				_astats = new AndroidJavaClass("com.glu.plugins.AStats");
			}
			return _astats;
		}
	}

	public static AndroidJavaClass flurry
	{
		get
		{
			if (_flurry == null)
			{
				_flurry = new AndroidJavaClass("com.glu.plugins.FlurryGlu");
			}
			return _flurry;
		}
	}

	public static AndroidJavaClass kontagent
	{
		get
		{
			if (_kontagent == null)
			{
				_kontagent = new AndroidJavaClass("com.glu.plugins.KontagentGlu");
			}
			return _kontagent;
		}
	}

	public static AndroidJavaClass mat
	{
		get
		{
			if (_mat == null)
			{
				_mat = new AndroidJavaClass("com.glu.plugins.MobileAppTrackerGlu");
			}
			return _mat;
		}
	}

	public static AndroidJavaClass GA
	{
		get
		{
			if (_ga == null)
			{
				_ga = new AndroidJavaClass("com.glu.plugins.GluGoogleAnalytics");
			}
			return _ga;
		}
	}

	public static void Init()
	{
		AStats_Init(Debug.isDebugBuild);
	}

	private static void AStats_Init(bool debug)
	{
		astats.CallStatic("Init", debug);
	}

	private static void Flurry_SetExtras(string[] parameters)
	{
		flurry.CallStatic("SetExtras", false, parameters);
	}

	private static void Flurry_StartSession(string key)
	{
		flurry.CallStatic("StartSession", key);
	}

	private static void Flurry_LogEvent(string eventId, bool timed)
	{
		flurry.CallStaticSafe("LogEvent", eventId, timed);
	}

	private static void Flurry_LogEvent(string eventId, string[] parameters, bool timed)
	{
		flurry.CallStaticSafe("LogEvent", eventId, parameters, timed);
	}

	private static void Flurry_LogEvent(string eventId, Dictionary<string, string> dict, bool timed)
	{
		using (AndroidJavaObject androidJavaObject = DictionaryToHashMap(dict))
		{
			flurry.CallStaticSafe("LogEvent", eventId, androidJavaObject, timed);
		}
	}

	private static void Flurry_EndTimedEvent(string eventId)
	{
		flurry.CallStatic("EndTimedEvent", eventId);
	}

	private static void Flurry_EndSession()
	{
		flurry.CallStatic("EndSession");
	}

	private static void Kontagent_StartSession(string key)
	{
		kontagent.CallStatic("StartSession", key);
	}

	private static void Kontagent_LogEvent(string eventId)
	{
		kontagent.CallStaticSafe("LogEvent", eventId);
	}

	private static void Kontagent_LogEvent(string eventId, string[] parameters)
	{
		kontagent.CallStaticSafe("LogEvent", eventId, parameters);
	}

	private static void Kontagent_LogEvent(string eventId, Dictionary<string, string> dict)
	{
		using (AndroidJavaObject androidJavaObject = DictionaryToHashMap(dict))
		{
			kontagent.CallStaticSafe("LogEvent", eventId, androidJavaObject);
		}
	}

	private static void Kontagent_RevenueTracking(int val)
	{
		kontagent.CallStatic("RevenueTracking", val);
	}

	private static void Kontagent_RevenueTracking(int val, string[] parameters)
	{
		kontagent.CallStatic("RevenueTracking", val, parameters);
	}

	private static void Kontagent_RevenueTracking(int val, Dictionary<string, string> dict)
	{
		using (AndroidJavaObject androidJavaObject = DictionaryToHashMap(dict))
		{
			kontagent.CallStatic("RevenueTracking", val, androidJavaObject);
		}
	}

	private static void Kontagent_EndSession()
	{
		kontagent.CallStatic("EndSession");
	}

	private static void MAT_Init(string packageName, string conversionKey)
	{
		mat.CallStatic("Init", packageName, conversionKey);
	}

	private static void MAT_TrackAction(string eventId, string[] parameters)
	{
		mat.CallStatic("TrackAction", eventId, parameters);
	}

	private static void MAT_TrackAction(string eventId, float price, string currency)
	{
		mat.CallStatic("TrackAction", eventId, price, currency);
	}

	private static AndroidJavaObject DictionaryToHashMap(Dictionary<string, string> dict)
	{
		//Discarded unreachable code: IL_00fd, IL_0111
		JniHelper.PushLocalFrame();
		AndroidJavaObject androidJavaObject = null;
		try
		{
			androidJavaObject = new AndroidJavaObject("java.util.HashMap");
			IntPtr methodID = AndroidJNI.GetMethodID(androidJavaObject.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
			object[] array = new object[2];
			foreach (KeyValuePair<string, string> item in dict)
			{
				JniHelper.PushLocalFrame();
				try
				{
					using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.String", item.Key))
					{
						using (AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.lang.String", item.Value))
						{
							array[0] = androidJavaObject2;
							array[1] = androidJavaObject3;
							AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(array));
						}
					}
				}
				finally
				{
					JniHelper.PopLocalFrame();
				}
			}
			return androidJavaObject;
		}
		catch (Exception)
		{
			if (androidJavaObject != null)
			{
				androidJavaObject.Dispose();
			}
			throw;
		}
		finally
		{
			JniHelper.PopLocalFrame();
		}
	}

	private static void GA_StartSession(string trackingID)
	{
		GA.CallStatic("StartSession", trackingID);
	}

	private static void GA_SendEvent(string category, string action, string label, long value)
	{
		GA.CallStatic("SendEvent", category, action, label, value);
	}
}
