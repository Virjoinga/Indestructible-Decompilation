using UnityEngine;

public class DialogIAPBuy : UIDialog
{
	private static float iapTimeOut = Time.realtimeSinceStartup;

	private static DialogIAPBuy instance = null;

	public IAPShopItem Item;

	public static DialogIAPBuy Instance
	{
		get
		{
			return instance;
		}
	}

	private new void Awake()
	{
		instance = this;
		iapTimeOut = Time.realtimeSinceStartup;
	}

	private void OnApplicationPuase(bool Paused)
	{
		if (!Paused)
		{
			iapTimeOut = Time.realtimeSinceStartup;
		}
	}

	public void OnDestroy()
	{
		MonoSingleton<GameController>.Instance.SuspendBecauseOfIAP = false;
	}

	private void Update()
	{
		float num = 60f;
		if (Debug.isDebugBuild)
		{
			Debug.Log("Time lapsed : " + (Time.realtimeSinceStartup - iapTimeOut));
			num = 15f;
		}
		if (Time.realtimeSinceStartup - iapTimeOut > num)
		{
			Dialogs.IAPCancelled();
			AStats.Flurry.LogEvent("IAPTimedOut", Item.productId);
			Close();
		}
	}

	private void IAPTracking(ICInAppPurchase.TRANSACTION_STATE state)
	{
		GamePlayHaven.IAPTracking(Item.productId, state);
	}
}
