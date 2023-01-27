using System;
using UnityEngine;

[Serializable]
public class ServerSettings : ScriptableObject
{
	public enum HostingOption
	{
		NotSet = 0,
		PhotonCloud = 1,
		SelfHosted = 2,
		OfflineMode = 3
	}

	public static string DefaultCloudServerUrl = "app.exitgamescloud.com";

	public static string DefaultServerAddress = "127.0.0.1";

	public static int DefaultMasterPort = 5055;

	public static string DefaultAppID = "Master";

	public HostingOption HostType;

	public string ServerAddress = DefaultServerAddress;

	public int ServerPort = 5055;

	public string AppID = string.Empty;

	[HideInInspector]
	public bool DisableAutoOpenWizard;

	public void UseCloud(string cloudAppid)
	{
		HostType = HostingOption.PhotonCloud;
		AppID = cloudAppid;
		ServerAddress = DefaultCloudServerUrl;
		ServerPort = DefaultMasterPort;
	}

	public void UseMyServer(string serverAddress, int serverPort, string application)
	{
		HostType = HostingOption.SelfHosted;
		AppID = ((application == null) ? DefaultAppID : application);
		ServerAddress = serverAddress;
		ServerPort = serverPort;
	}

	public override string ToString()
	{
		return string.Concat("ServerSettings: ", HostType, " ", ServerAddress);
	}
}
