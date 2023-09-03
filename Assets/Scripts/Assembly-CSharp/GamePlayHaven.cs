public static class GamePlayHaven
{
	public static void Placement(string id)
	{
		MonoSingleton<GameController>.Instance.StartPublisherContentRequest(id, false, false);
	}

	public static void Placement(string id, bool showErrorMessage)
	{
		MonoSingleton<GameController>.Instance.StartPublisherContentRequest(id, false, false);
	}

	public static void IAPTracking(string productId, ICInAppPurchase.TRANSACTION_STATE state)
	{
	}
}
