using System.Xml.Serialization;

public class VehicleAbilityPassive
{
	[XmlAttribute("nameId")]
	public string NameId;

	[XmlAttribute("descriptionId")]
	public string DescriptionId;

	[XmlAttribute("icon")]
	public string Icon;

	public void Override(VehicleAbilityPassive other)
	{
		ShopItem.OverrideValue(ref other.NameId, NameId);
		ShopItem.OverrideValue(ref other.DescriptionId, DescriptionId);
		ShopItem.OverrideValue(ref other.Icon, Icon);
	}
}
