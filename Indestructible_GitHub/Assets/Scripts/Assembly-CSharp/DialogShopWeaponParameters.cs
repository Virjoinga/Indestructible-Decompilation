using Glu.Localization;
using UnityEngine;

public class DialogShopWeaponParameters : MonoBehaviour
{
	public GameObject ObjectClass;

	public GameObject ObjectDPS;

	public GameObject ObjectRateOfFire;

	public GameObject ObjectEnergy;

	public GameObject ObjectBurnDamage;

	public PackedSprite ClassIcon;

	private string _classIconName;

	private Vector3 basePosition = Vector3.zero;

	private Vector3 baseDelta = new Vector3(0f, -12.5f, 0f);

	private Vector3 position;

	private void PlaceParameter(GameObject parameterObject, string amount)
	{
		if (amount.Length == 0)
		{
			MonoUtils.DetachAndDestroy(parameterObject);
			return;
		}
		DialogShopWeaponParameter component = parameterObject.GetComponent<DialogShopWeaponParameter>();
		Transform component2 = parameterObject.GetComponent<Transform>();
		component2.localPosition = position;
		component.Value.Text = amount.ToString();
		position += baseDelta;
	}

	public void SetData(ShopItemWeapon item)
	{
		position = basePosition;
		string amount = string.Empty;
		string damagePerSecondString = item.GetDamagePerSecondString();
		string rateOfFirePerSecondString = item.GetRateOfFirePerSecondString();
		string energyConsumptionPerSecondString = item.GetEnergyConsumptionPerSecondString();
		string amount2 = string.Empty;
		if (item.WeaponType == "kinetic")
		{
			_classIconName = "Kinetic";
			amount = Strings.GetString("IDS_WEAPON_KINETIC");
		}
		else if (item.WeaponType == "thermal")
		{
			_classIconName = "Thermal";
			amount = Strings.GetString("IDS_WEAPON_THERMAL");
			amount2 = item.GetBurnDamageString();
		}
		else if (item.WeaponType == "explosive")
		{
			_classIconName = "Explosive";
			amount = Strings.GetString("IDS_WEAPON_EXPLOSIVE");
		}
		PlaceParameter(ObjectClass, amount);
		PlaceParameter(ObjectDPS, damagePerSecondString);
		PlaceParameter(ObjectRateOfFire, rateOfFirePerSecondString);
		PlaceParameter(ObjectEnergy, energyConsumptionPerSecondString);
		PlaceParameter(ObjectBurnDamage, amount2);
	}

	private void Start()
	{
		ClassIcon.PlayAnim(_classIconName);
	}
}
