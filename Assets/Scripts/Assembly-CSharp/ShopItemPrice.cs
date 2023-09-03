using System.Xml.Serialization;

[XmlType("price")]
public class ShopItemPrice : ShopItem
{
	public ShopItemPrice()
	{
		_itemType = ShopItemType.Price;
	}

	public override bool IsStorable()
	{
		return false;
	}

	public override ShopConfigItem Clone()
	{
		ShopItemPrice shopItemPrice = new ShopItemPrice();
		Override(shopItemPrice);
		return shopItemPrice;
	}
}
