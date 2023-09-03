using System.IO;
using UnityEngine;

namespace Glu.Localization.StreamGetters
{
	public sealed class AssetBundleStreamGetter : IStreamGetter
	{
		private AssetBundle assetBundle;

		private string baseDir;

		public string BaseDir
		{
			get
			{
				return baseDir;
			}
			set
			{
				baseDir = value;
			}
		}

		public AssetBundleStreamGetter(AssetBundle assetBundle)
		{
			this.assetBundle = assetBundle;
			baseDir = string.Empty;
		}

		public Stream GetStream(string name)
		{
			string name2 = Path.Combine(baseDir, name).Replace('\\', '/');
			TextAsset textAsset = assetBundle.Load(name2, typeof(TextAsset)) as TextAsset;
			if (!object.ReferenceEquals(textAsset, null))
			{
				return new MemoryStream(textAsset.bytes);
			}
			return null;
		}
	}
}
