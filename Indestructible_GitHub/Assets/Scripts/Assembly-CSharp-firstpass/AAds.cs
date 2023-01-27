using System;
using UnityEngine;

public class AAds : MonoBehaviour
{
	public class Tapjoy
	{
		public const string TJC_DISPLAY_AD_SIZE_320X50 = "320x50";

		public const string TJC_DISPLAY_AD_SIZE_640x100 = "640x100";

		public const string TJC_DISPLAY_AD_SIZE_768x90 = "768x90";

		public static bool isOfferWallOpen;

		public static void Init(string gameObjectName, string adSize = "640x100")
		{
			Tapjoy_Init(gameObjectName, AJavaTools.Properties.GetTapjoyAppID(), AJavaTools.Properties.GetTapjoySecretKey(), adSize);
			tapjoyGameObjectName = gameObjectName;
		}

		public static void SetPadding(int left, int top, int right, int bottom)
		{
			Tapjoy_SetPadding(left, top, right, bottom);
		}

		public static void Show(int width, int height, int gravity)
		{
			Tapjoy_Show(width, height, gravity);
		}

		public static void Hide()
		{
			Tapjoy_Hide();
		}

		public static bool IsAvailable()
		{
			return Tapjoy_IsAvailable();
		}

		public static void Launch(bool showFSA = true)
		{
			if (showFSA)
			{
				Tapjoy_ShowFullScreenAd();
			}
			isOfferWallOpen = true;
			Tapjoy_Launch();
		}

		public static void ShowFullScreenAd()
		{
			Tapjoy_ShowFullScreenAd();
		}

		public static void ShowDailyRewardAd()
		{
			Tapjoy_ShowDailyRewardAd();
		}

		public static void ActionComplete(string actionID)
		{
			Tapjoy_ActionComplete(actionID);
		}

		public static void GetPoints()
		{
			Tapjoy_GetPoints();
		}

		public static void ClearPoints()
		{
			Tapjoy_ClearPoints();
		}

		public static string GetUserID()
		{
			return Tapjoy_GetUserID();
		}

		public static void SetUserID(string userID)
		{
			Tapjoy_SetUserID(userID);
		}
	}

	public class Custom
	{
		public const int TYPEFACE_DEFAULT = 0;

		public const int TYPEFACE_DEFAULT_BOLD = 1;

		public const int TYPEFACE_MONOSPACE = 2;

		public const int TYPEFACE_SANS_SERIF = 3;

		public const int TYPEFACE_SERIF = 4;

		public const int STYLE_NORMAL = 0;

		public const int STYLE_BOLD = 1;

		public const int STYLE_ITALICS = 2;

		public const int COLOR_BLACK = -16777216;

		public const int COLOR_BLUE = -16776961;

		public const int COLOR_CYAN = -16711681;

		public const int COLOR_DKGRAY = -12303292;

		public const int COLOR_GRAY = -7829368;

		public const int COLOR_GREEN = -16711936;

		public const int COLOR_LTGRAY = -3355444;

		public const int COLOR_MAGENTA = -65281;

		public const int COLOR_RED = -65536;

		public const int COLOR_WHITE = -1;

		public const int COLOR_YELLOW = -256;

		public static void Init(string tag, string gameObjectName, string mainImage, string altImage = "")
		{
			Custom_Init(tag, gameObjectName, mainImage, altImage);
		}

		public static void SetPadding(string tag, int left, int top, int right, int bottom)
		{
			Custom_SetPadding(tag, left, top, right, bottom);
		}

		public static void Show(string tag, int width, int height, int gravity)
		{
			Custom_Show(tag, width, height, gravity);
		}

		public static void Hide(string tag)
		{
			Custom_Hide(tag);
		}

		public static void HideAll()
		{
			Custom_HideAll();
		}

		public static bool IsAvailable(string tag)
		{
			return Custom_IsAvailable(tag);
		}

		public static bool IsActive(string tag)
		{
			return Custom_IsActive(tag);
		}

		public static void SetText(string tag, string text, int typeface, int style, float size, int color)
		{
			Custom_SetText(tag, text, typeface, style, size, color);
		}

