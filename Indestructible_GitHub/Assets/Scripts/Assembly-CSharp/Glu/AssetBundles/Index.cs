using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Glu.AssetBundles.Internal;

namespace Glu.AssetBundles
{
	[XmlType("Index")]
	public class Index
	{
		[XmlType("AssetBundle")]
		public class AssetBundle
		{
			[XmlType("Asset")]
			public class Asset
			{
				[XmlAttribute("Filename")]
				public string m_filename;

				[XmlAttribute("GUID")]
				public string m_guid;

				[XmlAttribute("Hash")]
				public string m_hash;

				public Asset()
				{
					m_filename = null;
					m_guid = null;
					m_hash = null;
				}
			}

			public enum Type
			{
				None = 0,
				Asset = 1,
				Scene = 2
			}

			[XmlAttribute("Filename")]
			public string m_filename;

			[XmlAttribute("Type")]
			public Type m_type;

			[XmlAttribute("Compressed")]
			public bool? m_isCompressed;

			[XmlAttribute("Size")]
			public long? m_size;

			[XmlAttribute("ContentHash")]
			public string m_contentHash;

			[XmlArray("Urls")]
			[XmlArrayItem("URL")]
			public string[] m_urls;

			[XmlArrayItem("Asset")]
			[XmlArray("Assets")]
			public List<Asset> m_assets;

			[XmlIgnore]
			public string m_editableFilename;

			[XmlIgnore]
			public bool m_wasDiffProcessed;

			[XmlIgnore]
			public bool m_wasAssetsSubstituted;

			public bool IsCompressed
			{
				get
				{
					return (!m_isCompressed.HasValue) ? (!RTUtils.UncompressedAssetBundlesAllowed) : m_isCompressed.Value;
				}
			}

			public long Size
			{
				get
				{
					return (!m_size.HasValue) ? 0 : m_size.Value;
				}
			}

			public AssetBundle()
			{
				m_filename = string.Empty;
				m_type = Type.None;
				m_isCompressed = null;
				m_size = null;
				m_contentHash = null;
				m_urls = null;
				m_assets = new List<Asset>();
				m_assets.Clear();
				m_editableFilename = null;
				m_wasDiffProcessed = false;
				m_wasAssetsSubstituted = false;
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.AssetBundles.Index");
			}
		}

		public static string m_overridenFilenameMask;

		[XmlAttribute("Filename")]
		public string m_filename;

		[XmlAttribute("BuildTag")]
		public string m_buildTag;

		[XmlArrayItem("AssetBundle")]
		[XmlArray("AssetBundles")]
		public List<AssetBundle> m_assetBundles;

		[XmlIgnore]
		public string m_xmlFilename;

		[XmlIgnore]
		public string m_editableFilename;

		[XmlIgnore]
		public Index m_doppelganger;

		private static XmlWriterSettings m_xmlWriterSettings;

		private static XmlSerializer m_xmlSerializer;

		private static XmlWriterSettings xmlWriterSettings
		{
			get
			{
				if (m_xmlWriterSettings != null)
				{
					return m_xmlWriterSettings;
				}
				m_xmlWriterSettings = new XmlWriterSettings();
				m_xmlWriterSettings.Encoding = Encoding.UTF8;
				m_xmlWriterSettings.Indent = true;
				m_xmlWriterSettings.IndentChars = "\t";
				m_xmlWriterSettings.NewLineChars = "\r\n";
				m_xmlWriterSettings.NewLineHandling = NewLineHandling.None;
				m_xmlWriterSettings.OmitXmlDeclaration = true;
				return m_xmlWriterSettings;
			}
		}

		private static XmlSerializer xmlSerializer
		{
			get
			{
				if (m_xmlSerializer != null)
				{
					return m_xmlSerializer;
				}
				m_xmlSerializer = new XmlSerializer(typeof(Index));
				return m_xmlSerializer;
			}
		}

		public Index()
		{
			m_filename = "index";
			m_buildTag = null;
			m_assetBundles = new List<AssetBundle>();
			m_assetBundles.Clear();
			m_xmlFilename = null;
			m_editableFilename = null;
			m_doppelganger = null;
		}

		static Index()
		{
			m_overridenFilenameMask = null;
		}

		public static Index CreateInstance()
		{
			return new Index();
		}

		public static Index DuplicateInstance(Index srcIndex)
		{
			Index result = null;
			if (srcIndex != null)
			{
				result = new Index();
				result.m_filename = srcIndex.m_filename;
				result.m_buildTag = srcIndex.m_buildTag;
				result.m_assetBundles = new List<AssetBundle>();
				result.m_assetBundles.Clear();
				{
					foreach (AssetBundle assetBundle2 in srcIndex.m_assetBundles)
					{
						AssetBundle assetBundle = new AssetBundle();
						assetBundle.m_filename = assetBundle2.m_filename;
						assetBundle.m_type = assetBundle2.m_type;
						assetBundle.m_isCompressed = assetBundle2.m_isCompressed;
						assetBundle.m_size = assetBundle2.m_size;
						assetBundle.m_contentHash = assetBundle2.m_contentHash;
						if (assetBundle2.m_urls != null)
						{
							assetBundle.m_urls = new string[assetBundle2.m_urls.Length];
							Array.Copy(assetBundle2.m_urls, assetBundle.m_urls, assetBundle2.m_urls.Length);
						}
						else
						{
							assetBundle.m_urls = null;
						}
						assetBundle.m_assets = new List<AssetBundle.Asset>();
						assetBundle.m_assets.Clear();
						foreach (AssetBundle.Asset asset2 in assetBundle2.m_assets)
						{
							AssetBundle.Asset asset = new AssetBundle.Asset();
							asset.m_filename = asset2.m_filename;
							asset.m_guid = asset2.m_guid;
							asset.m_hash = asset2.m_hash;
							assetBundle.m_assets.Add(asset);
						}
						result.m_assetBundles.Add(assetBundle);
					}
					return result;
				}
			}
			return result;
		}

		public static Index LoadInstance(Stream stream)
		{
			Index index = null;
			try
			{
				return xmlSerializer.Deserialize(stream) as Index;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void SaveInstance(Stream stream)
		{
			using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
			{
				xmlSerializer.Serialize(xmlWriter, this);
			}
		}
	}
}
