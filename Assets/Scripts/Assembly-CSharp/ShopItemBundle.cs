using System.Xml.Serialization;

[XmlType("bundle")]
public class ShopItemBundle : ShopItem
{
	public class Component
	{
		[XmlAttribute("id")]
		public string Id;
	}

	[XmlAttribute("durationHours")]
	public int? DurationHours;

	[XmlAttribute("vehicleId")]
	public string VehicleId;

	[XmlAttribute("bodyId")]
	public string BodyId;

	[XmlAttribute("weaponId")]
	public string WeaponId;

	[XmlAttribute("armorId")]
	public string ArmorId;

	[XmlAttribute("itemBackground")]
	public string ItemBackground;

	[XmlArrayItem("component")]
	[XmlArray("components")]
	public Component[] Components = new Component[3];

	public ShopItemBundle()
	{
		_itemType = ShopItemType.Bundle;
	}

	private void OverrideComponents(ref Component[] otherComponents)
	{
		int num = Components.Length;
		otherComponents = new Component[num];
		for (int i = 0; i < num; i++)
		{
			if (Components[i] != null)
			{
				otherComponents[i] = new Component();
				otherComponents[i].Id = Components[i].Id;
			}
		}
	}

	public int GetDurationHours()
	{
		int? durationHours = DurationHours;
		return durationHours.HasValue ? durationHours.Value : 0;
	}

	public override bool IsStorable()
	{
		return true;
	}

	public virtual bool IsOwned()
	{
		if (MonoSingleton<Player>.Instance.IsBought(id))
		{
			return true;
		}
		if (MonoSingleton<Player>.Instance.IsBought(VehicleId))
		{
			return true;
		}
		if (MonoSingleton<Player>.Instance.IsBought(WeaponId))
		{
			return true;
		}
		return false;
	}

	public virtual bool CanActivate()
	{
		if (base.IsLocked())
		{
			return false;
		}
		if (IsOwned())
		{
			return false;
		}
		return true;
	}

	public override bool IsLocked()
	{
		return false;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemBundle shopItemBundle = other as ShopItemBundle;
		ShopItem.OverrideValue(ref shopItemBundle.VehicleId, VehicleId);
		ShopItem.OverrideValue(ref shopItemBundle.BodyId, BodyId);
		ShopItem.OverrideValue(ref shopItemBundle.WeaponId, WeaponId);
		ShopItem.OverrideValue(ref shopItemBundle.ArmorId, ArmorId);
		ShopItem.OverrideValue(ref shopItemBundle.ItemBackground, ItemBackground);
		OverrideComponents(ref shopItemBundle.Components);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemBundle shopItemBundle = new ShopItemBundle();
		Override(shopItemBundle);
		return shopItemBundle;
	}
}
