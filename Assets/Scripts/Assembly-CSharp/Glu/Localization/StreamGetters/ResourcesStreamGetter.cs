using System;
using System.IO;
using UnityEngine;

namespace Glu.Localization.StreamGetters
{
	public sealed class ResourcesStreamGetter : IStreamGetter
	{
		private string baseDir;

		public string BaseDir
		{
			get
			{
				return baseDir;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				baseDir = value;
			}
		}

		public ResourcesStreamGetter()
		{
			baseDir = string.Empty;
		}

		public Stream GetStream(string name)
		{
			string path = Path.ChangeExtension(name, null);
			path = Path.Combine(baseDir, path).Replace('\\', '/');
			TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
			if (textAsset != null)
			{
				return new MemoryStream(textAsset.bytes);
			}
			return null;
		}
	}
}
