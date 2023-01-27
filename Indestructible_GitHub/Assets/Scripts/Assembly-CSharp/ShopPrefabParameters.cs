using UnityEngine;

public static class ShopPrefabParameters
{
	public static void Initialize(ShopConfig shop)
	{
		foreach (ShopItem item in shop.items)
		{
			if (item.ItemType == ShopItemType.Vehicle)
			{
				Collect(item as ShopItemVehicle);
			}
			else if (item.ItemType == ShopItemType.Armor)
			{
				Collect(item as ShopItemArmor);
			}
			else if (item.ItemType == ShopItemType.Weapon)
			{
				Collect(item as ShopItemWeapon);
			}
		}
		Resources.UnloadUnusedAssets();
	}

	private static void Collect(ShopItemWeapon item)
	{
		GameObject gameObject = Resources.Load(item.prefab) as GameObject;
		if (gameObject == null)
		{
			return;
		}
		MainWeapon componentAlsoInChildren = MonoUtils.GetComponentAlsoInChildren<MainWeapon>(gameObject);
		if (componentAlsoInChildren == null)
		{
			return;
		}
		if (item.WeaponType == "thermal")
		{
			IDOTWeapon iDOTWeapon = componentAlsoInChildren as IDOTWeapon;
			if (iDOTWeapon == null)
			{
				return;
			}
			DOTWeaponImp dotInterface = iDOTWeapon.dotInterface;
			item.DamageBurn = dotInterface.GetBaseDamage() * BurningBuff._damage;
			item.DamageBurnDuration = dotInterface.GetBaseDuration();
		}
		item.ShotEnergyConsumption = componentAlsoInChildren.baseShotEnergyConsumption;
		item.ShotInterval = componentAlsoInChildren.baseFireInterval;
		item.Damage = componentAlsoInChildren.baseDamage;
	}

	private static void Collect(ShopItemArmor item)
	{
		Object @object = Resources.Load(item.ComponentPrefab);
		if (!(@object == null))
		{
			DamageBoostConf damageBoostConf = @object as DamageBoostConf;
			if (!(damageBoostConf == null))
			{
				item.Damage = damageBoostConf.DamageBoost;
			}
		}
	}

	private static void Collect(ShopItemVehicle item)
	{
		GameObject gameObject = Resources.Load(item.prefab) as GameObject;
		if (gameObject == null)
		{
			return;
		}
		Engine component = gameObject.GetComponent<Engine>();
		if (component == null)
		{
			return;
		}
		Vehicle component2 = gameObject.GetComponent<Vehicle>();
		if (!(component2 == null))
		{
			Destructible component3 = gameObject.GetComponent<Destructible>();
			if (!(component3 == null))
			{
				item.Speed = component.baseMaxSpeed;
				item.Health = component3.baseMaxHP;
				item.Energy = component2.baseMaxEnergy;
			}
		}
	}
}
