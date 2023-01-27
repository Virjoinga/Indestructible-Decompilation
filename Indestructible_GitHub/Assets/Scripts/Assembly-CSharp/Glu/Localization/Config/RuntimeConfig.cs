using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Glu.Localization.Config
{
	public sealed class RuntimeConfig
	{
		[Obsolete]
		public string BaseDirectory { get; set; }

		public IList<PoLocation> Locations { get; private set; }

		public RuntimeConfig()
		{
			Locations = new List<PoLocation>();
		}

		public void Reset()
		{
			Locations.Clear();
		}

		public void Load(Stream stream)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(RuntimeConfigData));
			RuntimeConfigData runtimeConfigData = (RuntimeConfigData)xmlSerializer.Deserialize(stream);
			Validate(runtimeConfigData);
			List<PoLocation> list = new List<PoLocation>();
			if (runtimeConfigData.Locations != null)
			{
				PoLocation[] locations = runtimeConfigData.Locations;
				foreach (PoLocation poLocation in locations)
				{
					PoLocation item = poLocation;
					if (runtimeConfigData.Version == 1 && item.Source == ResourceSource.Resources)
					{
						item.BaseDirectory = runtimeConfigData.BaseDirectory;
					}
					if (item.BaseDirectory == null)
					{
						item.BaseDirectory = string.Empty;
					}
					list.Add(item);
				}
			}
			Locations = list;
		}

		public void Save(Stream stream)
		{
			RuntimeConfigData runtimeConfigData = new RuntimeConfigData();
			runtimeConfigData.Version = 2;
			runtimeConfigData.Locations = Locations.ToArray();
			Validate(runtimeConfigData);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Encoding = Encoding.UTF8;
			xmlWriterSettings.Indent = true;
			using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(RuntimeConfigData));
				xmlSerializer.Serialize(xmlWriter, runtimeConfigData);
			}
		}

		private void Validate(RuntimeConfigData data)
		{
			if (data.Version != 1 && data.Version != 2)
			{
				throw new InvalidConfigException(string.Format("Unsupported config version {0}", data.Version));
			}
		}
	}
}
