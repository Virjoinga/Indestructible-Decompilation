using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShopController : MonoSingleton<ShopController>, IShopConfigProvider
{
	private ShopConfig _shop;

	private ShopConfig _iapShop;

	private ShopConfig _talentsShop;

	private ShopConfig _bundlesShop;

	private IAPProcessor _processor;

	public ShopConfig shopConfig
	{
		get
		{
			return _iapShop;
		}
	}

	private void SetupOldPrices(ShopConfig oldShop, ShopConfig newShop)
	{
		foreach (ShopConfigItem item in newShop.items)
		{
			ShopItem shopItem = item as ShopItem;
			if (shopItem != null)
			{
				ShopItem shopItem2 = oldShop.FindItemById(shopItem.id) as ShopItem;
				if (shopItem2 != null)
				{
					shopItem.OldPriceSoft = shopItem2.PriceSoft;
					shopItem.OldPriceHard = shopItem2.PriceHard;
				}
			}
		}
	}

	public ShopConfig LoadShop(string shopPath)
	{
		string path = "Assets/Bundles/" + shopPath + ".xml";
		TextAsset textAsset = BundlesUtils.Load(path) as TextAsset;
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes, false);
		ShopConfig shopConfig = ShopConfig.Load(memoryStream);
		memoryStream.Close();
		string path2 = "Assets/Bundles/" + shopPath + "_override.xml";
		TextAsset textAsset2 = BundlesUtils.Load(path2) as TextAsset;
		if (textAsset2 != null)
		{
			MemoryStream memoryStream2 = new MemoryStream(textAsset2.bytes, false);
			ShopOverride overs = ShopOverride.Load(memoryStream2);
			memoryStream2.Close();
			ShopConfig shopConfig2 = shopConfig.Override(overs);
			SetupOldPrices(shopConfig, shopConfig2);
			shopConfig = shopConfig2;
		}
		return shopConfig;
	}

	public void ResumeIAPProcessor()
	{
	}

	public void PauseIAPProcessor()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		_shop = LoadShop("Shop/idt_shop");
		_iapShop = LoadShop("Shop/idt_iaps");
		_talentsShop = LoadShop("Shop/idt_talents");
		_bundlesShop = LoadShop("Shop/idt_bundles");
		SetupGroupsIds(_shop);
		ShopPrefabParameters.Initialize(_shop);
		PauseIAPProcessor();
	}

	private void SetupGroupsIds(ShopConfig shop)
	{
		List<ShopConfigGroup> rawGroups = shop.rawGroups;
		foreach (ShopConfigGroup item in rawGroups)
		{
			List<ShopConfigGroup.Reference> rawItemRefs = item.rawItemRefs;
			foreach (ShopConfigGroup.Reference item2 in rawItemRefs)
			{
				ShopItem shopItem = item2.item as ShopItem;
				if (shopItem != null)
				{
					shopItem.GroupId = item.id;
				}
			}
		}
	}

	public IAPShopItemSimple GetIAPItem(string id)
	{
		return _iapShop.FindItemById(id) as IAPShopItemSimple;
	}

	public IAPShopItemBoost GetItemBoost(string id)
	{
		return _iapShop.FindItemById(id) as IAPShopItemBoost;
	}

	public ShopItemTalent GetItemTalent(int index)
	{
		return _talentsShop.items[index] as ShopItemTalent;
	}

	public ShopItem GetItem(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		ShopConfigItem shopConfigItem = _shop.FindItemById(id);
		if (shopConfigItem != null)
		{
			return shopConfigItem as ShopItem;
		}
		shopConfigItem = _bundlesShop.FindItemById(id);
		if (shopConfigItem != null)
		{
			return shopConfigItem as ShopItem;
		}
		shopConfigItem = _iapShop.FindItemById(id);
		if (shopConfigItem != null)
		{
			return shopConfigItem as ShopItem;
		}
		shopConfigItem = _talentsShop.FindItemById(id);
		if (shopConfigItem != null)
		{
			return shopConfigItem as ShopItem;
		}
		Debug.LogWarning("Item not found - " + id);
		return null;
	}

	public ShopItemTalent GetItemTalent(string id)
	{
		return GetItem(id) as ShopItemTalent;
	}

	public ShopItemVehicle GetItemVehicle(string id)
	{
		return GetItem(id) as ShopItemVehicle;
	}

	public ShopItemArmor GetItemArmor(string id)
	{
		return GetItem(id) as ShopItemArmor;
	}

	public ShopItemBody GetItemBody(string id)
	{
		return GetItem(id) as ShopItemBody;
	}

	public ShopItemWeapon GetItemWeapon(string id)
	{
		return GetItem(id) as ShopItemWeapon;
	}

	public ShopItemComponent GetItemComponent(string id)
	{
		return GetItem(id) as ShopItemComponent;
	}

	public ShopItemAmmunition GetItemAmmunition(string id)
	{
		return GetItem(id) as ShopItemAmmunition;
	}

	public ShopItemPrice GetItemPrice(string id)
	{
		return GetItem(id) as ShopItemPrice;
	}

	public ShopItemSlot GetItemSlot(string id)
	{
		return GetItem(id) as ShopItemSlot;
	}

	public ShopItemBundle GetItemBundle(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		return _bundlesShop.FindItemById(id) as ShopItemBundle;
	}

	public ShopConfigGroup GetGroup(string id)
	{
		ShopConfigGroup shopConfigGroup = _shop.FindGroup(id);
		if (shopConfigGroup != null)
		{
			return shopConfigGroup;
		}
		shopConfigGroup = _bundlesShop.FindGroup(id);
		if (shopConfigGroup != null)
		{
			return shopConfigGroup;
		}
		shopConfigGroup = _iapShop.FindGroup(id);
		if (shopConfigGroup != null)
		{
			return shopConfigGroup;
		}
		shopConfigGroup = _talentsShop.FindGroup(id);
		if (shopConfigGroup != null)
		{
			return shopConfigGroup;
		}
		return null;
	}
}
