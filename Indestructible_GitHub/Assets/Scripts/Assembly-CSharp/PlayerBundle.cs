using System.Collections.Generic;

public class PlayerBundle
{
	public enum FillingFlags
	{
		Vehicle = 1,
		Body = 2,
		Weapon = 4,
		Armor = 8,
		Component1 = 0x10,
		Component2 = 0x20,
		Component3 = 0x40
	}

	public ShopItemBundle Item;

	public bool Expired;

	public long StartTime;

	private List<ShopItem> _items;

	public float GetSeconds(long time)
	{
		long num = Item.GetDurationHours();
		num *= 36000000000L;
		long num2 = StartTime + num - time;
		float num3 = num2 / 10000000;
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		return num3;
	}

	public void Update(long ticks)
	{
		float seconds = GetSeconds(ticks);
		if (!(seconds > 0f))
		{
			Expired = true;
		}
	}

	public void Activate(ShopItemBundle item, long ticks)
	{
		Item = item;
		StartTime = ticks;
	}

	public bool IsActive()
	{
		if (Expired)
		{
			return false;
		}
		if (Item.IsOwned())
		{
			return false;
		}
		if (StartTime > 0)
		{
			return true;
		}
		return false;
	}

	private void CollectPrices(ref int priceSoft, ref int priceHard, ShopItem item)
	{
		if (item != null)
		{
			int priceHard2 = item.GetPriceHard();
			if (priceHard2 != 0)
			{
				priceHard += priceHard2;
			}
			else
			{
				priceSoft += item.GetPriceSoft();
			}
		}
	}

	public List<ShopItem> GetItems(out FillingFlags filling)
	{
		filling = (FillingFlags)0;
		if (_items == null)
		{
			_items = new List<ShopItem>();
			filling |= (FillingFlags)(AddItemToList(MonoSingleton<ShopController>.Instance.GetItemVehicle(Item.VehicleId)) ? 1 : 0);
			filling |= (FillingFlags)(AddItemToList(MonoSingleton<ShopController>.Instance.GetItemBody(Item.BodyId)) ? 2 : 0);
			filling |= (FillingFlags)(AddItemToList(MonoSingleton<ShopController>.Instance.GetItemWeapon(Item.WeaponId)) ? 4 : 0);
			filling |= (FillingFlags)(AddItemToList(MonoSingleton<ShopController>.Instance.GetItemArmor(Item.ArmorId)) ? 8 : 0);
			FillingFlags[] array = new FillingFlags[3]
			{
				FillingFlags.Component1,
				FillingFlags.Component2,
				FillingFlags.Component3
			};
			int num = 0;
			ShopItemBundle.Component[] components = Item.Components;
			foreach (ShopItemBundle.Component component in components)
			{
				if (component != null && AddItemToList(MonoSingleton<ShopController>.Instance.GetItemComponent(component.Id)))
				{
					if (num < array.Length - 1)
					{
						filling |= array[num];
					}
					num++;
				}
			}
		}
		return _items;
	}

	private bool AddItemToList(ShopItem item)
	{
		if (item != null)
		{
			_items.Add(item);
			return true;
		}
		return false;
	}

	private int GetRegularHardPrice()
	{
		int priceSoft = 0;
		int priceHard = 0;
		FillingFlags filling;
		List<ShopItem> items = GetItems(out filling);
		foreach (ShopItem item in items)
		{
			CollectPrices(ref priceSoft, ref priceHard, item);
		}
		return priceHard;
	}

	public string GetSaveHardPriceString()
	{
		int regularHardPrice = GetRegularHardPrice();
		regularHardPrice -= Item.GetPriceHard();
		return "\u001f " + regularHardPrice;
	}

	public string GetRegularHardPriceString()
	{
		int regularHardPrice = GetRegularHardPrice();
		return "\u001f " + regularHardPrice;
	}
}
