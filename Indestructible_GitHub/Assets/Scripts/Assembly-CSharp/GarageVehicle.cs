public class GarageVehicle
{
	public ShopItemVehicle Vehicle;

	public ShopItemWeapon Weapon;

	public ShopItemArmor Armor;

	public ShopItemBody Body;

	public ShopItemComponent[] Components = new ShopItemComponent[3];

	public ShopItemAmmunition Ammunition;

	public GarageVehicleFuel Fuel = new GarageVehicleFuel();

	public int GetTotalPower()
	{
		int num = Vehicle.GetPower();
		ShopItemComponent[] components = Components;
		foreach (ShopItemComponent shopItemComponent in components)
		{
			if (shopItemComponent != null && shopItemComponent.GetPower() > 0)
			{
				num += shopItemComponent.GetPower();
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public int GetPower()
	{
		int num = 0;
		if (Armor != null)
		{
			num -= Armor.GetPower();
		}
		if (Weapon != null)
		{
			num -= Weapon.GetPower();
		}
		ShopItemComponent[] components = Components;
		foreach (ShopItemComponent shopItemComponent in components)
		{
			if (shopItemComponent != null && shopItemComponent.GetPower() < 0)
			{
				num -= shopItemComponent.GetPower();
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public int ContainsAt(ShopItem item)
	{
		if (item == null)
		{
			return -1;
		}
		if (item.ItemType == ShopItemType.Component)
		{
			for (int i = 0; i < Components.Length; i++)
			{
				ShopItemComponent shopItemComponent = Components[i];
				if (shopItemComponent != null && shopItemComponent.id == item.id)
				{
					return i;
				}
			}
		}
		else
		{
			ShopItem item2 = GetItem(item.ItemType, 0);
			if (item2 != null && item2.id == item.id)
			{
				return 0;
			}
		}
		return -1;
	}

	public bool Contains(ShopItem item)
	{
		int num = ContainsAt(item);
		return num != -1;
	}

	public ShopItem GetItem(ShopItemType type, int slot)
	{
		switch (type)
		{
		case ShopItemType.Vehicle:
			return Vehicle;
		case ShopItemType.Body:
			return Body;
		case ShopItemType.Weapon:
			return Weapon;
		case ShopItemType.Armor:
			return Armor;
		case ShopItemType.Component:
			return Components[slot];
		case ShopItemType.Ammunition:
			return Ammunition;
		default:
			return null;
		}
	}

	private ShopItem Replace<T>(ref T item, ShopItem with) where T : ShopItem
	{
		T result = item;
		item = with as T;
		return result;
	}

	public ShopItem Mount(ShopItem item, ShopItemType type, int slot)
	{
		switch (type)
		{
		case ShopItemType.Vehicle:
			return Replace(ref Vehicle, item);
		case ShopItemType.Body:
			return Replace(ref Body, item);
		case ShopItemType.Weapon:
		{
			ShopItem result = Replace(ref Weapon, item);
			if (Ammunition != null && !Ammunition.IsMatchedWeapon((ShopItemWeapon)item))
			{
				Ammunition = null;
			}
			return result;
		}
		case ShopItemType.Armor:
			return Replace(ref Armor, item);
		case ShopItemType.Component:
			return Replace(ref Components[slot], item);
		case ShopItemType.Ammunition:
			return Replace(ref Ammunition, item);
		default:
			return null;
		}
	}

	public bool CanMount(ShopItem item, int slot)
	{
		int num = 0;
		int power = item.GetPower();
		ShopItem item2 = GetItem(item.ItemType, slot);
		if (item2 != null)
		{
			num = item2.GetPower();
		}
		int power2 = GetPower();
		int totalPower = GetTotalPower();
		int num2 = power2 - power;
		if (num < 0)
		{
			num2 += num;
		}
		int num3 = totalPower;
		if (num > 0)
		{
			num3 -= num;
		}
		return power >= 0 || num3 >= num2;
	}
}
