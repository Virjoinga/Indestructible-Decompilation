using UnityEngine;

namespace Glu.AssetMapping
{
	public class ResourcesGroupLoader : IAssetGroupLoader
	{
		private string path;

		public ResourcesGroupLoader(string path)
		{
			this.path = path;
		}

		public AssetGroup Load()
		{
			return (AssetGroup)Resources.Load(path, typeof(AssetGroup));
		}
	}
}
