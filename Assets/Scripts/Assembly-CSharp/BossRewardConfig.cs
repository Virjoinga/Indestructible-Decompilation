using System.Xml.Serialization;

public class BossRewardConfig
{
	[XmlAttribute("moneySoft")]
	public int? MoneySoft;

	[XmlAttribute("experience")]
	public int? Experience;

	[XmlAttribute("bundleId")]
	public string BundleId;
}
