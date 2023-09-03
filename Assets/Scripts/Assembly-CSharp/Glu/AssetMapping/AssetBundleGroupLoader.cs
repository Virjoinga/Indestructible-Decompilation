using Glu.AssetBundles;

namespace Glu.AssetMapping
{
	public class AssetBundleGroupLoader : IAssetGroupLoader
	{
		private string path;

		public AssetBundleGroupLoader(string path)
		{
			this.path = path;
		}

		public AssetGroup Load()
		{
			return (AssetGroup)Glu.AssetBundles.AssetBundles.Load(path, typeof(AssetGroup));
		}
	}
}
