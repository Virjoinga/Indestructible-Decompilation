using System.Xml.Serialization;

public class BossTextsConfig
{
	[XmlAttribute("description")]
	public string Description;

	[XmlAttribute("win")]
	public string Win;

	[XmlAttribute("defeat")]
	public string Defeat;
}
