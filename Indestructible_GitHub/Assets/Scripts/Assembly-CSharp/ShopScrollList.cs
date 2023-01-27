using System.Collections.Generic;
using UnityEngine;

public class ShopScrollList : MonoBehaviour
{
	public delegate bool FeedDelegateType(ShopItem item);

	public PanelShop ParentPanelShop;

	public GameObject ShopItemStandardPrefab;

	public FeedDelegateType FeedDelegate;

	private UIPageScrollList _scrollList;

	private string _currentGroup = string.Empty;

	private List<ShopItem> _items = new List<ShopItem>();

	public string GetCurrentGroup()
	{
		return _currentGroup;
	}

	public ShopItemStandard FindItem(string id)
	{
		int count = _scrollList.Count;
		for (int i = 0; i < count; i++)
		{
			UIListItemContainer uIListItemContainer = _scrollList.GetItem(i) as UIListItemContainer;
			ShopItemStandard component = uIListItemContainer.GetComponent<ShopItemStandard>();
			if (component.Item.id == id)
			{
				return component;
			}
		}
		return null;
	}

	public void ShowSkins(ShopItemStandard item)
	{
		ParentPanelShop.SaveShopDelegates();
		ShopItemVehicle vehicle = item.Item as ShopItemVehicle;
		FeedDelegate = delegate(ShopItem shopItem)
		{
			ShopItemBody shopItemBody = shopItem as ShopItemBody;
			return !(vehicle.BodyId == shopItemBody.id) && shopItemBody.VehicleId == item.Item.id;
		};
		FeedGroup("bodies");
	}

	private void Awake()
	{
		Vector2 screenSize = UITools.GetScreenSize();
		_scrollList = GetComponent<UIPageScrollList>();
		_scrollList.viewableArea.x = screenSize.x;
	}

	private void Start()
	{
	}

	private void EventFillData(IUIObject container, int index)
	{
		ShopItemStandard component = container.gameObject.GetComponent<ShopItemStandard>();
		component.SetData(this, _items[index]);
	}

	private void EventFreeData(IUIObject container, int index)
	{
		ShopItemStandard component = container.gameObject.GetComponent<ShopItemStandard>();
		component.ReleaseData();
	}

