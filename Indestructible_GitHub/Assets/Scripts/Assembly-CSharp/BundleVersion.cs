using UnityEngine;

public class BundleVersion
{
	private static string _bundleVersion;

	public static string Get()
	{
		if (_bundleVersion == null)
		{
			_bundleVersion = AJavaTools.GameInfo.GetVersionName();
			if (Debug.isDebugBuild)
			{
				_bundleVersion += "D";
			}
			_bundleVersion = _bundleVersion + "\n" + AJavaTools.Properties.GetBuildTag();
		}
		return _bundleVersion;
	}
}
