using System.Xml.Serialization;

public class BossConfig
{
	[XmlAttribute("name")]
	public string Name;

	[XmlAttribute("prefab")]
	public string Prefab;
}
