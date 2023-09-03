using UnityEngine;

public static class NativeUtils
{
	public enum NetworkType
	{
		NetNone = 0,
		NetWiFi = 1,
		Net3G = 2
	}

	private static bool inited;

	private static float lastUpdate;

	private static int pss;

	private static AndroidJavaObject activityManager;

	private static int[] pids;

	public static bool IsNetworkAvailable()
	{
		int num = NetworkStatus();
		Debug.Log("NetworkStatus = " + num);
		return num != 0;
	}

	private static int NetworkStatus()
	{
		return 0;
	}

	public static void ReportMemory(string eventName)
	{
	}

	public static int GetCurrentMemoryBytes()
	{
		return MT_GetCurrentMemoryBytes();
	}

	private static int MT_GetCurrentMemoryBytes()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!inited)
		{
			inited = true;
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			activityManager = @static.Call<AndroidJavaObject>("getSystemService", new object[1] { "activity" });
			pids = new int[1] { new AndroidJavaClass("android.os.Process").CallStatic<int>("myPid", new object[0]) };
		}
		else if (realtimeSinceStartup < lastUpdate + 10f)
		{
			return pss;
		}
		lastUpdate = realtimeSinceStartup;
		AndroidJavaObject[] array = activityManager.Call<AndroidJavaObject[]>("getProcessMemoryInfo", new object[1] { pids });
		pss = 1024 * array[0].Call<int>("getTotalPss", new object[0]);
		return pss;
	}
}
