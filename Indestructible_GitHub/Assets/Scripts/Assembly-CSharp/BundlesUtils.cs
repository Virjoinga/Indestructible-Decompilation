using Glu.AssetBundles;
using UnityEngine;

public static class BundlesUtils
{
	public static Object Load(string path)
	{
		return AssetBundles.Load(path);
	}

	public static Object LoadFromResources(string path)
	{
		path = path.Replace("Assets/", string.Empty);
		int num = path.LastIndexOf(".");
		if (num != -1)
		{
			path = path.Substring(0, num);
		}
		return Resources.Load(path);
	}
}
