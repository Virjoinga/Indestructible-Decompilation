using System;
using Glu.Plugins.AMiscUtils;
using UnityEngine;

public class AndroidQualitySettings : MonoBehaviour
{
	private const string QualityDebugProperty = "quality";

	private string[] qualityDescription = new string[6] { "iPod4", "iPod4/iPhone4", "iPhone4", "iPad2", "iPad3/iPhone5", "Unknown" };

	private static AndroidQuality internalQuality = AndroidQuality.Unknown;

	private float lastInterval;

	private int frames;

	private float fps;

	private bool showDisplay;

	public static AndroidQuality Quality
	{
		get
		{
			if (internalQuality == AndroidQuality.Unknown)
			{
				GameObject target = new GameObject("AndroidQualitySettings", typeof(AndroidQualitySettings));
				UnityEngine.Object.DontDestroyOnLoad(target);
				Setup();
			}
			return internalQuality;
		}
	}

	public static void OverrideQualitySetting(AndroidQuality quality)
	{
		if (!Debug.isDebugBuild)
		{
			Debug.LogWarning("You can't override quality setting in release from code");
			return;
		}
		string value = null;
		if (quality != AndroidQuality.Unknown)
		{
			int num = (int)quality;
			value = num.ToString();
		}
		AJavaTools.DebugUtil.SetDebugProperty("quality", value);
		OverrideQualitySettingImpl(quality);
	}

	private static void Setup()
	{
		Debug.Log("******* AndroidQualitySettings **********");
		Debug.Log("*** gpu name: " + SystemInfo.graphicsDeviceName);
		Debug.Log("*** gpu memory: " + SystemInfo.graphicsMemorySize);
		Debug.Log("*** shader level: " + SystemInfo.graphicsShaderLevel);
		Debug.Log("*** cpu cores: " + SystemInfo.processorCount);
		Debug.Log("*** sys memory: " + SystemInfo.systemMemorySize);
		Debug.Log("*****************************************");
		AndroidQuality androidQuality = ReadOverrideQuality();
		if (androidQuality != AndroidQuality.Unknown)
		{
			OverrideQualitySettingImpl(androidQuality);
		}
		else if (SystemInfo.graphicsDeviceName.StartsWith("PowerVR"))
		{
			if (SystemInfo.processorCount == 1)
			{
				internalQuality = AndroidQuality.Tier_0;
			}
			else if (Screen.width > 1280)
			{
				if (SystemInfo.processorCount <= 2)
				{
					internalQuality = AndroidQuality.Tier_2;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_4;
				}
			}
			else if (Screen.width > 1024)
			{
				internalQuality = AndroidQuality.Tier_3;
			}
			else
			{
				internalQuality = AndroidQuality.Tier_2;
			}
		}
		else if (SystemInfo.graphicsDeviceName.StartsWith("Adreno"))
		{
			if (SystemInfo.processorCount == 1)
			{
				internalQuality = AndroidQuality.Tier_1;
			}
			else
			{
				internalQuality = AndroidQuality.Tier_2;
			}
		}
		else if (SystemInfo.graphicsDeviceName.StartsWith("Mali"))
		{
			if (SystemInfo.processorCount == 4 || SystemInfo.graphicsMemorySize >= 400)
			{
				internalQuality = AndroidQuality.Tier_4;
			}
			else if (SystemInfo.processorCount == 2)
			{
				if (Screen.width > 1024)
				{
					internalQuality = AndroidQuality.Tier_3;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_2;
				}
			}
		}
		else if (SystemInfo.graphicsDeviceName.StartsWith("NVIDIA Tegra") || SystemInfo.graphicsDeviceName.StartsWith("ULP GeForce"))
		{
			if (SystemInfo.processorCount == 4)
			{
				internalQuality = AndroidQuality.Tier_4;
			}
			else if (SystemInfo.processorCount == 2)
			{
				if (Screen.width > 1024)
				{
					internalQuality = AndroidQuality.Tier_1;
				}
				else
				{
					internalQuality = AndroidQuality.Tier_2;
				}
			}
		}
		else
		{
			internalQuality = AndroidQuality.Tier_1;
		}
	}

	private static AndroidQuality ReadOverrideQuality()
	{
		string debugProperty = AJavaTools.DebugUtil.GetDebugProperty("quality");
		if (debugProperty == null)
		{
			return AndroidQuality.Unknown;
		}
		int result;
		if (!int.TryParse(debugProperty, out result))
		{
			Debug.LogWarning("Failed to parse quality setting: {0}".Fmt(debugProperty));
			return AndroidQuality.Unknown;
		}
		Array values = Enum.GetValues(typeof(AndroidQuality));
		foreach (int item in values)
		{
			if (item == result)
			{
				return (AndroidQuality)item;
			}
		}
		Debug.LogWarning("Unknown quality setting: {0}".Fmt(debugProperty));
		return AndroidQuality.Unknown;
	}

	private static void OverrideQualitySettingImpl(AndroidQuality quality)
	{
		Debug.LogWarning("Overriding AndroidQuality setting {0}".Fmt(quality));
		internalQuality = quality;
	}

	private void OnGUI()
	{
		if (Debug.isDebugBuild && showDisplay)
		{
			GUILayout.Label(string.Concat("\nGPU name: ", SystemInfo.graphicsDeviceName, "\nGPU memory: ", SystemInfo.graphicsMemorySize, "\nShader level: ", SystemInfo.graphicsShaderLevel, "\nCPU cores: ", SystemInfo.processorCount, "\nSys memory: ", SystemInfo.systemMemorySize, "\nSceenSize: ", Screen.width, "x", Screen.height, "\nQuality: ", internalQuality, " - ", qualityDescription[(int)internalQuality], "\nFPS: ", fps.ToString("f2")));
		}
	}

	private void Update()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + 0.5f)
		{
			if (Input.touchCount > 2)
			{
				showDisplay = !showDisplay;
			}
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
		}
	}
}
