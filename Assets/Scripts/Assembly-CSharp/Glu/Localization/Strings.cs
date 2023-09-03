using System;
using System.Collections.Generic;
using System.Diagnostics;
using Glu.Localization.Config;
using Glu.Localization.Internal;
using Glu.Localization.StreamGetters;
using Glu.Localization.Utils;
using UnityEngine;

namespace Glu.Localization
{
	public static class Strings
	{
		private static bool initialized;

		private static string locale;

		private static LocalizationManager manager;

		private static LocaleListenerManager managers;

		private static LocaleListenerManager listeners;

		public static string Locale
		{
			get
			{
				Initialize();
				return locale;
			}
			set
			{
				locale = value;
				Initialize();
				managers.NotifyListeners(locale);
				listeners.NotifyListeners(locale);
			}
		}

		public static LocalizationManager Manager
		{
			get
			{
				InitializeMainLocalizationManager();
				return manager;
			}
			set
			{
				manager = value;
			}
		}

		public static string GetString(string id)
		{
			InitializeMainLocalizationManager();
			return manager.GetString(id);
		}

		public static string GetParticularString(string context, string id)
		{
			InitializeMainLocalizationManager();
			return manager.GetParticularString(context, id);
		}

		public static string GetStringFormat(string id, params object[] args)
		{
			InitializeMainLocalizationManager();
			return string.Format(manager.GetString(id), args);
		}

		public static string GetParticularStringFormat(string context, string id, params object[] args)
		{
			InitializeMainLocalizationManager();
			return string.Format(manager.GetParticularString(context, id), args);
		}

		[Conditional("o_O")]
		public static void MarkString(string id)
		{
		}

		[Conditional("o_O")]
		public static void MarkParticularString(string context, string id)
		{
		}

		public static void Initialize()
		{
			if (!initialized)
			{
				if (string.IsNullOrEmpty(locale))
				{
					locale = DetectLocale();
				}
				managers = new LocaleListenerManager();
				listeners = new LocaleListenerManager();
				initialized = true;
			}
		}

		public static void Destroy()
		{
			manager = null;
			locale = null;
			listeners = null;
			managers = null;
			initialized = false;
			RuntimeConfigManager.ReplaceConfig(null);
		}

		public static void RegisterLocaleListener(ILocaleListener listener)
		{
			Initialize();
			LocaleListenerManager localeListenerManager = ((!(listener is LocalizationManager)) ? listeners : managers);
			localeListenerManager.RegisterListener(listener);
		}

		public static void UnregisterLocaleListener(ILocaleListener listener)
		{
			Initialize();
			LocaleListenerManager localeListenerManager = ((!(listener is LocalizationManager)) ? listeners : managers);
			localeListenerManager.UnregisterListener(listener);
		}

		private static void InitializeMainLocalizationManager()
		{
			if (manager == null)
			{
				Initialize();
				RuntimeConfig instance = RuntimeConfigManager.Instance;
				LocalizationManager localizationManager = new LocalizationManager();
				localizationManager.StreamGetter = CreateGroupStreamGetter(instance.Locations);
				localizationManager.HandleLocaleChanged(locale);
				RegisterLocaleListener(localizationManager);
				manager = localizationManager;
			}
		}

		private static IStreamGetter CreateGroupStreamGetter(IList<PoLocation> locations)
		{
			GroupStreamGetter groupStreamGetter = new GroupStreamGetter();
			int count = locations.Count;
			for (int i = 0; i < count; i++)
			{
				PoLocation location = locations[i];
				IStreamGetter streamGetter = CreateStreamGetter(location);
				groupStreamGetter.AddChild(streamGetter);
			}
			return groupStreamGetter;
		}

		private static IStreamGetter CreateStreamGetter(PoLocation location)
		{
			string baseDirectory = location.BaseDirectory;
			switch (location.Source)
			{
			case ResourceSource.StreamingAssets:
			{
				string dataPath = Application.dataPath;
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					baseDirectory = dataPath + "/Raw";
				}
				if (Application.platform == RuntimePlatform.Android)
				{
					baseDirectory = string.Format("jar:file://{0}!/assets", dataPath);
					throw new NotSupportedException("Have to use WWW in stream getter");
				}
				baseDirectory = dataPath + "/StreamingAssets";
				FileStreamGetter fileStreamGetter = new FileStreamGetter();
				fileStreamGetter.BaseDir = baseDirectory;
				return fileStreamGetter;
			}
			case ResourceSource.AssetBundles:
			{
				AssetBundlesStreamGetter assetBundlesStreamGetter = new AssetBundlesStreamGetter();
				assetBundlesStreamGetter.BaseDir = baseDirectory;
				return assetBundlesStreamGetter;
			}
			case ResourceSource.Resources:
			{
				ResourcesStreamGetter resourcesStreamGetter = new ResourcesStreamGetter();
				resourcesStreamGetter.BaseDir = baseDirectory;
				return resourcesStreamGetter;
			}
			default:
				throw new NotSupportedException(string.Format("Not supported source {0}", location.Source));
			}
		}

		private static string DetectLocale()
		{
			string text = LanguageTable.GetIso639FromLanguage(Application.systemLanguage);
			if (text == "zh")
			{
				string deviceLocale = AJavaTools.DeviceInfo.GetDeviceLocale();
				if (deviceLocale == "zhcn")
				{
					text = "zh-Hans";
				}
				else if (deviceLocale == "zhtw")
				{
					text = "zh-Hant";
				}
			}
			return text;
		}
	}
}
