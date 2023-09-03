using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Glu.Localization.Config
{
	public sealed class RuntimeConfigManager
	{
		private const string DefaultConfigResource = "Conf/localization";

		private static RuntimeConfig config;

		public static string DefaultConfigPath
		{
			get
			{
				string text = Path.Combine("Assets/Resources", "Conf/localization") + ".xml";
				return text.Replace('\\', '/');
			}
		}

		public static RuntimeConfig Instance
		{
			get
			{
				if (config == null)
				{
					LoadConfig();
				}
				return config;
			}
		}

		public static void ReplaceConfig(RuntimeConfig newConfig)
		{
			config = newConfig;
		}

		public static void LoadConfig()
		{
			RuntimeConfig runtimeConfig = new RuntimeConfig();
			try
			{
				using (Stream stream = GetDefaultConfigStream())
				{
					if (stream != null)
					{
						runtimeConfig.Load(stream);
					}
				}
			}
			catch (Exception ex)
			{
				if (!(ex is XmlException) && !(ex is InvalidConfigException))
				{
					throw;
				}
			}
			config = runtimeConfig;
		}

		private static Stream GetDefaultConfigStream()
		{
			TextAsset textAsset = (TextAsset)Resources.Load("Conf/localization", typeof(TextAsset));
			if (!object.ReferenceEquals(textAsset, null))
			{
				return new MemoryStream(textAsset.bytes);
			}
			return null;
		}
	}
}
