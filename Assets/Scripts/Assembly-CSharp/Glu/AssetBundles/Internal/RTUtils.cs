using System;
using System.Text;
using UnityEngine;

namespace Glu.AssetBundles.Internal
{
	public static class RTUtils
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.Utils");
			}
		}

		private static Version m_unityVersion;

		public static Version UnityVersion
		{
			get
			{
				if (m_unityVersion != null)
				{
					return m_unityVersion;
				}
				string[] array = Application.unityVersion.Split('.');
				int[] array2 = new int[array.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					int num = array[i].Length;
					while (num > 0)
					{
						try
						{
							array2[i] = (int)Convert.ChangeType(array[i].Substring(0, num), typeof(int));
						}
						catch (Exception)
						{
							num--;
							continue;
						}
						break;
					}
				}
				int major = array2[0];
				int minor = array2[1];
				int build = array2[2];
				m_unityVersion = new Version(major, minor, build);
				return m_unityVersion;
			}
		}

		public static bool UncompressedAssetBundlesAllowed
		{
			get
			{
				return UnityVersion >= new Version("3.5.6");
			}
		}

		public static int HashToVersion(string hash)
		{
			byte[] bytes = Encoding.Default.GetBytes(hash);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 4; j + i < bytes.Length; j += 4)
				{
					bytes[i] ^= bytes[i + j];
				}
			}
			return ((bytes[0] & 0x7F) << 24) | ((bytes[1] & 0xFF) << 16) | ((bytes[2] & 0xFF) << 8) | (bytes[3] & 0xFF);
		}

		public static bool IsHash(string hash)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(hash))
			{
				hash = hash.ToLower();
				flag = true;
				for (int i = 0; i < hash.Length; i++)
				{
					switch (hash[i])
					{
					default:
						flag = false;
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case 'a':
					case 'b':
					case 'c':
					case 'd':
					case 'e':
					case 'f':
						break;
					}
					if (!flag)
					{
						break;
					}
				}
			}
			return flag;
		}
	}
}
