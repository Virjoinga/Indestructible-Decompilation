using System.Xml.Serialization;

[XmlType("fight")]
public class BossFightConfig
{
	[XmlAttribute("id")]
	public string Id;

	[XmlAttribute("mapName")]
	public string MapName;

	[XmlAttribute("shopVehicleBody")]
	public string ShopVehicleBody;

	[XmlAttribute("backgroundImage")]
	public string BackgroundImage;

	[XmlAttribute("bossName")]
	public string BossName;

	[XmlAttribute("gameConfig")]
	public string GameConfig;

	[XmlAttribute("waveNumber")]
	public int WaveNumber;

	[XmlElement("compareInfo")]
	public BossCompareScreenInfo CompareInfo;

	[XmlElement("reward")]
	public BossRewardConfig Reward;

	[XmlElement("rewardFail")]
	public BossRewardConfig RewardFail;

	[XmlElement("texts")]
	public BossTextsConfig Texts;

	[XmlArrayItem("boss")]
	[XmlArray("bosses")]
	public BossConfig[] Bosses;
}
