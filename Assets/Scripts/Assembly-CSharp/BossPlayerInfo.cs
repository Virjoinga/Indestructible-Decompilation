using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class BossPlayerInfo : MonoBehaviour
{
	public SpriteText ItemPersonText;

	public UISingleSprite VehicleSprite;

	public UISingleSprite PortraitSprite;

	public UISingleSprite IconSprite;

	public UISingleSprite BackgroundSprite;

	public CustomizeVehicleParameters VehicleParameters;

	public void SetData(BossFightConfig boss)
	{
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(boss.ShopVehicleBody);
		if (itemBody != null)
		{
			if (string.IsNullOrEmpty(boss.BossName))
			{
				ItemPersonText.Text = Strings.GetString(itemBody.nameId);
			}
			else
			{
				ItemPersonText.Text = Strings.GetString(boss.BossName);
			}
			MonoUtils.SetActive(VehicleSprite, true);
			MonoUtils.SetActive(PortraitSprite, true);
			MonoUtils.SetActive(IconSprite, true);
			MonoUtils.SetActive(BackgroundSprite, false);
			SimpleSpriteUtils.ChangeTexture(VehicleSprite, itemBody.GarageSprite);
			SimpleSpriteUtils.ChangeTexture(PortraitSprite, itemBody.PersonSprite);
			SimpleSpriteUtils.ChangeTexture(IconSprite, itemBody.PersonIcon);
		}
		else
		{
			MonoUtils.SetActive(VehicleSprite, false);
			MonoUtils.SetActive(PortraitSprite, false);
			MonoUtils.SetActive(IconSprite, false);
			MonoUtils.SetActive(BackgroundSprite, true);
			ItemPersonText.Text = Strings.GetString(boss.BossName);
			SimpleSpriteUtils.ChangeTexture(BackgroundSprite, boss.BackgroundImage);
		}
		BossCompareScreenInfo compareInfo = boss.CompareInfo;
		if (compareInfo != null)
		{
			BuffModifyInfo buffModifyInfo = new BuffModifyInfo();
			buffModifyInfo.FireInterval = 1f;
			buffModifyInfo.Damage = compareInfo.Damage;
			buffModifyInfo.Health = compareInfo.Armor;
			buffModifyInfo.Speed = compareInfo.Speed;
			buffModifyInfo.Energy = compareInfo.Energy;
			VehicleParameters.UpdateData(buffModifyInfo);
		}
	}

	public void SetPlayerData()
	{
		ShopItemBody body = MonoSingleton<Player>.Instance.SelectedVehicle.Body;
		if (body != null)
		{
			ItemPersonText.Text = Strings.GetString(MonoSingleton<Player>.Instance.Name);
			SimpleSpriteUtils.ChangeTexture(VehicleSprite, body.GarageSprite);
			SimpleSpriteUtils.ChangeTexture(PortraitSprite, body.PersonSprite);
			SimpleSpriteUtils.ChangeTexture(IconSprite, body.PersonIcon);
		}
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		VehicleParameters.UpdateData(CollectBuffModifyInfo(GetPlayerBaseInfo(), selectedVehicle.Armor, selectedVehicle.Components, MonoSingleton<Player>.Instance.BoughtTalents));
	}

	public void SetDataCompared(BossFightConfig boss)
	{
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(boss.ShopVehicleBody);
		if (itemBody != null)
		{
			if (string.IsNullOrEmpty(boss.BossName))
			{
				ItemPersonText.Text = Strings.GetString(itemBody.nameId);
			}
			else
			{
				ItemPersonText.Text = Strings.GetString(boss.BossName);
			}
			MonoUtils.SetActive(VehicleSprite, true);
			MonoUtils.SetActive(PortraitSprite, true);
			MonoUtils.SetActive(IconSprite, true);
			MonoUtils.SetActive(BackgroundSprite, false);
			SimpleSpriteUtils.ChangeTexture(VehicleSprite, itemBody.GarageSprite);
			SimpleSpriteUtils.ChangeTexture(PortraitSprite, itemBody.PersonSprite);
			SimpleSpriteUtils.ChangeTexture(IconSprite, itemBody.PersonIcon);
		}
		else
		{
			MonoUtils.SetActive(VehicleSprite, false);
			MonoUtils.SetActive(PortraitSprite, false);
			MonoUtils.SetActive(IconSprite, false);
			MonoUtils.SetActive(BackgroundSprite, true);
			ItemPersonText.Text = Strings.GetString(boss.BossName);
			SimpleSpriteUtils.ChangeTexture(BackgroundSprite, boss.BackgroundImage);
		}
		BossCompareScreenInfo compareInfo = boss.CompareInfo;
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		VehicleParameters.UpdateData(CollectBuffModifyInfo(GetPlayerBaseInfo(), selectedVehicle.Armor, selectedVehicle.Components, MonoSingleton<Player>.Instance.BoughtTalents));
		if (compareInfo != null)
		{
			BuffModifyInfo buffModifyInfo = new BuffModifyInfo();
			buffModifyInfo.FireInterval = 1f;
			buffModifyInfo.Damage = compareInfo.Damage;
			buffModifyInfo.Health = compareInfo.Armor;
			buffModifyInfo.Speed = compareInfo.Speed;
			buffModifyInfo.Energy = compareInfo.Energy;
			VehicleParameters.UpdateData(buffModifyInfo, CollectBuffModifyInfo(GetPlayerBaseInfo(), selectedVehicle.Armor, selectedVehicle.Components, MonoSingleton<Player>.Instance.BoughtTalents));
		}
	}

	public void SetPlayerDataCompared(BossFightConfig boss)
	{
		ShopItemBody body = MonoSingleton<Player>.Instance.SelectedVehicle.Body;
		if (body != null)
		{
			ItemPersonText.Text = MonoSingleton<Player>.Instance.Name;
			SimpleSpriteUtils.ChangeTexture(VehicleSprite, body.GarageSprite);
			SimpleSpriteUtils.ChangeTexture(PortraitSprite, body.PersonSprite);
			SimpleSpriteUtils.ChangeTexture(IconSprite, body.PersonIcon);
		}
		BossCompareScreenInfo compareInfo = boss.CompareInfo;
		BuffModifyInfo buffModifyInfo = new BuffModifyInfo();
		if (compareInfo != null)
		{
			buffModifyInfo.FireInterval = 1f;
			buffModifyInfo.Damage = compareInfo.Damage;
			buffModifyInfo.Health = compareInfo.Armor;
			buffModifyInfo.Speed = compareInfo.Speed;
			buffModifyInfo.Energy = compareInfo.Energy;
		}
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		VehicleParameters.UpdateData(CollectBuffModifyInfo(GetPlayerBaseInfo(), selectedVehicle.Armor, selectedVehicle.Components, MonoSingleton<Player>.Instance.BoughtTalents), buffModifyInfo);
	}

	private BuffModifyInfo GetPlayerBaseInfo()
	{
		BuffModifyInfo buffModifyInfo = new BuffModifyInfo();
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		string weaponType = selectedVehicle.Weapon.WeaponType;
		buffModifyInfo.WeaponDamageType = DamageType.Generic;
		switch (weaponType)
		{
		case "explosive":
			buffModifyInfo.WeaponDamageType = DamageType.Explosive;
			break;
		case "kinetic":
			buffModifyInfo.WeaponDamageType = DamageType.Kinetic;
			break;
		case "thermal":
			buffModifyInfo.WeaponDamageType = DamageType.Thermal;
			break;
		}
		buffModifyInfo.Health = selectedVehicle.Vehicle.Health;
		buffModifyInfo.Power = selectedVehicle.Vehicle.GetPower();
		buffModifyInfo.EnergyShot = selectedVehicle.Weapon.ShotEnergyConsumption;
		buffModifyInfo.Energy = selectedVehicle.Vehicle.Energy;
		buffModifyInfo.Speed = selectedVehicle.Vehicle.Speed;
		buffModifyInfo.FireInterval = selectedVehicle.Weapon.ShotInterval;
		buffModifyInfo.Damage = selectedVehicle.Weapon.Damage;
		return buffModifyInfo;
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

	private BuffModifyInfo CollectBuffModifyInfo(BuffModifyInfo baseInfo, ShopItemArmor armor, ShopItemComponent[] components, List<PlayerTalent> talents)
	{
		BuffModifyInfo info = new BuffModifyInfo(baseInfo);
		Collect(armor, 1, baseInfo, ref info);
		if (components != null)
		{
			foreach (ShopItem item in components)
			{
				Collect(item, 1, baseInfo, ref info);
			}
		}
		if (talents != null)
		{
			foreach (PlayerTalent talent in talents)
			{
				Collect(talent.Item, talent.Level, baseInfo, ref info);
			}
		}
		return info;
	}
}
