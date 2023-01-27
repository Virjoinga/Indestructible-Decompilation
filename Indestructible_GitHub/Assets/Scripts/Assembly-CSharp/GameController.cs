using System;
using System.Collections;
using System.Collections.Generic;
using Glu.Kontagent;
using UnityEngine;

public class GameController : MonoSingleton<GameController>
{
	public delegate void SuspendResumeDelegate();

	public GameConfiguration Configuration = new GameConfiguration();

	public bool SuspendBecauseOfIAP;

	private bool _suspended;

	private bool _ready;

	private string _invitation;

	private List<string> processedIDs = new List<string>();

	private Queue PurchaseConfirmationQueue = new Queue();

	public event SuspendResumeDelegate suspendEvent;

	public event SuspendResumeDelegate resumeEvent;

	public void Reset()
	{
		_ready = false;
	}

	private void LoadConfiguration()
	{
		GameConfiguration gameConfiguration = GameConfiguration.Load();
		if (gameConfiguration != null)
		{
			Configuration = gameConfiguration;
		}
	}

	public void OnAssetBundlesReady()
	{
		_ready = true;
		LoadConfiguration();
		SpriteAtlasUtils.UnloadAll();
	}

	public void InitPushNotifications()
	{
		ANotificationManager.Init(string.Empty);
	}

