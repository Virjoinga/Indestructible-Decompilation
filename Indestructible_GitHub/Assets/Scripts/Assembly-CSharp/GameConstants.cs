using UnityEngine;

public static class GameConstants
{
	public const string IDT_KONTAGENT_KEY = "";

	public const string IDT_PRIVACY_TERMS_ADDRESS = "http://www.glu.com/legal";

	private const string server_url_live = "http://gluservices.s3.amazonaws.com/PushNotifications/Indestructible";

	private const string server_url_stage = "http://griptonite.s3.amazonaws.com/indestructible/android/server_notifications/";

	private static string m_androidFilePath = string.Empty;

	private static string m_buildType = string.Empty;

	private static string m_buildTag = string.Empty;

	public static string AndroidFilePath
	{
		get
		{
			if (m_androidFilePath.Length == 0)
			{
				m_androidFilePath = AJavaTools.GameInfo.GetFilesPath();
			}
			return m_androidFilePath;
		}
	}

	public static string BuildType
	{
		get
		{
			if (m_buildType.Length == 0)
			{
				m_buildType = AJavaTools.Properties.GetBuildType();
			}
			return m_buildType;
		}
	}

	public static string BuildTag
	{
		get
		{
			if (m_buildTag.Length == 0)
			{
				m_buildTag = AJavaTools.Properties.GetBuildTag();
			}
			return m_buildTag;
		}
	}

	public static string NotificationURL
	{
		get
		{
			if (BuildType == "amazon")
			{
				if (Debug.isDebugBuild)
				{
					return "http://griptonite.s3.amazonaws.com/indestructible/android/server_notifications/amazon_sn-debug.txt";
				}
				return "http://gluservices.s3.amazonaws.com/PushNotifications/Indestructible_Amazon/notifications.txt";
			}
			if (Debug.isDebugBuild)
			{
				return "http://griptonite.s3.amazonaws.com/indestructible/android/server_notifications/sn-debug.txt";
			}
			return "http://gluservices.s3.amazonaws.com/PushNotifications/Indestructible/notifications.txt";
		}
	}

	public static string GameSku
	{
		get
		{
			if (BuildType == "amazon")
			{
				return "INDESTRUCTIBLE_AMAZON";
			}
			return "INDESTRUCTIBLE";
		}
	}

	public static string IDT_ASSET_BUNDLES_URL
	{
		get
		{
			if (Debug.isDebugBuild)
			{
				return "http://griptonite.s3.amazonaws.com/indestructible/assetbundles/stage/" + BuildTag + "/" + BuildType + "/" + AJavaTools.Properties.GetBuildLocale();
			}
			return "https://d2lk18ssnvgdhj.cloudfront.net/indestructible/assetbundles/live/" + BuildTag + "/" + BuildType + "/" + AJavaTools.Properties.GetBuildLocale();
		}
	}
}
