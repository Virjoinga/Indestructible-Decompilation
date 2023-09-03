using System.Xml.Serialization;

namespace Glu.Localization.Config
{
	public struct PoLocation
	{
		[XmlElement("Type")]
		public ResourceSource Source { get; set; }

		[XmlElement("BaseDir")]
		public string BaseDirectory { get; set; }
	}
}
