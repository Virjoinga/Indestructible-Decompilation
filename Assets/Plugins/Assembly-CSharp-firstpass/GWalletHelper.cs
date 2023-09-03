using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GWHMiniJSON;
using UnityEngine;

public class GWalletHelper : MonoBehaviour
{
	private enum SUBPLAN
	{
		bronze = 0,
		silver = 1,
		gold = 2,
		platinum = 3
	}

	public struct GWCombinedBillingRecommendation_Unity
	{
		public bool isSubscription;

		public GWIAPRecommendation_Unity iap;

		public GWSubscriptionRecommendation_Unity sub;
	}

	private const string VERSION = "2.1.2";

	private const string GWALLET_SERVER_STAGE = "http://gwallet-stage.glu.com/wallet-server/";

	private const string GWALLET_SERVER_CERTIFICATION = "http://gwallet-cert.glu.com/wallet-server/";

	private const string GWALLET_SERVER_PREPROD = "http://gwallet-pp.glu.com/wallet-server/";

	private const string GWALLET_SERVER_LIVE = "http://gwallet.glu.com/wallet-server/";

	private const string GGN_SERVER_PREPROD = "http://ggnpp.glu.com/android/";

	private const string GGN_SERVER_LIVE = "http://m.glu.com/android/";

	private const string EVENT_NOTIFICATION_IMPRESSION = "NOTIFICATION_IMPRESSION";

	private const string EVENT_NOTIFICATION_CLICKTHROUGH = "NOTIFICATION_CLICKTHROUGH";

	private const string EVENT_AD_IMPRESSION = "AD_IMPRESSION";

	private const string EVENT_AD_CLICKTHROUGH = "AD_CLICKTHROUGH";

	private const string EVENT_CUSTOM_IMPRESSION = "CUSTOM_IMPRESSION";

	private const string EVENT_CUSTOM_CLICKTHROUGH = "CUSTOM_CLICKTHROUGH";

	private const string EVENT_PRESTITIAL_IMPRESSION = "PRESTITIAL_IMPRESSION";

	private const string EVENT_PRESTITIAL_CLICKTHROUGH = "PRESTITIAL_CLICKTHROUGH";

	private const string EVENT_SESSION_START = "SESSION_START";

	private const string EVENT_SESSION_END = "SESSION_END";

	public const string NOTIFICATION_LOCATION_BANK = "BANK";

	public const string NOTIFICATION_LOCATION_STORE = "STORE";

	public const string NOTIFICATION_LOCATION_LAUNCH = "LAUNCH";

	private const string NOTIFICATION_DISPLAY_TYPE_OUT_OF_GAME = "OUT_OF_GAME";

	private const string NOTIFICATION_NOTE_TYPE_ANNOUNCEMENT = "ANNOUNCEMENT";

	private const string NOTIFICATION_NOTE_TYPE_INTERSTITIAL = "INTERSTITIAL";

	private const string NOTIFICATION_NOTE_TYPE_INCENTIVIZED_INTERSTITIAL = "INCENTIVIZED_INTERSTITIAL";

	private const string NOTIFICATION_NOTE_TYPE_VGP = "VGP";

	private const string NOTIFICATION_NOTE_TYPE_IAP = "IAP";

	private const string NOTIFICATION_NOTE_TYPE_SUBSCRIPTION_IAP = "SUBSCRIPTION_IAP";

	private const string NOTIFICATION_NOTE_TYPE_LAUNCH_GAME = "LAUNCH_GAME";

	private const string NOTIFICATION_NOTE_TYPE_ANNOUNCEMENT_TEXT = "ANNOUNCEMENT_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_INTERSTITIAL_TEXT = "INTERSTITIAL_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_INCENTIVIZED_INTERSTITIAL_TEXT = "INCENTIVIZED_INTERSTITIAL_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_VGP_TEXT = "VGP_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_IAP_TEXT = "IAP_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_SUBSCRIPTION_IAP_TEXT = "SUBSCRIPTION_IAP_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_LAUNCH_GAME_TEXT = "LAUNCH_GAME_TEXT";

	private const string NOTIFICATION_NOTE_TYPE_PLAYHAVEN = "PLAYHAVEN";

	private const string AD_TYPE_CUSTOM = "CUSTOM";

	private const string AD_TYPE_TAPJOY = "TAPJOY";

	private const string AD_TYPE_AMAZON = "AMAZON";

	public const string AD_LOCATION_BANK = "BANK";

	public const string AD_LOCATION_STORE = "STORE";

	public const string AD_LOCATION_MAIN = "MAIN_GAME_SCREEN";

	private const string AD_CUSTOM_ACTION_IAP = "IAP";

	private const string AD_CUSTOM_ACTION_SUBSCRIPTION_IAP = "SUBSCRIPTION_IAP";

	private const string AD_CUSTOM_ACTION_URL = "URL";

	private const string AD_CUSTOM_ACTION_INCENTIVIZED_URL = "INCENTIVIZED_URL";

	private const string AD_CUSTOM_ACTION_INGAME_ITEM = "INGAME_ITEM";

	private const string AD_CUSTOM_ACTION_LAUNCH_GAME = "LAUNCH_GAME";

	private const string AD_CUSTOM_ACTION_ANNOUNCEMENT = "ANNOUNCEMENT";

	private static string PRESTITIAL_BG;

	private static string PRESTITIAL_BENEFITS;

	private static string PRESTITIAL_VIPBRONZE;

	private static string PRESTITIAL_VIPSILVER;

	private static string PRESTITIAL_VIPGOLD;

	private static string PRESTITIAL_VIPPLATINUM;

	private static string SRC;

	private static string PLATFORM;

	private static string GGN_IMAGE;

	private static string GGN_ALT_IMAGE;

	private static string GGN_BADGE_IMAGE;

	private static int GGN_WIDTH;

	private static int GGN_HEIGHT;

	private static int GGN_GRAVITY;

	private static int GGN_PADDING_LEFT;

	private static int GGN_PADDING_TOP;

	private static int GGN_PADDING_RIGHT;

	private static int GGN_PADDING_BOTTOM;

	private static int GGN_BADGE_WIDTH;

	private static int GGN_BADGE_HEIGHT;

	private static int GGN_BADGE_GRAVITY;

	private static int GGN_BADGE_PADDING_LEFT;

	private static int GGN_BADGE_PADDING_TOP;

	private static int GGN_BADGE_PADDING_RIGHT;

	private static int GGN_BADGE_PADDING_BOTTOM;

	private static GameObject go;

	private int balance;

	private float lastCFAttempt;

	private static bool disableBackKeyThisUpdate;

	public static GWSubscriptionRecommendation_Unity[] subscription_recommendations;

	public static GWIAPRecommendation_Unity[] iap_recommendations;

	public static GWCombinedBillingRecommendation_Unity[] combined_recommendations;

	public static string[] owned_subscriptions;

	private static GWNotification_Unity[] notification_recommendations;

	private static GWAdvertisement_Unity[] ad_recommendations;

	private static int lhc = int.MinValue;

	private static int highest_recommended_index;

	private static SUBPLAN highest_recommended_value;

	private static bool location_launch_loaded;

	private static bool location_store_loaded;

	private static bool location_bank_loaded;

	private static string activeNotificationType;

	private static int activeNotificationID;

	private static string activeNotificationURI;

	public static int ggncount;