		public static void Attach(string tag, string toTag)
		{
			Custom_Attach(tag, toTag);
		}

		public static int GetSourceWidth(string tag)
		{
			return Custom_GetSourceWidth(tag);
		}

		public static int GetSourceHeight(string tag)
		{
			return Custom_GetSourceHeight(tag);
		}
	}

	public class PlayHaven
	{
		private const string moreGamesShownTimestampProperty = "PHBC";

		private const int maxBadgeCount = 9;

		public const string RESOLUTION_SUCCESS = "buy";

		public const string RESOLUTION_CANCEL = "cancel";

		public const string RESOLUTION_ERROR = "error";

		public static string phGameObjectName = string.Empty;

		public static void Init(string gameObjectName)
		{
			PlayHaven_Init(gameObjectName, AJavaTools.Properties.GetPlayHavenToken(), AJavaTools.Properties.GetPlayHavenSecret());
			phGameObjectName = gameObjectName;
		}

		public static void Show(string placement)
		{
			if (placement == "more_games")
			{
				string value = DateTime.UtcNow.ToString();
				PlayerPrefs.SetString("PHBC", value);
				PlayerPrefs.Save();
			}
			PlayHaven_Show(placement);
		}

		public static void ReportResolution(string sku, int quantity, string resolution)
		{
			PlayHaven_ReportResolution(sku, quantity, resolution);
		}

		public static int GetBadgeCount()
		{
			//Discarded unreachable code: IL_003d
			string @string = PlayerPrefs.GetString("PHBC");
			if (!string.IsNullOrEmpty(@string))
			{
				try
				{
					DateTime dateTime = Convert.ToDateTime(@string);
					return Math.Min((DateTime.UtcNow - dateTime).Days, 9);
				}
				catch (FormatException)
				{
				}
			}
			return 0;
		}
	}

	public class Amazon
	{
		public const string AD_SIZE_300x50 = "300x50";

		public const string AD_SIZE_300x250 = "300x250";

		public const string AD_SIZE_320x50 = "320x50";

		public const string AD_SIZE_600x90 = "600x90";

		public const string AD_SIZE_728x90 = "728x90";

		public const string AD_SIZE_1024x50 = "1024x50";

		public static void Init(string adSize = "600x90")
		{
			Amazon_Init(AJavaTools.Properties.GetAmazonAdAppID(), adSize);
		}

		public static void SetPadding(int left, int top, int right, int bottom)
		{
			Amazon_SetPadding(left, top, right, bottom);
		}

		public static void Show(int width, int height, int gravity)
		{
			Amazon_Show(width, height, gravity);
		}

		public static void Hide()
		{
			Amazon_Hide();
		}
	}

	public class BrandBoost
	{
		public static void Init(string gameObjectName, string partnerName, string siteName, string gameName)
		{
			BrandBoost_Init(gameObjectName, partnerName, siteName, gameName);
		}

		public static void Show(int gravity)
		{
			BrandBoost_Show(gravity);
		}

		public static void Hide()
		{
			BrandBoost_Hide();
		}

		public static bool IsAvailable()
		{
			return BrandBoost_IsAvailable();
		}

		public static void Launch()
		{
			BrandBoost_Launch();
		}
	}

	public class ChartBoost
	{
		public static void Init()
		{
			CB_Init(AJavaTools.Properties.GetChartBoostAppID(), AJavaTools.Properties.GetChartBoostAppSignature());
		}

		public static void LogEvent(string eventID)
		{
			CB_SendEvent(eventID);
		}

		public static void StartSession()
		{
			CB_StartSession();
		}

		internal static void OnDestroy()
		{
			CB_OnDestroy();
		}
	}

	public const int GRAVITY_BOTTOM = 80;

	public const int GRAVITY_CENTER = 17;

	public const int GRAVITY_CENTER_HORIZONTAL = 1;

	public const int GRAVITY_CENTER_VERTICAL = 16;

	public const int GRAVITY_LEFT = 3;

	public const int GRAVITY_RIGHT = 5;

	public const int GRAVITY_TOP = 48;

	private static string tapjoyGameObjectName;

	private static AndroidJavaClass _aads;

	private static AndroidJavaClass _tapjoy;

