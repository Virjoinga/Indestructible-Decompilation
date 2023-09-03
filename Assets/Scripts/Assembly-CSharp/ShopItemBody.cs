using System.Xml.Serialization;
using Glu.Localization;

[XmlType("body")]
public class ShopItemBody : ShopItemGarage
{
	[XmlAttribute("itemBackground")]
	public string ItemBackground;

	[XmlAttribute("personSprite")]
	public string PersonSprite;

	[XmlAttribute("personIcon")]
	public string PersonIcon;

	[XmlAttribute("personHead")]
	public string PersonHead;

	public ShopItemBody()
	{
		_itemType = ShopItemType.Body;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemBody shopItemBody = other as ShopItemBody;
		ShopItem.OverrideValue(ref shopItemBody.ItemBackground, ItemBackground);
		ShopItem.OverrideValue(ref shopItemBody.PersonSprite, PersonSprite);
		ShopItem.OverrideValue(ref shopItemBody.PersonIcon, PersonIcon);
		ShopItem.OverrideValue(ref shopItemBody.PersonHead, PersonHead);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemBody shopItemBody = new ShopItemBody();
		Override(shopItemBody);
		return shopItemBody;
	}

	public override bool IsLocked()
	{
		return !MonoSingleton<Player>.Instance.IsBought(VehicleId) || base.IsLocked();
	}

	public override bool IsLocked(ShopItemCurrency currency)
	{
		return !MonoSingleton<Player>.Instance.IsBought(VehicleId) || base.IsLocked(currency);
	}

	public override string LockText()
	{
		if (!MonoSingleton<Player>.Instance.IsBought(VehicleId))
		{
			return Strings.GetString("IDS_SHOP_ITEM_LOCK_VEHICLE");
		}
		return base.LockText();
	}
}