	public static int LocalHardCurrency
	{
		get
		{
			if (lhc == int.MinValue)
			{
				string @string = PlayerPrefs.GetString("large_hadron_collider_identifier", "0");
				if (!@string.Equals("0"))
				{
					try
					{
						string text = AJavaTools.DeviceInfo.GetAndroidID();
						if (text == null)
						{
							text = "gluheartandroid!";
						}
						lhc = Convert.ToInt32(Decrypt(Base64Decode(@string), text));
					}
					catch (Exception ex)
					{
						Log("Error reading local hard currency: " + ex.ToString());
					}
				}
				else
				{
					lhc = 0;
				}
			}
			return lhc;
		}
		set
		{
			lhc = value;
			if (lhc == 0)
			{
				PlayerPrefs.DeleteKey("large_hadron_collider_identifier");
			}
			else
			{
				string clearText = lhc.ToString();
				try
				{
					string text = AJavaTools.DeviceInfo.GetAndroidID();
					if (text == null)
					{
						text = "gluheartandroid!";
					}
					PlayerPrefs.SetString("large_hadron_collider_identifier", Base64Encode(Encrypt(clearText, text)));
				}
				catch (Exception ex)
				{
					Log("Error setting local hard currency: " + ex.ToString());
				}
			}
			PlayerPrefs.Save();
		}
	}

	public static void SetupPrestitial(string background, string benefits, string vipbronze, string vipsilver, string vipgold, string vipplatinum)
	{
		Log("GWalletHelper.SetupPrestitial( " + background + ", " + benefits + ", " + vipbronze + ", " + vipsilver + ", " + vipgold + ", " + vipplatinum + " )");
		PRESTITIAL_BG = background;
		PRESTITIAL_BENEFITS = benefits;
		PRESTITIAL_VIPBRONZE = vipbronze;
		PRESTITIAL_VIPSILVER = vipsilver;
		PRESTITIAL_VIPGOLD = vipgold;
		PRESTITIAL_VIPPLATINUM = vipplatinum;
	}

	public static void SetupGGN(string ggnImage, string ggnBadgeImage, string ggnAltImage = "")
	{
		Log("GWalletHelper.SetupGGN( " + ggnImage + ", " + ggnBadgeImage + ", " + ggnAltImage + " )");
		GGN_IMAGE = ggnImage;
		GGN_ALT_IMAGE = ggnAltImage;
		GGN_BADGE_IMAGE = ggnBadgeImage;
	}

	public static void SetupGGNButton(int width, int height, int gravity, int left = 0, int top = 0, int right = 0, int bottom = 0)
	{
		Log("GWalletHelper.SetupGGNButton( " + width + ", " + height + ", " + gravity + ", " + left + ", " + top + ", " + right + ", " + bottom + " )");
		GGN_WIDTH = width;
		GGN_HEIGHT = height;
		GGN_GRAVITY = gravity;
		GGN_PADDING_LEFT = left;
		GGN_PADDING_TOP = top;
		GGN_PADDING_RIGHT = right;
		GGN_PADDING_BOTTOM = bottom;
	}

	public static void SetupGGNBadge(int width, int height, int gravity, int left = 0, int top = 0, int right = 0, int bottom = 0)
	{
		Log("GWalletHelper.SetupGGNBadge( " + width + ", " + height + ", " + gravity + ", " + left + ", " + top + ", " + right + ", " + bottom + " )");
		GGN_BADGE_WIDTH = width;
		GGN_BADGE_HEIGHT = height;
		GGN_BADGE_GRAVITY = gravity;
		GGN_BADGE_PADDING_LEFT = left;
		GGN_BADGE_PADDING_TOP = top;
		GGN_BADGE_PADDING_RIGHT = right;
		GGN_BADGE_PADDING_BOTTOM = bottom;
	}

	public static void Init(GameObject gameObject)
	{
		gameObject.AddComponent("GWalletHelper");
	}

	private void Awake()
	{
		Log("GWalletHelper.Awake()");
		Log("GWalletHelper Version: 2.1.2");
		go = base.gameObject;
		string text = null;
		string text2 = null;
		if (AJavaTools.Properties.IsBuildGoogle())
		{
			text = "ANDROID_MARKET";
		}
		else if (AJavaTools.Properties.IsBuildAmazon())
		{
			text = "AMAZON_APPSTORE_FOR_ANDROID";
		}
		text2 = AJavaTools.Properties.GetGWalletSKU();
		if (text == null || text2 == null || text2.Equals(string.Empty))
		{
			LogError("ERROR - Failed to query build type or gwallet sku");
		}
		else
		{
			GWallet.Init(text, text2, (!Debug.isDebugBuild) ? "http://gwallet.glu.com/wallet-server/" : "http://gwallet-pp.glu.com/wallet-server/", base.gameObject.name, "onGWalletEvent");
			Log("GWallet Version: " + GWallet.GetVersion());
			GWallet.LogEvent("Session Start", "SESSION_START");
		}
		if (PRESTITIAL_BG == null || PRESTITIAL_BENEFITS == null)
		{
			LogWarning("Warning - Call SetupPrestitial() before attaching GWalletHelper to the scene");
		}
		else
		{
			AAds.Custom.Init("prestitial-bg", base.gameObject.name, PRESTITIAL_BG, string.Empty);
			AAds.Custom.Init("prestitial-benefits", base.gameObject.name, PRESTITIAL_BENEFITS, string.Empty);
			AAds.Custom.Init("prestitial-close", base.gameObject.name, "close", string.Empty);
		}
		CheckBillingRecommendations();
		CheckNotifications();
		CheckAds();
		SRC = AJavaTools.Properties.GetGGNSRC();
		if (AJavaTools.Properties.IsBuildGoogle())
		{
			PLATFORM = "android";
		}
		else
		{
			PLATFORM = AJavaTools.Properties.GetBuildType();
		}
		if (Screen.width >= 1024 || Screen.height >= 1024)
		{
			PLATFORM += "tab";
		}
		if (PLATFORM == null || SRC == null || SRC.Equals(string.Empty))
		{
			LogError("ERROR - Failed to query build type or ggn src");
		}
		else if (GGN_IMAGE == null || GGN_BADGE_IMAGE == null)
		{
			LogWarning("WARNING - Call SetupGGN() before attaching GWalletHelper to the scene");
		}
		else
		{
			AAds.Custom.Init("ggn", base.gameObject.name, GGN_IMAGE, GGN_ALT_IMAGE);
			AAds.Custom.Init("ggnbadge", base.gameObject.name, GGN_BADGE_IMAGE, string.Empty);
			AAds.Custom.Attach("ggnbadge", "ggn");
		}
		StartCoroutine(CheckGGN());
	}

	private void Update()
	{
		GWallet.Update((int)Time.deltaTime);
		if (LocalHardCurrency > 0 && Time.realtimeSinceStartup - lastCFAttempt > 10f)
		{
			lastCFAttempt = Time.realtimeSinceStartup;
			int pReturnBalance = -1;
			if (GWallet.AddCurrency((uint)LocalHardCurrency, "Pre-existing balance added to gWallet", "CREDIT_BALANCE_CARRIED_FORWARD", ref pReturnBalance) == eGWalletCompletionStatus.GWALLET_SUCCESS)
			{
				Log("Balance carry forward successful, zeroing local hard currency");
				LocalHardCurrency = 0;
			}
		}
		int pReturnBalance2 = balance;
		GWallet.GetBalance(ref pReturnBalance2);
		if (pReturnBalance2 != balance)
		{
			balance = pReturnBalance2;
			Log("Received new balance");
			SendMessage("onGWalletBalanceChanged");
		}
	}