	private static AndroidJavaClass _custom;

	private static AndroidJavaClass _playhaven;

	private static AndroidJavaClass _amazon;

	private static AndroidJavaClass _brandboost;

	private static AndroidJavaClass _cb;

	public static AndroidJavaClass aads
	{
		get
		{
			if (_aads == null)
			{
				_aads = new AndroidJavaClass("com.glu.plugins.AAds");
			}
			return _aads;
		}
	}

	public static AndroidJavaClass tapjoy
	{
		get
		{
			if (_tapjoy == null)
			{
				_tapjoy = new AndroidJavaClass("com.glu.plugins.TapjoyGlu");
			}
			return _tapjoy;
		}
	}

	public static AndroidJavaClass custom
	{
		get
		{
			if (_custom == null)
			{
				_custom = new AndroidJavaClass("com.glu.plugins.CustomGlu");
			}
			return _custom;
		}
	}

	public static AndroidJavaClass playhaven
	{
		get
		{
			if (_playhaven == null)
			{
				_playhaven = new AndroidJavaClass("com.glu.plugins.PlayHavenGlu");
			}
			return _playhaven;
		}
	}

	public static AndroidJavaClass amazon
	{
		get
		{
			if (_amazon == null)
			{
				_amazon = new AndroidJavaClass("com.glu.plugins.AmazonGlu");
			}
			return _amazon;
		}
	}

	public static AndroidJavaClass brandboost
	{
		get
		{
			if (_brandboost == null)
			{
				_brandboost = new AndroidJavaClass("com.glu.plugins.BrandBoostGlu");
			}
			return _brandboost;
		}
	}

	public static AndroidJavaClass CB
	{
		get
		{
			if (_cb == null)
			{
				_cb = new AndroidJavaClass("com.glu.plugins.ChartBoostGlu");
			}
			return _cb;
		}
	}

	public static void Init(GameObject gameObject = null)
	{
		AAds_Init(Debug.isDebugBuild);
		if (gameObject == null)
		{
			gameObject = new GameObject(".AAds");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		gameObject.AddComponent<AAds>();
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			return;
		}
		if (Tapjoy.isOfferWallOpen)
		{
			Tapjoy.isOfferWallOpen = false;
			if (tapjoyGameObjectName != null)
			{
				GameObject gameObject = GameObject.Find(tapjoyGameObjectName);
				if (gameObject != null)
				{
					gameObject.SendMessage("onTapjoyOfferWallClosed", string.Empty);
				}
			}
		}
		else if (!string.IsNullOrEmpty(PlayHaven.phGameObjectName))
		{
			PlayHaven.Init(PlayHaven.phGameObjectName);
		}
	}

	private void OnDestroy()
	{
		ChartBoost.OnDestroy();
	}

	private static void AAds_Init(bool debug)
	{
		aads.CallStatic("Init", debug);
	}

	private static void Tapjoy_Init(string gameObjectName, string appID, string secretKey, string adSize)
	{
		tapjoy.CallStatic("Init", gameObjectName, appID, secretKey, adSize);
	}

	private static void Tapjoy_SetPadding(int left, int top, int right, int bottom)
	{
		tapjoy.CallStatic("SetPadding", left, top, right, bottom);
	}

	private static void Tapjoy_Show(int width, int height, int gravity)
	{
		tapjoy.CallStatic("Show", width, height, gravity);
	}

	private static void Tapjoy_Hide()
	{
		tapjoy.CallStatic("Hide");
	}

	private static bool Tapjoy_IsAvailable()
	{
		return tapjoy.CallStatic<bool>("IsAvailable", new object[0]);
	}

	private static void Tapjoy_Launch()
	{
		tapjoy.CallStatic("Launch");
	}

	private static void Tapjoy_ShowFullScreenAd()
	{
		tapjoy.CallStatic("ShowFullScreenAd");
	}

	private static void Tapjoy_ShowDailyRewardAd()
	{
		tapjoy.CallStatic("ShowDailyRewardAd");
	}

	private static void Tapjoy_ActionComplete(string actionID)
	{
		tapjoy.CallStatic("ActionComplete", actionID);
	}

