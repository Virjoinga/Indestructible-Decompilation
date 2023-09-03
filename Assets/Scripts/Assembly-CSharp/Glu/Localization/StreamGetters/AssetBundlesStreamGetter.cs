using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Glu.Localization.StreamGetters
{
	public sealed class AssetBundlesStreamGetter : IStreamGetter
	{
		private string baseDir;

		private MethodInfo loadMethod;

		private MethodInfo containsMethod;

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

		public AssetBundlesStreamGetter()
		{
			Assembly assembly = typeof(AssetBundlesStreamGetter).Assembly;
			Type type = assembly.GetType("Glu.AssetBundles.AssetBundles");
			loadMethod = type.GetMethod("Load", new Type[2]
			{
				typeof(string),
				typeof(Type)
			});
			containsMethod = type.GetMethod("Contains", new Type[1] { typeof(string) });
		}

		public Stream GetStream(string name)
		{
			string text = Path.Combine(baseDir, name).Replace('\\', '/');
			if (!object.ReferenceEquals(containsMethod, null) && !(bool)containsMethod.Invoke(null, new object[1] { text }))
			{
				return null;
			}
			TextAsset textAsset = loadMethod.Invoke(null, new object[2]
			{
				text,
				typeof(TextAsset)
			}) as TextAsset;
			if (!object.ReferenceEquals(textAsset, null))
			{
				return new MemoryStream(textAsset.bytes);
			}
			return null;
		}
	}
}
