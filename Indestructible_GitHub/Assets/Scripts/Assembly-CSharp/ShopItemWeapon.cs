using System.Xml.Serialization;
using Glu.Localization;

[XmlType("weapon")]
public class ShopItemWeapon : ShopItemGarage
{
	public const string Kinetic = "kinetic";

	public const string Thermal = "thermal";

	public const string Explosive = "explosive";

	[XmlAttribute("weaponType")]
	public string WeaponType;

	[XmlIgnore]
	public float ShotEnergyConsumption;

	[XmlIgnore]
	public float ShotInterval;

	[XmlIgnore]
	public float DamageBurnDuration;

	[XmlIgnore]
	public float DamageBurn;

	[XmlIgnore]
	public float Damage;

	public ShopItemWeapon()
	{
		_itemType = ShopItemType.Weapon;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemWeapon shopItemWeapon = other as ShopItemWeapon;
		ShopItem.OverrideValue(ref shopItemWeapon.WeaponType, WeaponType);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemWeapon shopItemWeapon = new ShopItemWeapon();
		Override(shopItemWeapon);
		return shopItemWeapon;
	}

	public string GetDamagePerSecondString()
	{
		string @string = Strings.GetString("IDS_WEAPON_DAMAGE_SEC");
		return string.Format(@string, NumberFormat.Get(Damage / ShotInterval, 1));
	}

	public string GetEnergyConsumptionPerSecondString()
	{
		string @string = Strings.GetString("IDS_WEAPON_ENERGY_CONSUMPTION_SEC");
		return string.Format(@string, NumberFormat.Get(ShotEnergyConsumption / ShotInterval, 1));
	}

	public string GetRateOfFirePerSecondString()
	{
		if (WeaponType == "thermal")
		{
			return Strings.GetString("IDS_WEAPON_RATE_OF_FIRE_CONTINUOUS");
		}
		string @string = Strings.GetString("IDS_WEAPON_RATE_OF_FIRE_SEC");
		return string.Format(@string, NumberFormat.Get(1f / ShotInterval, 1));
	}

	public string GetBurnDamageString()
	{
		string @string = Strings.GetString("IDS_WEAPON_BURN_DAMAGE_TEXT");
		string arg = NumberFormat.Get(DamageBurnDuration, 1);
		string arg2 = NumberFormat.Get(DamageBurn, 1);
		return string.Format(@string, arg2, arg);
	}
}
