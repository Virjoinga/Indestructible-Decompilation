using System.Xml.Serialization;

[XmlType("ammunition")]
public class ShopItemAmmunition : ShopItem
{
	[XmlAttribute("weaponType")]
	public string WeaponType;

	[XmlAttribute("ammunitionSubname")]
	public string AmmunitionSubname = string.Empty;

	public ShopItemAmmunition()
	{
		_itemType = ShopItemType.Ammunition;
	}

	public override bool IsRemovable()
	{
		return true;
	}

	public override ShopConfigItem Clone()
	{
		ShopItemAmmunition shopItemAmmunition = new ShopItemAmmunition();
		Override(shopItemAmmunition);
		return shopItemAmmunition;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemAmmunition shopItemAmmunition = other as ShopItemAmmunition;
		ShopItem.OverrideValue(ref shopItemAmmunition.AmmunitionSubname, AmmunitionSubname);
		ShopItem.OverrideValue(ref shopItemAmmunition.WeaponType, WeaponType);
	}

	public bool IsMatchedWeapon(ShopItemWeapon weapon)
	{
		if (weapon == null)
		{
			return false;
		}
		return string.IsNullOrEmpty(WeaponType) || WeaponType.Contains(weapon.WeaponType);
	}

	public string ConfigureWeapon(ShopItemWeapon weapon)
	{
		if (IsMatchedWeapon(weapon))
		{
			return string.Format("{0}{1}", weapon.prefab, AmmunitionSubname);
		}
		return weapon.prefab;
	}
}
