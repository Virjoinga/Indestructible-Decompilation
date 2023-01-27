public static class PhotonUtils
{
	public static void EnsureOffline()
	{
		PhotonNetwork.offlineMode = false;
		PhotonNetwork.Disconnect();
		PhotonNetwork.ConnectUsingSettings("1.0");
		PhotonNetwork.Disconnect();
	}
}
