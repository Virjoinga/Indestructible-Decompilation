using System.Xml.Serialization;

[XmlType("boost")]
public class IAPShopItemBoost : IAPShopItem
{
	public override ShopConfigItem Clone()
	{
		IAPShopItemBoost iAPShopItemBoost = new IAPShopItemBoost();
		Override(iAPShopItemBoost);
		return iAPShopItemBoost;
	}
}
