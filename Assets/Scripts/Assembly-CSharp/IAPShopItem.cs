using System.Xml.Serialization;

public abstract class IAPShopItem : IAPShopConfigItem
{
	[XmlAttribute("itemSprite")]
	public string ItemSprite;

	private bool _delivered;

	public bool IsDelivered()
	{
		return _delivered;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		IAPShopItem iAPShopItem = other as IAPShopItem;
		ShopItem.OverrideValue(ref iAPShopItem.ItemSprite, ItemSprite);
	}

	public override void Buy()
	{
		MonoSingleton<GameController>.Instance.SuspendBecauseOfIAP = true;
		_delivered = false;
		base.Buy();
	}

	public override void Deliver()
	{
		_delivered = true;
		MonoSingleton<Player>.Instance.Buy(this);
	}
}
