using System.Xml.Serialization;

public class ShopItemGarage : ShopItem
{
	[XmlAttribute("garagePrefab")]
	public string GaragePrefab;

	[XmlAttribute("garageSprite")]
	public string GarageSprite;

	[XmlAttribute("vehicleId")]
	public string VehicleId;

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemGarage shopItemGarage = other as ShopItemGarage;
		ShopItem.OverrideValue(ref shopItemGarage.GarageSprite, GarageSprite);
		ShopItem.OverrideValue(ref shopItemGarage.GaragePrefab, GaragePrefab);
		ShopItem.OverrideValue(ref shopItemGarage.VehicleId, VehicleId);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemGarage shopItemGarage = new ShopItemGarage();
		Override(shopItemGarage);
		return shopItemGarage;
	}
}
