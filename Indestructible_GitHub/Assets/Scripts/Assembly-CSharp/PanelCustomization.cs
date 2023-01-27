using UnityEngine;

public class PanelCustomization : PanelAtlasController
{
	public CustomizeSlot[] Slots;

	public CustomizePowerMeter PowerMeter;

	public CustomizeVehicleParameters VehicleParameters;

	public CustomizeSlotInformation SlotInformation;

	public UIScrollList SlotsList;

	private CustomizeSlot _selected;

	private CustomizeSlot _previous;

	private BuffModifyInfo _info = new BuffModifyInfo();

	private BuffModifyInfo _baseInfo = new BuffModifyInfo();

	private void Awake()
	{
		float num = 50f * (float)Slots.Length;
		num += SlotsList.itemSpacing * ((float)Slots.Length + 1f);
		Vector2 screenSize = UITools.GetScreenSize();
		if (screenSize.x < num)
		{
			num = screenSize.x;
		}
		SlotsList.viewableArea.x = num;
	}

	public void OnPlayButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<Player>.Instance.Tutorial.SetPlayButtonTap();
		GameAnalytics.EventPlayButtonTap(base.Name);
		if (!MonoSingleton<Player>.Instance.IsEnoughPower())
		{
			Dialogs.TooMuchPowerUsed();
		}
		else if (!MonoSingleton<Player>.Instance.IsEnoughFuel())
		{
			Dialogs.RefuelVehicle();
		}
		else if (PanelGarage.NeedOpenAmmunitionPanel())
		{
			GarageManager garageManager = Owner as GarageManager;
			garageManager.ActivatePanel("PanelAmmunition");
		}
		else
		{
			GarageManager manager = Owner as GarageManager;
			PanelGarage.StartSelectScene(manager);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}

	public void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (MonoSingleton<Player>.Instance.IsEnoughPower())
		{
			Owner.ActivatePanel("PanelGarage");
		}
		else
		{
			Dialogs.TooMuchPowerUsed();
		}
	}

	public void OpenShopWeapons()
	{
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		panelShop.ScrollList.FeedWeapons(string.Empty);
	}

	public void OpenShopArmors()
	{
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		panelShop.ScrollList.FeedArmors(string.Empty);
	}

	public void OpenShopAmmunitions()
	{
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		panelShop.ScrollList.FeedAmmunitions(string.Empty);
	}

	public CustomizeSlot GetWeaponSlot()
	{
		CustomizeSlot[] slots = Slots;
		foreach (CustomizeSlot customizeSlot in slots)
		{
			if (customizeSlot.SlotType == ShopItemType.Weapon)
			{
				return customizeSlot;
			}
		}
		return null;
	}

	public bool SlotValid(CustomizeSlot slot)
	{
		if (slot == null)
		{
			return false;
		}
		if (slot.Item == null)
		{
			return false;
		}
		return true;
	}

	private void Collect(ShopItem item, int stackCount, BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		if (item != null && !string.IsNullOrEmpty(item.ComponentPrefab))
		{
			BuffConf buffConf = (BuffConf)Resources.Load(item.ComponentPrefab);
			if (buffConf != null)
			{
				Buff buff = buffConf.CreateBuff();
				buff.Stacks = stackCount;
				buff.GetModifyInfo(baseInfo, ref info);
			}
		}
	}

	private void CollectBuffModifyInfo()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		string weaponType = selectedVehicle.Weapon.WeaponType;
		_baseInfo.WeaponDamageType = DamageType.Generic;
		switch (weaponType)
		{
		case "explosive":
			_baseInfo.WeaponDamageType = DamageType.Explosive;
			break;
		case "kinetic":
			_baseInfo.WeaponDamageType = DamageType.Kinetic;
			break;
		case "thermal":
			_baseInfo.WeaponDamageType = DamageType.Thermal;
			break;
		}
		_baseInfo.Health = selectedVehicle.Vehicle.Health;
		_baseInfo.Power = selectedVehicle.Vehicle.GetPower();
		_baseInfo.EnergyShot = selectedVehicle.Weapon.ShotEnergyConsumption;
		_baseInfo.Energy = selectedVehicle.Vehicle.Energy;
		_baseInfo.Speed = selectedVehicle.Vehicle.Speed;
		_baseInfo.FireInterval = selectedVehicle.Weapon.ShotInterval;
		_baseInfo.Damage = selectedVehicle.Weapon.Damage;
		_info = new BuffModifyInfo(_baseInfo);
		Collect(selectedVehicle.Armor, 1, _baseInfo, ref _info);
		ShopItemComponent[] components = selectedVehicle.Components;
		foreach (ShopItem item in components)
		{
			Collect(item, 1, _baseInfo, ref _info);
		}
		foreach (PlayerTalent boughtTalent in MonoSingleton<Player>.Instance.BoughtTalents)
		{
			Collect(boughtTalent.Item, boughtTalent.Level, _baseInfo, ref _info);
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle == null)
		{
			return;
		}
		CollectBuffModifyInfo();
		VehicleParameters.UpdateData(_info);
		CustomizeSlot[] slots = Slots;
		foreach (CustomizeSlot customizeSlot in slots)
		{
			ShopItem shopItem = selectedVehicle.GetItem(customizeSlot.SlotType, customizeSlot.SlotIndex);
			if (shopItem != null && customizeSlot.SlotType == ShopItemType.Armor)
			{
				string armorId = selectedVehicle.Vehicle.ArmorId;
				if (!(shopItem.id != armorId))
				{
					shopItem = null;
				}
			}
			customizeSlot.SetData(this, shopItem);
		}
		if (SlotValid(_selected))
		{
			SlotInformation.SetData(_selected, _baseInfo, _info);
		}
		else if (SlotValid(_previous))
		{
			Select(_previous);
		}
		else
		{
			Select(GetWeaponSlot());
		}
		PowerMeter.UpdateValue();
	}

	private void OpenShopComponents(int slot)
	{
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		panelShop.ScrollList.FeedComponents(string.Empty, slot);
	}

	public void Select(CustomizeSlot slot)
	{
		bool flag = false;
		if (_selected == slot)
		{
			flag = true;
		}
		else
		{
			_previous = _selected;
			if (_selected != null)
			{
				_selected.SetSelected(false);
				_selected = null;
			}
			if (slot.Item == null)
			{
				flag = true;
			}
			_selected = slot;
			_selected.SetSelected(true);
			SlotInformation.SetData(_selected, _baseInfo, _info);
		}
		if (flag)
		{
			if (slot.SlotType == ShopItemType.Weapon)
			{
				OpenShopWeapons();
			}
			else if (slot.SlotType == ShopItemType.Armor)
			{
				OpenShopArmors();
			}
			else if (slot.SlotType == ShopItemType.Component)
			{
				OpenShopComponents(slot.SlotIndex);
			}
			else if (slot.SlotType == ShopItemType.Ammunition)
			{
				OpenShopAmmunitions();
			}
		}
	}
}