	public void FeedWeapons(string scrollToItemId)
	{
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("weapons");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedBodies(string scrollToItemId)
	{
		ShopItemVehicle vehicle = MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle;
		string vehicleId = vehicle.id;
		FeedDelegate = delegate(ShopItem shopItem)
		{
			ShopItemBody shopItemBody = shopItem as ShopItemBody;
			return vehicleId == shopItemBody.VehicleId;
		};
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("bodies");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedVehicles(string scrollToItemId)
	{
		FeedDelegate = (ShopItem shopItem) => !MonoSingleton<Player>.Instance.IsBought(shopItem);
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("vehicles");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedComponents(string scrollToItemId, int slot)
	{
		GarageVehicle vehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		int count = vehicle.Components.Length;
		ParentPanelShop.SlotIndex = slot;
		FeedDelegate = delegate(ShopItem shopItem)
		{
			for (int i = 0; i < count; i++)
			{
				if (i != slot)
				{
					ShopItemComponent shopItemComponent = vehicle.Components[i];
					if (shopItemComponent != null && shopItemComponent.id == shopItem.id)
					{
						return false;
					}
				}
			}
			return true;
		};
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("components");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedArmors(string scrollToItemId)
	{
		string vehicleId = MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.id;
		FeedDelegate = delegate(ShopItem shopItem)
		{
			ShopItemGarage shopItemGarage = shopItem as ShopItemGarage;
			return vehicleId == shopItemGarage.VehicleId;
		};
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("armors");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedAmmunitions(string scrollToItemId)
	{
		ShopItemWeapon weapon = MonoSingleton<Player>.Instance.SelectedVehicle.Weapon;
		FeedDelegate = (ShopItem shopItem) => true;
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("ammunition");
		FeedGroup(group, scrollToItemId);
	}

	public void FeedGroup(string id)
	{
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup(id);
		FeedGroup(group, string.Empty);
	}

	public void FeedGroup(ShopConfigGroup group, string scrollToItemId)
	{
		Clear();
		_currentGroup = group.id;
		int num = -1;
		int num2 = 0;
		if (group.id == "vehicles" && MonoSingleton<GameController>.Instance.Configuration.Bundles.Enabled)
		{
			foreach (PlayerBundle playerBundle in MonoSingleton<Player>.Instance.PlayerBundles)
			{
				if (playerBundle.IsActive())
				{
					_items.Add(playerBundle.Item);
				}
			}
		}
		foreach (ShopConfigGroup.Reference itemRef in group.itemRefs)
		{
			ShopItem shopItem = itemRef.item as ShopItem;
			if (FeedDelegate != null && !FeedDelegate(shopItem))
			{
				continue;
			}
			if (num == -1)
			{
				if (!string.IsNullOrEmpty(scrollToItemId))
				{
					if (scrollToItemId == shopItem.id)
					{
						num = num2;
					}
				}
				else if (MonoSingleton<Player>.Instance.IsEquipped(shopItem))
				{
					num = num2;
				}
			}
			_items.Add(shopItem);
			num2++;
		}
		_scrollList.EventFillData = EventFillData;
		_scrollList.EventFreeData = EventFreeData;
		_scrollList.Init(_items.Count);
		if (num != -1)
		{
			if (num < 0)
			{
				num = 0;
			}
			_scrollList.ScrollToItemIndex(num, 1.5f);
		}
	}

	public void Clear()
	{
		_scrollList.ScrollListTo(0f);
		_scrollList.FreeItems();
		_items.Clear();
	}

	public void ResetDelegates()
	{
		_currentGroup = string.Empty;
		FeedDelegate = null;
	}

	public ShopItem MountItem(ShopItem mountItem, ShopItemType type, int slot)
	{
		if (mountItem != null)
		{
			if (mountItem.ItemType == ShopItemType.Body)
			{
				MonoSingleton<UISounds>.Instance.Play(UISounds.Type.PaintjobEquiped);
			}
			else
			{
				MonoSingleton<UISounds>.Instance.Play(UISounds.Type.ItemEquiped);
			}
		}
		else
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.ItemRemoved);
		}
		return MonoSingleton<Player>.Instance.SelectedVehicle.Mount(mountItem, type, slot);
	}

	public void DemountItem(ShopItem demountItem)
	{
		ShopItemStandard shopItemStandard = FindItem(demountItem.id);
		if (shopItemStandard != null)
		{
			shopItemStandard.UpdateState();
		}
	}

	public int GetMountSlot(ShopItem item)
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		return selectedVehicle.ContainsAt(item);
	}

	public bool EquipItem(ShopItemStandard item)
	{
		bool flag = MonoSingleton<Player>.Instance.IsBought(item.Item);
		bool flag2 = item.Item.CanBuyMore();
		if (flag)
		{
			if (MonoSingleton<Player>.Instance.IsEquipped(item.Item))
			{
				if (item.Item.IsRemovable())
				{
					int mountSlot = GetMountSlot(item.Item);
					ShopItem mountItem = null;
					if (item.Item.ItemType == ShopItemType.Armor)
					{
						string armorId = MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.ArmorId;
						mountItem = MonoSingleton<ShopController>.Instance.GetItemArmor(armorId);
					}
					MountItem(mountItem, item.Item.ItemType, mountSlot);
					item.UpdateState();
					MonoSingleton<Player>.Instance.Save();
					return true;
				}
			}
			else
			{
				int slotIndex = ParentPanelShop.SlotIndex;
				if (MonoSingleton<Player>.Instance.CanMount(item.Item, slotIndex))
				{
					ShopItem shopItem = MountItem(item.Item, item.Item.ItemType, slotIndex);
					if (shopItem != null)
					{
						DemountItem(shopItem);
					}
					item.UpdateState();
					MonoSingleton<Player>.Instance.Save();
				}
				else
				{
					Dialogs.NotEnoughPower();
				}
			}
			ParentPanelShop.UpdateVehicle();
		}
		return false;
	}

	public bool BuyItem(ShopItemStandard item, ShopItemCurrency currency)
	{
		bool flag = MonoSingleton<Player>.Instance.IsBought(item.Item);
		bool flag2 = item.Item.CanBuyMore();
		if (!flag || flag2)
		{
			if (item.Item.IsLocked(currency))
			{
				return false;
			}
			if (!MonoSingleton<Player>.Instance.Buy(item.Item, currency))
			{
				return false;
			}
			item.UpdateState();
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.ItemPurchased);
			MonoSingleton<Player>.Instance.Achievements.UpdateGarage();
			MonoSingleton<Player>.Instance.Achievements.Send();
			MonoSingleton<Player>.Instance.Save();
		}
		return true;
	}
}
