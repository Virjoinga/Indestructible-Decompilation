using UnityEngine;

public class CustomizeSlotParameters : MonoBehaviour
{
	public GameObject ObjectDPS;

	public GameObject ObjectRateOfFire;

	public GameObject ObjectEnergy;

	public GameObject ObjectCapacity;

	private Vector3 basePosition = Vector3.zero;

	private Vector3 baseDelta = new Vector3(0f, -12.5f, 0f);

	private Vector3 position;

	private void PlaceParameter(GameObject parameterObject, string amount, float delta, bool invertColors)
	{
		if (string.IsNullOrEmpty(amount))
		{
			MonoUtils.SetActive(parameterObject, false);
			return;
		}
		CustomizeSlotParameter component = parameterObject.GetComponent<CustomizeSlotParameter>();
		Transform component2 = parameterObject.GetComponent<Transform>();
		component2.localPosition = position;
		component.Value.Text = amount;
		component.Delta.Text = FormatDelta(delta);
		if ((delta > 0f && !invertColors) || (delta < 0f && invertColors))
		{
			component.Delta.SetColor(new Color(0f, 0.78f, 0f));
		}
		else
		{
			component.Delta.SetColor(new Color(0.78f, 0f, 0f));
		}
		MonoUtils.SetActive(parameterObject, true);
		position += baseDelta;
	}

	public void SetData(ShopItem item, BuffModifyInfo baseInfo, BuffModifyInfo info)
	{
		position = basePosition;
		if (item.ItemType == ShopItemType.Weapon)
		{
			SetWeaponData(item as ShopItemWeapon, baseInfo, info);
			return;
		}
		if (item.ItemType == ShopItemType.Armor)
		{
			SetArmorData(item as ShopItemArmor);
			return;
		}
		if (item.ItemType == ShopItemType.Component)
		{
			SetComponentData(item as ShopItemComponent);
			return;
		}
		MonoUtils.SetActive(ObjectCapacity, false);
		MonoUtils.SetActive(ObjectDPS, false);
		MonoUtils.SetActive(ObjectRateOfFire, false);
		MonoUtils.SetActive(ObjectEnergy, false);
	}

	private void SetComponentData(ShopItemComponent item)
	{
		PlaceParameter(ObjectCapacity, item.GetPower().ToString("+0;0"), 0f, false);
		MonoUtils.SetActive(ObjectDPS, false);
		MonoUtils.SetActive(ObjectRateOfFire, false);
		MonoUtils.SetActive(ObjectEnergy, false);
	}

	private void SetArmorData(ShopItemArmor item)
	{
		PlaceParameter(ObjectCapacity, item.GetPower().ToString("+0;0"), 0f, false);
		MonoUtils.SetActive(ObjectDPS, false);
		MonoUtils.SetActive(ObjectRateOfFire, false);
		MonoUtils.SetActive(ObjectEnergy, false);
	}

	private string FormatDelta(float delta)
	{
		delta = NumberFormat.RoundTo(delta, 2);
		if (delta == 0f)
		{
			return string.Empty;
		}
		string text = NumberFormat.TryRound(delta);
		if (delta > 0f)
		{
			text = "+" + text;
		}
		return text;
	}

	private void SetWeaponData(ShopItemWeapon item, BuffModifyInfo baseInfo, BuffModifyInfo info)
	{
		float delta = 0f;
		float delta2 = 0f;
		float delta3 = 0f;
		if (info.FireInterval != 0f && baseInfo.FireInterval != 0f)
		{
			delta = info.Damage / info.FireInterval - baseInfo.Damage / baseInfo.FireInterval;
			delta3 = info.EnergyShot / info.FireInterval - baseInfo.EnergyShot / baseInfo.FireInterval;
			if (item.WeaponType != "thermal")
			{
				delta2 = 1f / info.FireInterval - 1f / baseInfo.FireInterval;
			}
		}
		string amount = item.GetPower().ToString("+0;0");
		string damagePerSecondString = item.GetDamagePerSecondString();
		string rateOfFirePerSecondString = item.GetRateOfFirePerSecondString();
		string energyConsumptionPerSecondString = item.GetEnergyConsumptionPerSecondString();
		PlaceParameter(ObjectCapacity, amount, 0f, false);
		PlaceParameter(ObjectDPS, damagePerSecondString, delta, false);
		PlaceParameter(ObjectRateOfFire, rateOfFirePerSecondString, delta2, false);
		PlaceParameter(ObjectEnergy, energyConsumptionPerSecondString, delta3, true);
	}
}
