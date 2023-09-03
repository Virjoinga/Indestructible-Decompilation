using System.Xml.Serialization;

[XmlType("component")]
public class ShopItemComponent : ShopItem
{
	public ShopItemComponent()
	{
		_itemType = ShopItemType.Component;
	}

	public override bool IsRemovable()
	{
		return true;
	}

	public override ShopConfigItem Clone()
	{
		ShopItemComponent shopItemComponent = new ShopItemComponent();
		Override(shopItemComponent);
		return shopItemComponent;
	}
}