	private static void Tapjoy_GetPoints()
	{
		tapjoy.CallStatic("GetPoints");
	}

	private static void Tapjoy_ClearPoints()
	{
		tapjoy.CallStatic("ClearPoints");
	}

	private static string Tapjoy_GetUserID()
	{
		return tapjoy.CallStatic<string>("GetUserID", new object[0]);
	}

	public static void Tapjoy_SetUserID(string userID)
	{
		tapjoy.CallStatic("SetUserID", userID);
	}

	private static void Custom_Init(string tag, string gameObjectName, string mainImage, string altImage)
	{
		custom.CallStatic("Init", tag, gameObjectName, mainImage, altImage);
	}

	private static void Custom_SetPadding(string tag, int left, int top, int right, int bottom)
	{
		custom.CallStatic("SetPadding", tag, left, top, right, bottom);
	}

	private static void Custom_Show(string tag, int width, int height, int gravity)
	{
		custom.CallStatic("Show", tag, width, height, gravity);
	}

	private static void Custom_Hide(string tag)
	{
		custom.CallStatic("Hide", tag);
	}

	private static void Custom_HideAll()
	{
		custom.CallStatic("HideAll");
	}

	private static bool Custom_IsAvailable(string tag)
	{
		return custom.CallStatic<bool>("IsAvailable", new object[1] { tag });
	}

	private static bool Custom_IsActive(string tag)
	{
		return custom.CallStatic<bool>("IsActive", new object[1] { tag });
	}

	private static void Custom_SetText(string tag, string text, int typeface, int style, float size, int color)
	{
		custom.CallStatic("SetText", tag, text, typeface, style, size, color);
	}

	private static void Custom_Attach(string tag, string toTag)
	{
		custom.CallStatic("Attach", tag, toTag);
	}

	private static int Custom_GetSourceWidth(string tag)
	{
		return custom.CallStatic<int>("GetSourceWidth", new object[1] { tag });
	}

	private static int Custom_GetSourceHeight(string tag)
	{
		return custom.CallStatic<int>("GetSourceHeight", new object[1] { tag });
	}

	private static void PlayHaven_Init(string gameObjectName, string token, string secret)
	{
		playhaven.CallStatic("Init", gameObjectName, token, secret);
	}

	private static void PlayHaven_Show(string placement)
	{
		playhaven.CallStatic("Show", placement);
	}

	private static void PlayHaven_ReportResolution(string sku, int quantity, string resolution)
	{
		playhaven.CallStatic("ReportResolution", sku, quantity, resolution);
	}

	private static void Amazon_Init(string appID, string adSize)
	{
		amazon.CallStatic("Init", appID, adSize);
	}

	private static void Amazon_SetPadding(int left, int top, int right, int bottom)
	{
		amazon.CallStatic("SetPadding", left, top, right, bottom);
	}

	private static void Amazon_Show(int width, int height, int gravity)
	{
		amazon.CallStatic("Show", width, height, gravity);
	}

	private static void Amazon_Hide()
	{
		amazon.CallStatic("Hide");
	}

	private static void BrandBoost_Init(string gameObjectName, string partnerName, string siteName, string gameName)
	{
		brandboost.CallStatic("Init", gameObjectName, partnerName, siteName, gameName);
	}

	private static void BrandBoost_Show(int gravity)
	{
		brandboost.CallStatic("Show", gravity);
	}

	private static void BrandBoost_Hide()
	{
		brandboost.CallStatic("Hide");
	}

	private static bool BrandBoost_IsAvailable()
	{
		return brandboost.CallStatic<bool>("IsAvailable", new object[0]);
	}

	private static void BrandBoost_Launch()
	{
		brandboost.CallStatic("Launch");
	}

	private static void CB_Init(string AppID, string AppSecret)
	{
		CB.CallStatic("Init", AppID, AppSecret);
	}

	private static void CB_StartSession()
	{
		CB.CallStatic("StartSession");
	}

	private static void CB_SendEvent(string eventID)
	{
		CB.CallStatic("SendEvent", eventID);
	}

	private static void CB_OnDestroy()
	{
		if (CB != null)
		{
			CB.CallStatic("OnDestroy");
		}
	}
}
