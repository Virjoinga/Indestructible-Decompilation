using UnityEngine;

namespace Glu.UnityBuildSystem
{
	public class BuildInfo
	{
		private static string _buildTag;

		private static string _bundleId;

		private static string _bundleVersion;

		public static string buildTag
		{
			get
			{
				if (string.IsNullOrEmpty(_buildTag))
				{
					TextAsset textAsset = Resources.Load("build_tag") as TextAsset;
					if (textAsset != null)
					{
						_buildTag = textAsset.text.Trim();
					}
					else
					{
						_buildTag = "NOT_TAGGED";
					}
				}
				return _buildTag;
			}
		}

		public static string bundleId
		{
			get
			{
				return GetBundleId();
			}
		}

		public static string bundleVersion
		{
			get
			{
				return GetBundleVersion();
			}
		}

		private static string GetBundleId()
		{
			if (string.IsNullOrEmpty(_bundleId))
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						_bundleId = androidJavaObject.Call<string>("getPackageName", new object[0]);
					}
				}
			}
			return _bundleId;
		}

		private static string GetBundleVersion()
		{
			if (string.IsNullOrEmpty(_bundleVersion))
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						using (AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getPackageManager", new object[0]))
						{
							using (AndroidJavaObject androidJavaObject3 = androidJavaObject.Call<AndroidJavaObject>("getPackageName", new object[0]))
							{
								using (AndroidJavaObject androidJavaObject4 = androidJavaObject2.Call<AndroidJavaObject>("getPackageInfo", new object[2] { androidJavaObject3, 0 }))
								{
									_bundleVersion = androidJavaObject4.Get<string>("versionName");
								}
							}
						}
					}
				}
			}
			return _bundleVersion;
		}
	}
}
