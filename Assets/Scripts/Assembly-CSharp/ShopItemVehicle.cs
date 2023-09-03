using System.Xml.Serialization;

[XmlType("vehicle")]
public class ShopItemVehicle : ShopItemGarage
{
	[XmlAttribute("bodyId")]
	public string BodyId;

	[XmlAttribute("weaponId")]
	public string WeaponId;

	[XmlAttribute("armorId")]
	public string ArmorId;

	[XmlElement("abilities")]
	public VehicleAbilities Abilities = new VehicleAbilities();

	[XmlIgnore]
	public float Speed;

	[XmlIgnore]
	public float Health;

	[XmlIgnore]
	public float Energy;

	public ShopItemVehicle()
	{
		_itemType = ShopItemType.Vehicle;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemVehicle shopItemVehicle = other as ShopItemVehicle;
		ShopItem.OverrideValue(ref shopItemVehicle.WeaponId, WeaponId);
		ShopItem.OverrideValue(ref shopItemVehicle.ArmorId, ArmorId);
		ShopItem.OverrideValue(ref shopItemVehicle.BodyId, BodyId);
		Abilities.Override(shopItemVehicle.Abilities);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemVehicle shopItemVehicle = new ShopItemVehicle();
		Override(shopItemVehicle);
		return shopItemVehicle;
	}
}
