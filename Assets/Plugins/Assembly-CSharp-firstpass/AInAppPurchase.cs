using UnityEngine;

public class AInAppPurchase : MonoBehaviour
{
	public static bool BillingSupported = true;

	public static bool SubscriptionSupported = true;

	public static string UserID;

	public static bool RequestedIAP;

	private static AndroidJavaClass _aiap;

	public static AndroidJavaClass aiap
	{
		get
		{
			if (_aiap == null)
			{
				_aiap = new AndroidJavaClass("com.glu.plugins.AInAppPurchase");
			}
			return _aiap;
		}
	}

	public static void Init(string gameObjectName)
	{
		AIAP_Init(gameObjectName, AJavaTools.Properties.GetBuildType(), Debug.isDebugBuild, AJavaTools.Properties.GetAppPublicKey());
	}

	public static void Register()
	{
		AIAP_Register();
	}

	public static void Unregister()
	{
		AIAP_Unregister();
	}

	public static void Destroy()
	{
		AIAP_Destroy();
	}

	public static void RequestPurchase(string itemId, string developerPayload = "")
	{
		RequestedIAP = true;
		AIAP_RequestPurchase(itemId, developerPayload);
	}

	public static void RequestPendingPurchases()
	{
		AIAP_RequestPendingPurchases();
	}

	public static void ConfirmPurchase(string purchaseID)
	{
		AIAP_ConfirmPurchase(purchaseID);
	}

	public static void RestoreTransactions()
	{
		AIAP_RestoreTransactions();
	}

	private static void AIAP_Init(string go, string type, bool debug, string publicKey)
	{
		aiap.CallStatic("Init", go, type, debug, publicKey);
	}

	private static void AIAP_Register()
	{
		aiap.CallStatic("Register");
	}

	private static void AIAP_Unregister()
	{
		aiap.CallStatic("Unregister");
	}

	private static void AIAP_Destroy()
	{
		aiap.CallStatic("Destroy");
	}

	private static void AIAP_RequestPurchase(string itemId, string developerPayload)
	{
		aiap.CallStatic("RequestPurchase", itemId, developerPayload);
	}

	private static void AIAP_RequestPendingPurchases()
	{
		aiap.CallStatic("RequestPendingPurchases");
	}

	private static void AIAP_ConfirmPurchase(string purchaseID)
	{
		aiap.CallStatic("ConfirmPurchase", purchaseID);
	}

	private static void AIAP_RestoreTransactions()
	{
		aiap.CallStatic("RestoreTransactions");
	}
}