	private void LateUpdate()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && !disableBackKeyThisUpdate)
		{
			if (AAds.Custom.IsActive("prestitial-bg"))
			{
				AAds.Custom.Hide("prestitial-iap-0");
				AAds.Custom.Hide("prestitial-iap-1");
				AAds.Custom.Hide("prestitial-benefits");
				AAds.Custom.Hide("prestitial-close");
				AAds.Custom.Hide("prestitial-bg");
				SendMessage("onGWalletDisplayDismissed", true);
			}
			if (AAds.Custom.IsActive("LAUNCH"))
			{
				AAds.Custom.Hide("LAUNCH");
				AAds.Custom.Hide("LAUNCH-close");
				SendMessage("onGWalletDisplayDismissed", false);
			}
			else if (AAds.Custom.IsActive("BANK"))
			{
				AAds.Custom.Hide("BANK");
				AAds.Custom.Hide("BANK-close");
				SendMessage("onGWalletDisplayDismissed", false);
			}
			else if (AAds.Custom.IsActive("STORE"))
			{
				AAds.Custom.Hide("STORE");
				AAds.Custom.Hide("STORE-close");
				SendMessage("onGWalletDisplayDismissed", false);
			}
		}
		disableBackKeyThisUpdate = false;
	}

	private void OnApplicationPause(bool pause)
	{
		Log("GWalletHelper.OnApplicationPause( " + pause + " )");
		if (pause)
		{
			GWallet.Pause();
			GWallet.LogEvent("Session End", "SESSION_END");
		}
		else
		{
			GWallet.Resume();
			GWallet.LogEvent("Session Start", "SESSION_START");
			StartCoroutine(CheckGGN());
		}
	}

	private void OnApplicationQuit()
	{
		Log("GWalletHelper.OnApplicationQuit()");
		GWallet.LogEvent("Session End", "SESSION_END");
		GWallet.Destroy();
	}

	public static bool IsSubscriptionRecommendationAvailable()
	{
		return subscription_recommendations != null && subscription_recommendations.Length > 0;
	}

	public static bool IsIAPRecommendationAvailable()
	{
		return iap_recommendations != null && iap_recommendations.Length > 0;
	}

	public static bool IsBillingRecommendationAvailable()
	{
		return IsSubscriptionRecommendationAvailable() || IsIAPRecommendationAvailable();
	}

	public static bool IsGWalletDisplayActive()
	{
		if (AAds.Custom.IsActive("prestitial-bg"))
		{
			return true;
		}
		if (AAds.Custom.IsActive("LAUNCH") || AAds.Custom.IsActive("STORE") || AAds.Custom.IsActive("BANK"))
		{
			return true;
		}
		return false;
	}

	public static int GetBalance()
	{
		int pReturnBalance = 0;
		if (GWallet.GetBalance(ref pReturnBalance) != 0)
		{
			return LocalHardCurrency;
		}
		return pReturnBalance;
	}

	public static void AddCurrency(int amount, string type, string desc)
	{
		Log("GWalletHelper.AddCurrency( " + amount + ", " + type + ", " + desc + " )");
		if (amount == 0)
		{
			return;
		}
		if (amount < 0)
		{
			LogError("AddCurrency called with negative amount");
			return;
		}
		int pReturnBalance = -1;
		eGWalletCompletionStatus eGWalletCompletionStatus2 = GWallet.AddCurrency((uint)amount, desc, type, ref pReturnBalance);
		if (eGWalletCompletionStatus2 != 0)
		{
			Log("GWallet AddCurrency Failed - Error Code: " + eGWalletCompletionStatus2);
			Log("Adding to local hard currency");
			LocalHardCurrency += amount;
		}
	}

	public static void SubtractCurrency(int amount, string type, string desc)
	{
		Log("GWalletHelper.SubtractCurrency( " + amount + ", " + type + ", " + desc + " )");
		if (amount == 0)
		{
			return;
		}
		if (amount < 0)
		{
			LogError("SubtractCurrency called with negative amount");
			return;
		}
		int pReturnBalance = -1;
		eGWalletCompletionStatus eGWalletCompletionStatus2 = GWallet.SubtractCurrency((uint)amount, desc, type, ref pReturnBalance);
		if (eGWalletCompletionStatus2 != 0)
		{
			Log("GWallet SubtractCurrency Failed - Error Code: " + eGWalletCompletionStatus2);
			Log("Subtracting from local hard currency");
			LocalHardCurrency -= amount;
		}
	}

	public static bool AddSoftCurrency(int amount, string type, string desc)
	{
		Log("GWalletHelper.AddSoftCurrency( " + amount + ", " + type + ", " + desc + " )");
		if (amount == 0)
		{
			return false;
		}
		if (amount < 0)
		{
			LogError("AddSoftCurrency called with negative amount");
			return false;
		}
		eGWalletCompletionStatus eGWalletCompletionStatus2 = GWallet.AddSoftCurrency(amount, desc, type);
		if (eGWalletCompletionStatus2 != 0)
		{
			Log("GWallet AddSoftCurrency Failed - Error Code: " + eGWalletCompletionStatus2);
			return false;
		}
		return true;
	}

	public static bool SubtractSoftCurrency(int amount, string type, string desc)
	{
		Log("GWalletHelper.SubtractSoftCurrency( " + amount + ", " + type + ", " + desc + " )");
		if (amount == 0)
		{
			return false;
		}
		if (amount < 0)
		{
			LogError("SubtractSoftCurrency called with negative amount");
			return false;
		}
		eGWalletCompletionStatus eGWalletCompletionStatus2 = GWallet.SubtractSoftCurrency(amount, desc, type);
		if (eGWalletCompletionStatus2 != 0)
		{
			Log("GWallet SubtractSoftCurrency Failed - Error Code: " + eGWalletCompletionStatus2);
			return false;
		}
		return true;
	}

	public static bool ShowPrestitial()
	{
		Log("GWalletHelper.ShowPrestitial()");
		if ((Debug.isDebugBuild && AJavaTools.Util.GetRunCount() % 2 != 1) || (!Debug.isDebugBuild && AJavaTools.Util.GetRunCount() % 7 != 1))
		{
			Log("Skipping prestitial for this launch");
			return false;
		}
		if (!GWallet.IsDeviceSupported() || GWallet.IsSubscriber() || !IsSubscriptionRecommendationAvailable() || !AAds.Custom.IsAvailable("prestitial-bg") || !AAds.Custom.IsAvailable("prestitial-benefits") || !AAds.Custom.IsAvailable("prestitial-iap-0") || Application.internetReachability == NetworkReachability.NotReachable)
		{
			Log("Prestitial conditions not met - skipping");
			return false;
		}
		AAds.Custom.Show("prestitial-bg", Screen.width, Screen.height, 17);
		float num = (float)Screen.width / (float)AAds.Custom.GetSourceWidth("prestitial-bg");
		float num2 = (float)Screen.height / (float)AAds.Custom.GetSourceHeight("prestitial-bg");
		if (AAds.Custom.GetSourceWidth("prestitial-bg") > AAds.Custom.GetSourceHeight("prestitial-bg"))
		{
			AAds.Custom.SetPadding("prestitial-benefits", 0, 0, (int)(20f * num), (int)(12f * num2));
			AAds.Custom.Show("prestitial-benefits", (int)((float)AAds.Custom.GetSourceWidth("prestitial-benefits") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-benefits") * num2), 85);
			AAds.Custom.SetPadding("prestitial-iap-0", 0, (int)(40f * num2), (int)(20f * num), 0);
			AAds.Custom.Show("prestitial-iap-0", (int)((float)AAds.Custom.GetSourceWidth("prestitial-iap-0") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-iap-0") * num2), 53);
			AAds.Custom.SetPadding("prestitial-iap-1", 0, (int)(285f * num2), (int)(20f * num), 0);
			AAds.Custom.Show("prestitial-iap-1", (int)((float)AAds.Custom.GetSourceWidth("prestitial-iap-1") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-iap-1") * num2), 53);
			AAds.Custom.Show("prestitial-close", (int)((float)AAds.Custom.GetSourceWidth("prestitial-close") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-close") * num2), 53);
		}
		else
		{
			AAds.Custom.SetPadding("prestitial-benefits", 0, 0, 0, (int)(2f * num2));
			AAds.Custom.Show("prestitial-benefits", (int)((float)AAds.Custom.GetSourceWidth("prestitial-benefits") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-benefits") * num2), 81);
			AAds.Custom.Show("prestitial-close", (int)((float)AAds.Custom.GetSourceWidth("prestitial-close") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-close") * num2), 53);
			AAds.Custom.SetPadding("prestitial-iap-0", (int)(18f * num), 0, 0, (int)(125f * num2));
			AAds.Custom.Show("prestitial-iap-0", (int)((float)AAds.Custom.GetSourceWidth("prestitial-iap-0") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-iap-0") * num2), 83);
			AAds.Custom.SetPadding("prestitial-iap-1", 0, 0, (int)(35f * num), (int)(125f * num2));
			AAds.Custom.Show("prestitial-iap-1", (int)((float)AAds.Custom.GetSourceWidth("prestitial-iap-1") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-iap-1") * num2), 85);
			AAds.Custom.Show("prestitial-close", (int)((float)AAds.Custom.GetSourceWidth("prestitial-close") * num), (int)((float)AAds.Custom.GetSourceHeight("prestitial-close") * num2), 53);
		}
		GWallet.LogEvent("prestitial", "PRESTITIAL_IMPRESSION");
		disableBackKeyThisUpdate = true;
		return true;
	}

	private void CheckBillingRecommendations()
	{
		Log("GWalletHelper.CheckBillingRecommendations()");
		int numSubscriptionPlans = GWallet.GetNumSubscriptionPlans();
		owned_subscriptions = new string[numSubscriptionPlans];
		for (int i = 0; i < numSubscriptionPlans; i++)
		{
			GWSubscriptionPlan_Unity subscriptionPlan = default(GWSubscriptionPlan_Unity);
			GWallet.GetSubscriptionPlanAtIndex(i, ref subscriptionPlan);
			owned_subscriptions[i] = subscriptionPlan.m_planName;
		}
		bool flag = false;
		int numSubscriptionRecommendations = GWallet.GetNumSubscriptionRecommendations();
		if (subscription_recommendations == null || numSubscriptionRecommendations != subscription_recommendations.Length)
		{
			subscription_recommendations = new GWSubscriptionRecommendation_Unity[numSubscriptionRecommendations];
			flag = true;
		}
		Log("Number of recommended subscriptions: " + numSubscriptionRecommendations);
		GWSubscriptionRecommendation_Unity[] array = new GWSubscriptionRecommendation_Unity[numSubscriptionRecommendations];
		for (int j = 0; j < numSubscriptionRecommendations; j++)
		{
			GWallet.GetSubscriptionRecommendationAtIndex(j, ref array[j]);
		}
		for (int k = 0; k < numSubscriptionRecommendations; k++)
		{
			int num = k;
			for (int l = k + 1; l < numSubscriptionRecommendations; l++)
			{
				if (array[l].m_displayOrder < array[num].m_displayOrder)
				{
					num = l;
				}
			}
			if (num != k)
			{
				GWSubscriptionRecommendation_Unity gWSubscriptionRecommendation_Unity = array[k];
				array[k] = array[num];
				array[num] = gWSubscriptionRecommendation_Unity;
			}
		}
		for (int m = 0; m < numSubscriptionRecommendations; m++)
		{
			string storeSkuCode = subscription_recommendations[m].m_storeSkuCode;
			Log("subscription[" + m + "]: " + array[m].m_planName + " " + array[m].m_storeSkuCode + " " + array[m].m_displayUrl);
			array[m].m_storeSkuCode = array[m].m_storeSkuCode.Trim();
			subscription_recommendations[m] = array[m];
			try
			{
				SUBPLAN sUBPLAN = (SUBPLAN)(int)Enum.Parse(typeof(SUBPLAN), subscription_recommendations[m].m_planName, true);
				if (m == 0 || sUBPLAN > highest_recommended_value)
				{
					Log("Found new highest recommendation: " + subscription_recommendations[m].m_planName);
					highest_recommended_value = sUBPLAN;
					highest_recommended_index = m;
				}
			}
			catch (Exception ex)
			{
				Log(ex.Message);
			}
			if (storeSkuCode != null && storeSkuCode.Equals(subscription_recommendations[m].m_storeSkuCode))
			{
				continue;
			}
			Log("Found new subscription");
			flag = true;
			if (PRESTITIAL_BG == null || m >= 2)
			{
				continue;
			}
			AAds.Custom.Hide("prestitial-iap-" + m);
			if (Application.internetReachability == NetworkReachability.NotReachable || subscription_recommendations[m].m_displayUrl == null || subscription_recommendations[m].m_displayUrl.Equals(string.Empty))
			{
				if (subscription_recommendations[m].m_planName.ToLower().Equals("bronze") && PRESTITIAL_VIPBRONZE != null)
				{
					AAds.Custom.Init("prestitial-iap-" + m, base.gameObject.name, PRESTITIAL_VIPBRONZE, string.Empty);
				}
				else if (subscription_recommendations[m].m_planName.ToLower().Equals("silver") && PRESTITIAL_VIPSILVER != null)
				{
					AAds.Custom.Init("prestitial-iap-" + m, base.gameObject.name, PRESTITIAL_VIPSILVER, string.Empty);
				}
				else if (subscription_recommendations[m].m_planName.ToLower().Equals("gold") && PRESTITIAL_VIPGOLD != null)
				{
					AAds.Custom.Init("prestitial-iap-" + m, base.gameObject.name, PRESTITIAL_VIPGOLD, string.Empty);
				}
				else if (subscription_recommendations[m].m_planName.ToLower().Equals("platinum") && PRESTITIAL_VIPPLATINUM != null)
				{
					AAds.Custom.Init("prestitial-iap-" + m, base.gameObject.name, PRESTITIAL_VIPPLATINUM, string.Empty);
				}
			}
			else
			{
				AAds.Custom.Init("prestitial-iap-" + m, base.gameObject.name, subscription_recommendations[m].m_displayUrl, string.Empty);
			}
		}
		if (flag)
		{
			if (AAds.Custom.IsActive("prestitial-bg"))
			{
				ShowPrestitial();
			}
			Log("Received new subscription recommendations");
			SendMessage("onGWalletSubscriptionRecommendationsChanged");
		}
		bool flag2 = false;
		numSubscriptionRecommendations = GWallet.GetNumIAPRecommendations();
		if (iap_recommendations == null || numSubscriptionRecommendations != iap_recommendations.Length)
		{
			iap_recommendations = new GWIAPRecommendation_Unity[numSubscriptionRecommendations];
			flag2 = true;
		}
		Log("Number of recommended iaps: " + numSubscriptionRecommendations);
		GWIAPRecommendation_Unity[] array2 = new GWIAPRecommendation_Unity[numSubscriptionRecommendations];
		for (int n = 0; n < numSubscriptionRecommendations; n++)
		{
			GWallet.GetIAPRecommendationAtIndex(n, ref array2[n]);
		}
		for (int num2 = 0; num2 < numSubscriptionRecommendations; num2++)
		{
			int num3 = num2;
			for (int num4 = num2 + 1; num4 < numSubscriptionRecommendations; num4++)
			{
				if (array2[num4].m_displayOrder < array2[num3].m_displayOrder)
				{
					num3 = num4;
				}
			}
			if (num3 != num2)
			{
				GWIAPRecommendation_Unity gWIAPRecommendation_Unity = array2[num2];
				array2[num2] = array2[num3];
				array2[num3] = gWIAPRecommendation_Unity;
			}
		}
		for (int num5 = 0; num5 < numSubscriptionRecommendations; num5++)
		{
			string storeSkuCode2 = iap_recommendations[num5].m_storeSkuCode;
			Log("iap[" + num5 + "]: " + array2[num5].m_itemName + " " + array2[num5].m_storeSkuCode + " " + array2[num5].m_displayUrl);
			array2[num5].m_storeSkuCode = array2[num5].m_storeSkuCode.Trim();
			iap_recommendations[num5] = array2[num5];
			if (storeSkuCode2 == null || !storeSkuCode2.Equals(iap_recommendations[num5].m_storeSkuCode))
			{
				Log("Found new iap");
				flag2 = true;
			}
		}
		if (flag2)
		{
			Log("Received new iap recommendations");
			SendMessage("onGWalletIAPRecommendationsChanged");
		}
		if (flag2 || flag)
		{
			CombineRecommendations();
		}
	}

	private void CombineRecommendations()
	{
		Log("GWalletHelper.CombineRecommendations()");
		combined_recommendations = new GWCombinedBillingRecommendation_Unity[((subscription_recommendations != null) ? subscription_recommendations.Length : 0) + ((iap_recommendations != null) ? iap_recommendations.Length : 0)];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string iAPOrdering = AJavaTools.Properties.GetIAPOrdering();
		for (int i = 0; i < iAPOrdering.Length; i++)
		{
			if (iAPOrdering[i] == 'i' && iap_recommendations != null && num2 < iap_recommendations.Length)
			{
				combined_recommendations[num].isSubscription = false;
				combined_recommendations[num].iap = iap_recommendations[num2];
				num++;
				num2++;
			}
			else if (iAPOrdering[i] == 's' && subscription_recommendations != null && num3 < subscription_recommendations.Length)
			{
				combined_recommendations[num].isSubscription = true;
				combined_recommendations[num].sub = subscription_recommendations[num3];
				num++;
				num3++;
			}
		}
		for (int j = 0; j < combined_recommendations.Length; j++)
		{
			Log("combined: " + combined_recommendations[j].isSubscription + " " + ((!combined_recommendations[j].isSubscription) ? combined_recommendations[j].iap.m_storeSkuCode : combined_recommendations[j].sub.m_storeSkuCode));
		}
		SendMessage("onGWalletBillingRecommendationsChanged");
	}

	private void CheckNotifications()
	{
		Log("GWalletHelper.CheckNotifications()");
		int numNotifications = GWallet.GetNumNotifications();
		if (numNotifications <= 0)
		{
			return;
		}
		if (notification_recommendations == null || numNotifications != notification_recommendations.Length)
		{
			notification_recommendations = new GWNotification_Unity[numNotifications];
		}
		Log("Number of notifications: " + numNotifications);
		for (int i = 0; i < numNotifications; i++)
		{
			int id = notification_recommendations[i].m_id;
			GWallet.GetNotificationAtIndex(i, ref notification_recommendations[i]);
			Log("notification[" + i + "]: " + notification_recommendations[i].m_id + " " + notification_recommendations[i].m_consumed + " " + notification_recommendations[i].m_displayType + " " + notification_recommendations[i].m_notificationType + " " + notification_recommendations[i].m_message);
			if (notification_recommendations[i].m_consumed || notification_recommendations[i].m_displayType.Equals("OUT_OF_GAME"))
			{
				continue;
			}
			if (id == notification_recommendations[i].m_id)
			{
				Log("Already added this notification");
			}
			else if ((notification_recommendations[i].m_displayType.Equals("LAUNCH") && !location_launch_loaded) || (notification_recommendations[i].m_displayType.Equals("STORE") && !location_store_loaded) || (notification_recommendations[i].m_displayType.Equals("BANK") && !location_bank_loaded))
			{
				if (notification_recommendations[i].m_notificationType.Equals("ANNOUNCEMENT"))
				{
					AAds.Custom.Init(notification_recommendations[i].m_displayType, base.gameObject.name, notification_recommendations[i].m_message, string.Empty);
				}
				else if (notification_recommendations[i].m_notificationType.Equals("INTERSTITIAL") || notification_recommendations[i].m_notificationType.Equals("INCENTIVIZED_INTERSTITIAL") || notification_recommendations[i].m_notificationType.Equals("VGP") || notification_recommendations[i].m_notificationType.Equals("IAP") || notification_recommendations[i].m_notificationType.Equals("SUBSCRIPTION_IAP") || notification_recommendations[i].m_notificationType.Equals("LAUNCH_GAME"))
				{
					AAds.Custom.Init(notification_recommendations[i].m_displayType, base.gameObject.name, notification_recommendations[i].m_message, string.Empty);
					AAds.Custom.Init(notification_recommendations[i].m_displayType + "-close", base.gameObject.name, "close", string.Empty);
					AAds.Custom.Attach(notification_recommendations[i].m_displayType + "-close", notification_recommendations[i].m_displayType);
				}
				if (notification_recommendations[i].m_displayType.Equals("LAUNCH"))
				{
					location_launch_loaded = true;
				}
				if (notification_recommendations[i].m_displayType.Equals("STORE"))
				{
					location_store_loaded = true;
				}
				if (notification_recommendations[i].m_displayType.Equals("BANK"))
				{
					location_bank_loaded = true;
				}
			}
		}
	}

	public static bool ShowNotification(string location)
	{
		Log("GWalletHelper.ShowNotification( " + location + " )");
		if (!GWallet.IsDeviceSupported())
		{
			return false;
		}
		if (notification_recommendations != null)
		{
			for (int i = 0; i < notification_recommendations.Length; i++)
			{
				Log("notification[" + i + "]: " + notification_recommendations[i].m_id + " " + notification_recommendations[i].m_consumed + " " + notification_recommendations[i].m_displayType + " " + notification_recommendations[i].m_message);
				if (!notification_recommendations[i].m_consumed && notification_recommendations[i].m_displayType.Equals(location))
				{
					if (notification_recommendations[i].m_notificationType.Equals("ANNOUNCEMENT"))
					{
						AAds.Custom.Show(notification_recommendations[i].m_displayType, Screen.width, Screen.height, 17);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("INTERSTITIAL") || notification_recommendations[i].m_notificationType.Equals("INCENTIVIZED_INTERSTITIAL") || notification_recommendations[i].m_notificationType.Equals("LAUNCH_GAME"))
					{
						AAds.Custom.Show(notification_recommendations[i].m_displayType, Screen.width, Screen.height, 17);
						AAds.Custom.Show(notification_recommendations[i].m_displayType + "-close", Screen.width / 10, Screen.width / 10, 53);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("VGP") || notification_recommendations[i].m_notificationType.Equals("IAP") || notification_recommendations[i].m_notificationType.Equals("SUBSCRIPTION_IAP"))
					{
						AAds.Custom.Show(notification_recommendations[i].m_displayType, Screen.width, Screen.height, 17);
						AAds.Custom.Show(notification_recommendations[i].m_displayType + "-close", Screen.width / 10, Screen.width / 10, 53);
						GWallet.LogEvent(notification_recommendations[i].m_id + "-" + notification_recommendations[i].m_uri, "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("ANNOUNCEMENT_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onAnnouncementClick", "Alert", notification_recommendations[i].m_message, "OK", string.Empty, string.Empty);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("INTERSTITIAL_TEXT") || notification_recommendations[i].m_notificationType.Equals("INCENTIVIZED_INTERSTITIAL_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onInterstitialClick", "Alert", notification_recommendations[i].m_message, "Go", "Cancel", string.Empty);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("VGP_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onVGPClick", "Alert", notification_recommendations[i].m_message, "Get", "Cancel", string.Empty);
						GWallet.LogEvent(notification_recommendations[i].m_id + "-" + notification_recommendations[i].m_uri, "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("IAP_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onIAPClick", "Alert", notification_recommendations[i].m_message, "Buy", "Cancel", string.Empty);
						GWallet.LogEvent(notification_recommendations[i].m_id + "-" + notification_recommendations[i].m_uri, "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("SUBSCRIPTION_IAP_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onSubscriptionClick", "Alert", notification_recommendations[i].m_message, "Subscribe", "Cancel", string.Empty);
						GWallet.LogEvent(notification_recommendations[i].m_id + "-" + notification_recommendations[i].m_uri, "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("LAUNCH_GAME_TEXT"))
					{
						AJavaTools.UI.ShowAlert(go.name, "onLaunchGameClick", "Alert", notification_recommendations[i].m_message, "Launch", "Cancel", string.Empty);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					else if (notification_recommendations[i].m_notificationType.Equals("PLAYHAVEN"))
					{
						AAds.PlayHaven.Show(notification_recommendations[i].m_message);
						GWallet.LogEvent(Convert.ToString(notification_recommendations[i].m_id), "NOTIFICATION_IMPRESSION");
					}
					activeNotificationType = notification_recommendations[i].m_notificationType;
					activeNotificationID = notification_recommendations[i].m_id;
					activeNotificationURI = notification_recommendations[i].m_uri;
					notification_recommendations[i].m_consumed = true;
					GWallet.SetNotificationConsumed(notification_recommendations[i].m_id);
					disableBackKeyThisUpdate = true;
					return true;
				}
			}
		}
		return false;
	}

	private void CheckAds()
	{
		Log("GWalletHelper.CheckAds()");
		int numAdvertisements = GWallet.GetNumAdvertisements();
		if (numAdvertisements <= 0)
		{
			return;
		}
		HideAd();
		if (ad_recommendations == null || numAdvertisements != ad_recommendations.Length)
		{
			ad_recommendations = new GWAdvertisement_Unity[numAdvertisements];
		}
		Log("Number of ads: " + numAdvertisements);
		for (int i = 0; i < numAdvertisements; i++)
		{
			string resourceUrl = ad_recommendations[i].m_resourceUrl;
			GWallet.GetAdvertisementAtIndex(i, ref ad_recommendations[i]);
			Log("ads[" + i + "]: " + ad_recommendations[i].m_type + " " + ad_recommendations[i].m_displayLocation + " " + ad_recommendations[i].m_resourceUrl);
			if (ad_recommendations[i].m_type.Equals("CUSTOM") && resourceUrl != ad_recommendations[i].m_resourceUrl)
			{
				AAds.Custom.Hide("ad-" + i);
				AAds.Custom.Init("ad-" + i, base.gameObject.name, ad_recommendations[i].m_resourceUrl, string.Empty);
			}
		}
	}

	public static void ShowAd(string location, int width, int height, int gravity)
	{
		Log("GWalletHelper.ShowAd( " + location + ", " + width + ", " + height + ", " + gravity + " )");
		if (ad_recommendations != null)
		{
			for (int i = 0; i < ad_recommendations.Length; i++)
			{
				if (!location.Equals(ad_recommendations[i].m_displayLocation))
				{
					continue;
				}
				if (ad_recommendations[i].m_type.Equals("AMAZON"))
				{
					AAds.Amazon.Show(width, height, gravity);
					GWallet.LogEvent("amazon", "AD_IMPRESSION");
				}
				else if (ad_recommendations[i].m_type.Equals("TAPJOY"))
				{
					AAds.Tapjoy.Show(width, height, gravity);
					GWallet.LogEvent("tapjoy", "AD_IMPRESSION");
				}
				else if (ad_recommendations[i].m_type.Equals("CUSTOM"))
				{
					AAds.Custom.Show("ad-" + i, width, height, gravity);
					if (ad_recommendations[i].m_attributeCount > 0)
					{
						GWallet.LogEvent("custom-" + ad_recommendations[i].m_key[0], "AD_IMPRESSION");
					}
					else
					{
						GWallet.LogEvent("custom", "AD_IMPRESSION");
					}
				}
				return;
			}
		}
		string defaultAdProvider = AJavaTools.Properties.GetDefaultAdProvider();
		if (defaultAdProvider != null && defaultAdProvider.Equals("amazon"))
		{
			AAds.Amazon.Show(width, height, gravity);
		}
		else if (defaultAdProvider != null && defaultAdProvider.Equals("tapjoy"))
		{
			AAds.Tapjoy.Show(width, height, gravity);
		}
	}

	public static void HideAd()
	{
		Log("GWalletHelper.HideAd()");
		if (ad_recommendations != null)
		{
			for (int i = 0; i < ad_recommendations.Length; i++)
			{
				if (ad_recommendations[i].m_type.Equals("AMAZON"))
				{
					AAds.Amazon.Hide();
				}
				else if (ad_recommendations[i].m_type.Equals("TAPJOY"))
				{
					AAds.Tapjoy.Hide();
				}
				else if (ad_recommendations[i].m_type.Equals("CUSTOM"))
				{
					AAds.Custom.Hide("ad-" + i);
				}
			}
		}
		string defaultAdProvider = AJavaTools.Properties.GetDefaultAdProvider();
		if (defaultAdProvider != null && defaultAdProvider.Equals("amazon"))
		{
			AAds.Amazon.Hide();
		}
		else if (defaultAdProvider != null && defaultAdProvider.Equals("tapjoy"))
		{
			AAds.Tapjoy.Hide();
		}
	}

	public static bool IsGGNAvailable()
	{
		return SRC != null && PLATFORM != null;
	}

	public static void ShowGGN()
	{
		Log("GWalletHelper.ShowGGN()");
		if (IsGGNAvailable())
		{
			AAds.Custom.SetPadding("ggn", GGN_PADDING_LEFT, GGN_PADDING_TOP, GGN_PADDING_RIGHT, GGN_PADDING_BOTTOM);
			AAds.Custom.Show("ggn", GGN_WIDTH, GGN_HEIGHT, GGN_GRAVITY);
			if (ggncount > 0)
			{
				AAds.Custom.SetPadding("ggnbadge", GGN_BADGE_PADDING_LEFT, GGN_BADGE_PADDING_TOP, GGN_BADGE_PADDING_RIGHT, GGN_BADGE_PADDING_BOTTOM);
				AAds.Custom.Show("ggnbadge", GGN_BADGE_WIDTH, GGN_BADGE_HEIGHT, GGN_BADGE_GRAVITY);
			}
		}
	}

	public static void HideGGN()
	{
		Log("GWalletHelper.HideGGN()");
		if (IsGGNAvailable())
		{
			AAds.Custom.Hide("ggn");
			AAds.Custom.Hide("ggnbadge");
		}
	}

	public static void GoVIP()
	{
		Log("GWalletHelper.GoVIP()");
		if (IsGGNAvailable() && IsSubscriptionRecommendationAvailable())
		{
			string storeSkuCode = subscription_recommendations[highest_recommended_index].m_storeSkuCode;
			AInAppPurchase.RequestPurchase(storeSkuCode, "subscription");
		}
	}

	public static void LaunchGGN()
	{
		Log("GWalletHelper.LaunchGGN()");
		if (IsGGNAvailable())
		{
			string text = "http://m.glu.com/android/";
			text += "ghome?";
			text = text + "udid=" + ((!GWallet.IsDeviceSupported()) ? "null" : GWallet.GetAccountEmail());
			text = text + "&src=" + SRC;
			text = text + "&deviceid=" + SystemInfo.deviceUniqueIdentifier;
			text = text + "&account_id=" + ((!GWallet.IsDeviceSupported()) ? "null" : Convert.ToString(GWallet.GetAccountId()));
			text = text + "&p=" + PLATFORM;
			Log("Launching GGN URL: " + text);
			Application.OpenURL(text);
		}
	}

	public static void LaunchGGNBenefits()
	{
		Log("GWalletHelper.LaunchGGNBenefits()");
		if (IsGGNAvailable())
		{
			string text = "http://m.glu.com/android/";
			text += "vip-benefits?";
			text += "navbar=N";
			text = text + "&udid=" + ((!GWallet.IsDeviceSupported()) ? "null" : GWallet.GetAccountEmail());
			text = text + "&src=" + SRC;
			text = text + "&deviceid=" + SystemInfo.deviceUniqueIdentifier;
			text = text + "&account_id=" + ((!GWallet.IsDeviceSupported()) ? "null" : Convert.ToString(GWallet.GetAccountId()));
			text = text + "&p=" + PLATFORM;
			Log("Launching GGN Benefits URL: " + text);
			Application.OpenURL(text);
		}
	}

	private IEnumerator CheckGGN()
	{
		Log("GWalletHelper.CheckGGN()");
		if (!IsGGNAvailable())
		{
			yield break;
		}
		string url = "http://m.glu.com/android/";
		url += "get-unread-count?";
		url = url + "udid=" + ((!GWallet.IsDeviceSupported()) ? "null" : GWallet.GetAccountEmail());
		url = url + "&src=" + SRC;
		url = url + "&deviceid=" + SystemInfo.deviceUniqueIdentifier;
		url = url + "&account_id=" + ((!GWallet.IsDeviceSupported()) ? "null" : Convert.ToString(GWallet.GetAccountId()));
		url = url + "&p=" + PLATFORM;
		Log("GGN Checking Badge Count with URL: " + url);
		using (WWW www = new WWW(url))
		{
			yield return www;
			if (!www.isDone || !string.IsNullOrEmpty(www.error))
			{
				LogError("GGN Check Failed");
				yield break;
			}
			string response = www.text;
			IDictionary search = (IDictionary)Json.Deserialize(response);
			Log("GGN Response: " + response);
			Log("Found Count: " + search["total_count_value"]);
			if (search == null)
			{
				yield break;
			}
			ggncount = Convert.ToInt32(search["total_count_value"]);
			if (GGN_IMAGE != null && GGN_BADGE_IMAGE != null && ggncount > 0)
			{
				AAds.Custom.SetText("ggnbadge", Convert.ToString(ggncount), 2, 1, GGN_BADGE_WIDTH / 3, -1);
				if (AAds.Custom.IsActive("ggn"))
				{
					AAds.Custom.SetPadding("ggnbadge", GGN_BADGE_PADDING_LEFT, GGN_BADGE_PADDING_TOP, GGN_BADGE_PADDING_RIGHT, GGN_BADGE_PADDING_BOTTOM);
					AAds.Custom.Show("ggnbadge", GGN_BADGE_WIDTH, GGN_BADGE_HEIGHT, GGN_BADGE_GRAVITY);
				}
			}
		}
	}

	private void onCustomTap(string info)
	{
		string[] array = info.Split('|');
		string text = array[0];
		string text2 = array[1];
		float num = float.Parse(array[2]);
		float num2 = float.Parse(array[3]);
		Log("Unity: onCustomTap [" + text + "] " + text2 + " at (" + num + "," + num2 + ")");
		if (text.StartsWith("prestitial-iap-"))
		{
			int num3 = Convert.ToInt32(text.Substring(15));
			string storeSkuCode = subscription_recommendations[num3].m_storeSkuCode;
			GWallet.LogEvent("prestitial-" + storeSkuCode, "PRESTITIAL_CLICKTHROUGH");
			AInAppPurchase.RequestPurchase(storeSkuCode, "subscription");
			AAds.Custom.Hide("prestitial-iap-0");
			AAds.Custom.Hide("prestitial-iap-1");
			AAds.Custom.Hide("prestitial-benefits");
			AAds.Custom.Hide("prestitial-close");
			AAds.Custom.Hide("prestitial-bg");
			SendMessage("onGWalletDisplayDismissed", true);
		}
		else if (text.Equals("prestitial-benefits"))
		{
			GWallet.LogEvent("prestitial-benefits", "PRESTITIAL_CLICKTHROUGH");
			LaunchGGNBenefits();
		}
		else if (text.Equals("prestitial-close"))
		{
			AAds.Custom.Hide("prestitial-iap-0");
			AAds.Custom.Hide("prestitial-iap-1");
			AAds.Custom.Hide("prestitial-benefits");
			AAds.Custom.Hide("prestitial-close");
			AAds.Custom.Hide("prestitial-bg");
			SendMessage("onGWalletDisplayDismissed", true);
		}
		else if (text.Equals("LAUNCH") || text.Equals("STORE") || text.Equals("BANK"))
		{
			Log("Tapped on a " + activeNotificationType + " notification");
			if (activeNotificationType.Equals("ANNOUNCEMENT"))
			{
				GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
			}
			else if (activeNotificationType.Equals("INTERSTITIAL") || activeNotificationType.Equals("INCENTIVIZED_INTERSTITIAL"))
			{
				GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
				Application.OpenURL(activeNotificationURI);
			}
			else if (activeNotificationType.Equals("LAUNCH_GAME"))
			{
				GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
				string altURL = string.Empty;
				if (AJavaTools.Properties.IsBuildGoogle())
				{
					altURL = "market://details?id=" + activeNotificationURI;
				}
				else if (AJavaTools.Properties.IsBuildAmazon())
				{
					altURL = "amzn://apps/android?p=" + activeNotificationURI;
				}
				AJavaTools.Util.LaunchGame(activeNotificationURI, altURL);
			}
			else if (activeNotificationType.Equals("VGP"))
			{
				GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
				SendMessage("onGWalletLaunchItem", activeNotificationURI);
			}
			else if (activeNotificationType.Equals("IAP"))
			{
				GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
				AInAppPurchase.RequestPurchase(activeNotificationURI, string.Empty);
			}
			else if (activeNotificationType.Equals("SUBSCRIPTION_IAP"))
			{
				GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
				AInAppPurchase.RequestPurchase(activeNotificationURI, "subscription");
			}
			AAds.Custom.Hide("LAUNCH");
			AAds.Custom.Hide("BANK");
			AAds.Custom.Hide("STORE");
			AAds.Custom.Hide("LAUNCH-close");
			AAds.Custom.Hide("BANK-close");
			AAds.Custom.Hide("STORE-close");
			if (text.StartsWith("LAUNCH"))
			{
				location_launch_loaded = false;
			}
			if (text.StartsWith("STORE"))
			{
				location_store_loaded = false;
			}
			if (text.StartsWith("BANK"))
			{
				location_bank_loaded = false;
			}
			SendMessage("onGWalletDisplayDismissed", false);
		}
		else if (text.EndsWith("-close"))
		{
			AAds.Custom.Hide("LAUNCH");
			AAds.Custom.Hide("BANK");
			AAds.Custom.Hide("STORE");
			AAds.Custom.Hide("LAUNCH-close");
			AAds.Custom.Hide("BANK-close");
			AAds.Custom.Hide("STORE-close");
			if (text.StartsWith("LAUNCH"))
			{
				location_launch_loaded = false;
			}
			if (text.StartsWith("STORE"))
			{
				location_store_loaded = false;
			}
			if (text.StartsWith("BANK"))
			{
				location_bank_loaded = false;
			}
			SendMessage("onGWalletDisplayDismissed", false);
		}
		else if (text.StartsWith("ad-"))
		{
			int num4 = Convert.ToInt32(text.Substring(3));
			if (ad_recommendations[num4].m_attributeCount <= 0)
			{
				return;
			}
			GWallet.LogEvent("custom-" + ad_recommendations[num4].m_key[0], "AD_CLICKTHROUGH");
			if (ad_recommendations[num4].m_key[0].Equals("URL") || ad_recommendations[num4].m_key[0].Equals("INCENTIVIZED_URL"))
			{
				Application.OpenURL(ad_recommendations[num4].m_value[0]);
			}
			else if (ad_recommendations[num4].m_key[0].Equals("IAP"))
			{
				AInAppPurchase.RequestPurchase(ad_recommendations[num4].m_value[0], string.Empty);
			}
			else if (ad_recommendations[num4].m_key[0].Equals("SUBSCRIPTION_IAP"))
			{
				AInAppPurchase.RequestPurchase(ad_recommendations[num4].m_value[0], "subscription");
			}
			else if (ad_recommendations[num4].m_key[0].Equals("LAUNCH_GAME"))
			{
				string text3 = ad_recommendations[num4].m_value[0];
				string altURL2 = string.Empty;
				if (AJavaTools.Properties.IsBuildGoogle())
				{
					altURL2 = "market://details?id=" + text3;
				}
				else if (AJavaTools.Properties.IsBuildAmazon())
				{
					altURL2 = "amzn://apps/android?p=" + text3;
				}
				AJavaTools.Util.LaunchGame(text3, altURL2);
			}
			else if (ad_recommendations[num4].m_key[0].Equals("INGAME_ITEM"))
			{
				SendMessage("onGWalletLaunchItem", ad_recommendations[num4].m_value[0]);
			}
			else if (!ad_recommendations[num4].m_key[0].Equals("ANNOUNCEMENT"))
			{
				Log("Unknown Action Key: " + ad_recommendations[num4].m_key[0]);
			}
		}
		else if (text.Equals("ggn") || text.Equals("ggnbadge"))
		{
			LaunchGGN();
		}
	}

	private void onAnnouncementClick(string info)
	{
		Log("GWalletHelper.onAnnouncementClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
		}
	}

	private void onInterstitialClick(string info)
	{
		Log("GWalletHelper.onInterstitialClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
			Application.OpenURL(activeNotificationURI);
		}
	}

	private void onLaunchGameClick(string info)
	{
		Log("GWalletHelper.onLaunchGameClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(Convert.ToString(activeNotificationID), "NOTIFICATION_CLICKTHROUGH");
			string altURL = string.Empty;
			if (AJavaTools.Properties.IsBuildGoogle())
			{
				altURL = "market://details?id=" + activeNotificationURI;
			}
			else if (AJavaTools.Properties.IsBuildAmazon())
			{
				altURL = "amzn://apps/android?p=" + activeNotificationURI;
			}
			AJavaTools.Util.LaunchGame(activeNotificationURI, altURL);
		}
	}

	private void onVGPClick(string info)
	{
		Log("GWalletHelper.onVGPClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
			SendMessage("onGWalletLaunchItem", activeNotificationURI);
		}
	}

	private void onIAPClick(string info)
	{
		Log("GWalletHelper.onIAPClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
			AInAppPurchase.RequestPurchase(activeNotificationURI, string.Empty);
		}
	}

	private void onSubscriptionClick(string info)
	{
		Log("GWalletHelper.onSubscriptionClick()");
		int num = Convert.ToInt32(info);
		if (num == -1)
		{
			GWallet.LogEvent(activeNotificationID + "-" + activeNotificationURI, "NOTIFICATION_CLICKTHROUGH");
			AInAppPurchase.RequestPurchase(activeNotificationURI, "subscription");
		}
	}

	private void onGWalletEvent(string rawCallbackString)
	{
		Log("Unity: onGWalletEvent( " + rawCallbackString + " )");
		GWCallbackResponse_Unity callbackResponseData = default(GWCallbackResponse_Unity);
		GWallet.ParseCallbackResponse(ref callbackResponseData, rawCallbackString);
		Log(string.Concat("Unity: onGWalletEvent: ", callbackResponseData.m_completedCallType, " ", callbackResponseData.m_completionStatus, " ", callbackResponseData.m_completionMessage));
		CheckBillingRecommendations();
		CheckNotifications();
		CheckAds();
	}

	public static void Log(string message)
	{
		if (Debug.isDebugBuild || AJavaTools.DeviceInfo.IsGluDebug())
		{
			Debug.Log("[GWH] " + message);
		}
	}

	public static void LogWarning(string message)
	{
		if (Debug.isDebugBuild || AJavaTools.DeviceInfo.IsGluDebug())
		{
			Debug.LogWarning("[GWH] " + message);
		}
	}

	public static void LogError(string message)
	{
		if (Debug.isDebugBuild || AJavaTools.DeviceInfo.IsGluDebug())
		{
			Debug.LogError("[GWH] " + message);
		}
	}

	private static byte[] GetBytes(string str)
	{
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	private static string GetString(byte[] bytes)
	{
		char[] array = new char[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return new string(array);
	}

	public static string Base64Encode(string toEncode)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(toEncode);
		return Convert.ToBase64String(bytes);
	}

	public static string Base64Decode(string encodedData)
	{
		byte[] bytes = Convert.FromBase64String(encodedData);
		return Encoding.Unicode.GetString(bytes);
	}

	public static string Decrypt(string cipherText, string password)
	{
		if (password.Length > 16)
		{
			password = password.Substring(0, 16);
		}
		else if (password.Length < 16)
		{
			password += "gluheartandroid!".Substring(0, 16 - password.Length);
		}
		byte[] bytes = GetBytes(cipherText);
		byte[] bytes2 = Encoding.ASCII.GetBytes(password);
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(bytes2, bytes2);
		MemoryStream stream = new MemoryStream(bytes);
		CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read);
		byte[] array = new byte[bytes.Length];
		cryptoStream.Read(array, 0, array.Length);
		return GetString(array);
	}

	public static string Encrypt(string clearText, string password)
	{
		if (password.Length > 16)
		{
			password = password.Substring(0, 16);
		}
		else if (password.Length < 16)
		{
			password += "gluheartandroid!".Substring(0, 16 - password.Length);
		}
		byte[] bytes = GetBytes(clearText);
		byte[] bytes2 = Encoding.ASCII.GetBytes(password);
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(bytes2, bytes2);
		MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		return GetString(memoryStream.ToArray());
	}
}
