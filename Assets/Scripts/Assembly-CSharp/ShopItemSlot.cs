using System.Xml.Serialization;

[XmlType("slot")]
public class ShopItemSlot : ShopItem
{
	public ShopItemSlot()
	{
		_itemType = ShopItemType.Slot;
	}

	public override bool IsStorable()
	{
		return false;
	}

	public override ShopConfigItem Clone()
	{
		ShopItemSlot shopItemSlot = new ShopItemSlot();
		Override(shopItemSlot);
		return shopItemSlot;
	}
}
