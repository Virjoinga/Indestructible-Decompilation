using System.Xml.Serialization;

[XmlType("armor")]
public class ShopItemArmor : ShopItemGarage
{
	[XmlIgnore]
	public float Damage;

	public ShopItemArmor()
	{
		_itemType = ShopItemType.Armor;
	}

	public override bool IsRemovable()
	{
		return true;
	}

	public override ShopConfigItem Clone()
	{
		ShopItemArmor shopItemArmor = new ShopItemArmor();
		Override(shopItemArmor);
		return shopItemArmor;
	}
}