	private IEnumerator WaitForDevicePushToken()
	{
		PushNotifications notifications = PushNotifications.GetInstance();
		while (notifications.GetDeviceToken() == null && notifications.GetRegistrationError() == null)
		{
			yield return null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Application.targetFrameRate = 60;
		MonoSingleton<AudioListenerController>.Instance.Create();
		AStats.Flurry.StartSession();
		AStats.Flurry.SetExtras("rooted", AJavaTools.DeviceInfo.IsDeviceRooted().ToString());
		AJavaTools.Util.LogEventOBB();
		AJavaTools.Util.LogEventDataRestored();
		AAds.Init();
		AAds.Tapjoy.Init(base.gameObject.name);
		AAds.PlayHaven.Init(base.gameObject.name);
		if (GameConstants.BuildType == "amazon")
		{
			AAds.Amazon.Init();
		}
		AInAppPurchase.Init(base.gameObject.name);
		ASocial.Init();
		if (GameConstants.BuildType == "amazon")
		{
			ASocial.Amazon.Init();
		}
		int num = Screen.width;
		int num2 = Screen.height;
		if (num < num2)
		{
			num = Screen.height;
			num2 = Screen.width;
		}
		float num3 = (float)num / 800f;
		float num4 = (float)num2 / 480f;
		GWalletHelper.SetupGGN("ggn_button", "ggn_badge", string.Empty);
		GWalletHelper.SetupGGNButton((int)(50f * num3), (int)(50f * num4), 51, (int)(154f * num3), (int)(6f * num4));
		GWalletHelper.SetupGGNBadge((int)(20f * num3), (int)(20f * num4), 53);
		GWalletHelper.Init(base.gameObject);
		Kontagent.StartSession(string.Empty);
		Kontagent.StartSession(string.Empty);
		InitPushNotifications();
		ASocial.Facebook.Init(string.Empty);
		AAds.ChartBoost.Init();
	}

	private void OnApplicationPause(bool pause)
	{
		_suspended = pause;
		if (_suspended)
		{
			OnSuspend();
			Kontagent.StopSession();
		}
		else
		{
			Kontagent.StartSession(string.Empty);
			OnRestore();
		}
	}

	private void Update()
	{
		if (!_ready)
		{
		}
	}

	public void EnableTapJoyPointsRetrieval()
	{
		AAds.Tapjoy.GetPoints();
	}

	public void DisableTapJoyPointsRetrieval()
	{
	}

	private void OnSuspend()
	{
		AStats.Flurry.EndSession();
		if (GameConstants.BuildType == "amazon")
		{
			ASocial.Amazon.SyncOnExit();
		}
		if (_ready)
		{
			GamePushNotifications.Setup();
		}
		if (this.suspendEvent != null)
		{
			this.suspendEvent();
		}
	}

	private void OnRestore()
	{
		AStats.Flurry.StartSession();
		ASocial.Facebook.ExtendAccess();
		if (_ready)
		{
			GamePushNotifications.Cancel();
		}
		if (this.resumeEvent != null)
		{
			this.resumeEvent();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AStats.Flurry.EndSession();
		AInAppPurchase.Destroy();
		Kontagent.StopSession();
		if (GameConstants.BuildType == "amazon")
		{
			ASocial.Amazon.SyncOnExit();
		}
	}

	private void onBillingSupported(string supported)
	{
		AInAppPurchase.BillingSupported = bool.Parse(supported);
		Debug.Log("Unity: onBillingSupported: " + supported);
	}

	private void onSubscriptionSupported(string supported)
	{
		AInAppPurchase.SubscriptionSupported = bool.Parse(supported);
		Debug.Log("Unity: onSubscriptionSupported: " + supported);
	}

	private void onGetUserIdResponse(string userID)
	{
		AInAppPurchase.UserID = userID;
		Debug.Log("Unity: onGetUserIdResponse: " + userID);
	}

	private void onPurchaseStateChange(string response)
	{
		lock (processedIDs)
		{
			Debug.Log("Unity: onPurchaseStateChange: " + response);
			string[] array = response.Split('|');
			string text = array[0];
			string text2 = array[1];
			string text3 = array[2];
			string text4 = array[3];
			string text5 = array[4];
			if (processedIDs.Contains(text))
			{
				return;
			}
			PurchaseConfirmationQueue.Enqueue(text);
			if (DialogIAPBuy.Instance != null)
			{
				DialogIAPBuy.Instance.Close();
			}
			if (text2.Equals("PURCHASED") || text2.Equals("SUCCESSFUL"))
			{
				Debug.Log("PURCHASED: " + text3 + ": " + text4);
				AStats.MobileAppTracking.TrackAction("iap_purchased");
				AStats.Flurry.LogEvent("IAP_PURCHASED", "product", text3, "device", (!AJavaTools.DeviceInfo.IsTablet()) ? "phone" : "tablet");
				if (!AInAppPurchase.RequestedIAP)
				{
					AStats.Flurry.LogEvent("IAP_RECOVERED", "product", text3, "device", (!AJavaTools.DeviceInfo.IsTablet()) ? "phone" : "tablet");
				}
				AAds.PlayHaven.ReportResolution(text3, 1, "buy");
				if (text4.ToLower().Equals("subscription"))
				{
					Debug.Log("PURCHASED SUBSCRIPTION: " + text5);
					GWallet.Subscribe(text5, AInAppPurchase.UserID);
					AStats.Flurry.LogEvent("SUBSCRIPTION_PURCHASED", text3);
					if (text3.ToLower().Contains("gold"))
					{
						AAds.Tapjoy.ActionComplete(AJavaTools.Properties.GetTapjoyPPASubscription());
					}
				}
				HandleAllPurchases(text3);
			}
			else if (text2.Equals("CANCELED"))
			{
				Debug.Log("CANCELED: " + text3 + ": " + text4);
				AAds.PlayHaven.ReportResolution(text3, 1, "error");
				if (GameConstants.BuildType == "google")
				{
					Dialogs.IAPFailed(true);
					if (DialogIAPBuy.Instance != null)
					{
						GameAnalytics.EventIAPItemCancelled(DialogIAPBuy.Instance.Item);
					}
				}
			}
			else if (text2.Equals("REFUNDED"))
			{
				Debug.Log("REFUNDED: " + text3 + ": " + text4);
				AAds.PlayHaven.ReportResolution(text3, 1, "error");
				HandleAllPurchases(text3);
				AStats.Flurry.LogEvent("REFUNDED", text3);
				AStats.MobileAppTracking.TrackAction("iap_purchased");
			}
			else if (text2.Equals("FAILED"))
			{
				AAds.PlayHaven.ReportResolution(text3, 1, "error");
			}
			AInAppPurchase.RequestedIAP = false;
			if (text != null && !text.Equals("null"))
			{
				processedIDs.Add(text);
			}
		}
	}

	public void ConfirmPurchase()
	{
		string text = (string)PurchaseConfirmationQueue.Dequeue();
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("*** PurchaseConfirmationQueue is EMPTY !! ***");
		}
		else
		{
			AInAppPurchase.ConfirmPurchase(text);
		}
	}

	private void HandleAllPurchases(string productId)
	{
		if (DialogIAPBuy.Instance == null)
		{
			ShopConfig shopConfig = MonoSingleton<ShopController>.Instance.LoadShop("Shop/idt_iaps");
			string text = productId.Replace("com.glu.indestructible.", "iap_");
			if (Debug.isDebugBuild)
			{
				Debug.Log("**** handling id " + text);
			}
			if (shopConfig == null)
			{
				Debug.LogError("*** unable to load iap xml ***");
				return;
			}
			IAPShopItemSimple iAPShopItemSimple = shopConfig.FindItemById(text) as IAPShopItemSimple;
			if (iAPShopItemSimple != null)
			{
				Debug.Log("*** delivering hc/sc ***");
				iAPShopItemSimple.Deliver();
				return;
			}
			if (Debug.isDebugBuild)
			{
				Debug.Log("**** handling boost item " + text);
			}
			IAPShopItemBoost iAPShopItemBoost = shopConfig.FindItemById(text) as IAPShopItemBoost;
			if (iAPShopItemBoost != null)
			{
				Debug.Log("*** delivering managed goods... ***");
				iAPShopItemBoost.Deliver();
			}
		}
		else
		{
			DialogIAPBuy.Instance.Item.Deliver();
		}
	}

	private void onRequestPurchaseResponse(string response)
	{
		Debug.Log("onRequestPurchaseResponse: " + response);
		if (response.Equals("RESULT_OK"))
		{
			Debug.Log("REQUEST BEING PROCESSED");
			return;
		}
		if (response.Equals("RESULT_USER_CANCELED"))
		{
			Debug.Log("REQUEST CANCELED BY USER");
			AAds.PlayHaven.ReportResolution(string.Empty, 1, "cancel");
			if (GameConstants.BuildType == "google")
			{
				if (DialogIAPBuy.Instance != null)
				{
					DialogIAPBuy.Instance.Close();
				}
				Dialogs.IAPFailed(true);
			}
			return;
		}
		Debug.Log("REQUEST ERRORED OUT");
		AAds.PlayHaven.ReportResolution(string.Empty, 1, "error");
		if (GameConstants.BuildType == "google")
		{
			if (DialogIAPBuy.Instance != null)
			{
				DialogIAPBuy.Instance.Close();
			}
			Dialogs.IAPCancelled();
		}
	}

	private void onRestoreTransactionsResponse(string response)
	{
		Debug.Log("onRestoreTransactionsResponse: " + response);
		if (response.Equals("RESULT_OK") || response.Equals("SUCCESSFUL"))
		{
			Debug.Log("TODO HANDLE RESTORE TRANSACTIONS SUCCESSFUL");
		}
		else
		{
			Debug.Log("TODO HANDLE RESTORE TRANSACTIONS FAILED");
		}
	}

	private void OnApplicationQuit()
	{
		QuitApplicaiton();
	}

	public void QuitApplicaiton()
	{
		GamePushNotifications.Setup();
		StartCoroutine(CoQuit());
	}

	private IEnumerator CoQuit()
	{
		yield return new WaitForSeconds(0.5f);
		Application.Quit();
	}

	public bool BackKeyReleased()
	{
		return Input.GetKeyUp(KeyCode.Escape);
	}

	private void onTapjoyPointsReceived(string info)
	{
		if (MonoSingleton<Player>.Exists())
		{
			int num = Convert.ToInt32(info);
			MonoSingleton<Player>.Instance.AddMoneyHard(num, "CREDIT_TAPJOY_AWARD", "Tapjoy", "TAPJOY");
			MonoSingleton<Player>.Instance.Save();
			AAds.Tapjoy.ClearPoints();
			GameAnalytics.EventTapJoyPointsReceived(num);
			Dialogs.TapJoyPointsReceived(num);
		}
	}

	private void onTapjoyOfferWallClosed(string info)
	{
		Debug.Log("Unity: onTapjoyOfferWallClosed: " + info);
		AAds.PlayHaven.Show("tj_closed");
	}

	private void onPlayHavenShouldMakePurchase(string info)
	{
		Debug.Log("Unity: onPlayHavenShouldMakePurchase: " + info);
		if (info.Equals("com.glu.tapjoy"))
		{
			AAds.Tapjoy.Launch(false);
			AAds.PlayHaven.ReportResolution(info, 1, "buy");
		}
		else
		{
			AInAppPurchase.RequestPurchase(info, string.Empty);
		}
	}

	public void StartPublisherContentRequest(string placement, bool showOverlayImmediately, bool showErrorMessage)
	{
		if (placement == "more_games" && GameConstants.BuildType == "amazon")
		{
			Application.OpenURL(AJavaTools.Internet.GetMoreGamesURL());
			return;
		}
		AAds.PlayHaven.Show(placement);
		if (placement == "game_launch")
		{
			AAds.PlayHaven.Show(GWallet.IsSubscriber() ? "subscriber" : "non_subscriber");
		}
	}
}
