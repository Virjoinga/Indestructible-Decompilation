using System.Xml.Serialization;

public class BossCompareScreenInfo
{
	[XmlAttribute("damage")]
	public float Damage;

	[XmlAttribute("armor")]
	public float Armor;

	[XmlAttribute("speed")]
	public float Speed;

	[XmlAttribute("energy")]
	public float Energy;
}
