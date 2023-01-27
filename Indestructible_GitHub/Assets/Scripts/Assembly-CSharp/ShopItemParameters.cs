using UnityEngine;

public class ShopItemParameters : MonoBehaviour
{
	public GameObject ObjectDPS;

	public GameObject ObjectEnergyConsumption;

	public GameObject ObjectPower;

	private Vector3 basePosition = new Vector3(0f, -60f, 0f);

	private Vector3 baseDelta = new Vector3(0f, 14f, 0f);

	private Vector3 position;

	private void PlaceParameter(GameObject parameterObject, string amount)
	{
		if (string.IsNullOrEmpty(amount))
		{
			MonoUtils.SetActive(parameterObject, false);
			return;
		}
		MonoUtils.SetActive(parameterObject, true);
		ShopItemParameter component = parameterObject.GetComponent<ShopItemParameter>();
		Transform component2 = parameterObject.GetComponent<Transform>();
		Vector3 localPosition = component2.localPosition;
		localPosition.y = position.y;
		component2.localPosition = localPosition;
		component.Value.SetColor(Color.white);
		component.Value.Text = amount;
		float internalWidth = component.Value.TotalWidth + component.WidthOffset;
		component.Background.SetInternalWidth(internalWidth);
		position += baseDelta;
	}

	public void SetData(ShopItem item, int slot)
	{
		if (!base.gameObject.active)
		{
			return;
		}
		position = basePosition;
		int power = item.GetPower();
		string amount = power.ToString("+0;0");
		PlaceParameter(ObjectPower, amount);
		ShopItemParameter component = ObjectPower.GetComponent<ShopItemParameter>();
		if (item.ItemType != ShopItemType.Vehicle)
		{
			if (!MonoSingleton<Player>.Instance.CanMount(item, slot))
			{
				component.Value.SetColor(new Color(0.9f, 0.2f, 0f));
			}
			else if (power > 0)
			{
				component.Value.SetColor(new Color(0f, 0.9f, 0.2f));
			}
		}
		string amount2 = string.Empty;
		string amount3 = string.Empty;
		if (item.ItemType == ShopItemType.Weapon)
		{
			ShopItemWeapon shopItemWeapon = item as ShopItemWeapon;
			amount2 = NumberFormat.TryRound(shopItemWeapon.Damage / shopItemWeapon.ShotInterval);
			amount3 = NumberFormat.TryRound(shopItemWeapon.ShotEnergyConsumption / shopItemWeapon.ShotInterval);
		}
		PlaceParameter(ObjectEnergyConsumption, amount3);
		PlaceParameter(ObjectDPS, amount2);
	}
}
