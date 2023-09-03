using System.Xml.Serialization;

namespace Glu.Localization.Config
{
	[XmlRoot("Localization")]
	public sealed class RuntimeConfigData
	{
		[XmlAttribute]
		public int Version;

		[XmlArray("Sources")]
		[XmlArrayItem("Source")]
		public PoLocation[] Locations;

		[XmlElement("BaseDir")]
		public string BaseDirectory;
	}
}
