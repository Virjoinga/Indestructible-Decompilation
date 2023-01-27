using System;
using System.IO;
using Glu.AssetMapping;
using Glu.Localization;
using Glu.UnityBuildSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
	public float WaitTime = 0.5f;

	private float _time;

	private void LocalizationCheck()
	{
		string path = Path.Combine(GameConstants.AndroidFilePath, "localization_decorate_strings.txt");
		if (File.Exists(path))
		{
			Strings.Manager.StringDecorator = (string ctxt, string id, string s) => "###";
		}
	}

	private void LoadLocaleMapping()
	{
		string format = "Localization/{0}-assets";
		string text = "en";
		string text2 = string.Format(format, text);
		AssetGroup group = (AssetGroup)Resources.Load(text2);
		AssetMapper.Instance.TagGroup(group);
		string text3 = Strings.Locale;
		string path = string.Format(format, text3);
		AssetGroup assetGroup = (AssetGroup)Resources.Load(path);
		if (assetGroup == null)
		{
			path = text2;
			text3 = text;
		}
		ResourcesGroupLoader groupLoader = new ResourcesGroupLoader(path);
		AssetMapper.Instance.AddGroup(groupLoader);
		Strings.Locale = text3;
	}

	private void Awake()
	{
		AJavaTools.Init();
		AJavaTools.Util.VerifySignature();
		AStats.Init();
		AStats.MobileAppTracking.Init();
		Screen.sleepTimeout = -1;
		if (AJavaTools.Util.IsFirstLaunchThisVersion())
		{
			try
			{
				string buildTag = BuildInfo.buildTag;
				string text = ((!string.IsNullOrEmpty(buildTag)) ? Path.Combine(Application.temporaryCachePath, buildTag) : Application.temporaryCachePath);
				if (Debug.isDebugBuild)
				{
					Debug.Log("Cache Path 1: " + text + ", " + Directory.Exists(text));
				}
				Directory.Delete(text, true);
				if (Debug.isDebugBuild)
				{
					Debug.Log("Cache Path 2: " + Directory.Exists(text));
				}
			}
			catch (Exception ex)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("The process failed: " + ex.Message);
				}
			}
		}
		if (AJavaTools.Properties.GetBuildLocale().ToLower() == "default" && !Debug.isDebugBuild && AJavaTools.Properties.IsBuildGoogle())
		{
			if (AJavaTools.DeviceInfo.GetDeviceLanguage() == "es" || AJavaTools.DeviceInfo.GetDeviceLanguage() == "fr" || AJavaTools.DeviceInfo.GetDeviceLanguage() == "de" || AJavaTools.DeviceInfo.GetDeviceLanguage() == "ru" || AJavaTools.DeviceInfo.GetDeviceLanguage() == "it")
			{
				Strings.Locale = AJavaTools.DeviceInfo.GetDeviceLanguage();
			}
			else
			{
				Strings.Locale = "en";
			}
		}
		LocalizationCheck();
		LoadLocaleMapping();
	}

	private void Update()
	{
		_time += Time.deltaTime;
		if (_time > WaitTime)
		{
			AJavaTools.UI.StartIndeterminateProgress(85);
			SceneManager.LoadSceneAsync("BootScene");
		}
	}
}
