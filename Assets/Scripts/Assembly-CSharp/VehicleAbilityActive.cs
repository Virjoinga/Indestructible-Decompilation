using System.Xml.Serialization;

public class VehicleAbilityActive
{
	[XmlAttribute("nameId")]
	public string NameId;

	[XmlAttribute("descriptionId")]
	public string DescriptionId;

	[XmlAttribute("icon")]
	public string Icon;

	[XmlAttribute("iconBuff")]
	public string IconBuff;

	public void Override(VehicleAbilityActive other)
	{
		ShopItem.OverrideValue(ref other.NameId, NameId);
		ShopItem.OverrideValue(ref other.DescriptionId, DescriptionId);
		ShopItem.OverrideValue(ref other.Icon, Icon);
		ShopItem.OverrideValue(ref other.IconBuff, IconBuff);
	}
}
