using Glu.Localization;
using UnityEngine;

public class AmmunitionInformation : MonoBehaviour
{
	public SpriteText AmmunitionName;

	public UISingleSprite AmmunitionSprite;

	public SpriteText AmmunitionCount;

	public SpriteText AmmunitionDesc;

	public UIButtonShade AmmunitionButton;

	public SpriteText TextTapToEquip;

	public SpriteText TextTapToChange;

	public SpriteText TextNothingEquipped;

	public void UpdateData()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		ShopItemAmmunition ammunition = selectedVehicle.Ammunition;
		if (ammunition != null)
		{
			MonoUtils.SetActive(AmmunitionName, true);
			MonoUtils.SetActive(AmmunitionSprite, true);
			MonoUtils.SetActive(AmmunitionDesc, true);
			MonoUtils.SetActive(AmmunitionCount, true);
			MonoUtils.SetActive(TextTapToChange, true);
			MonoUtils.SetActive(TextTapToEquip, false);
			MonoUtils.SetActive(TextNothingEquipped, false);
			AmmunitionName.Text = Strings.GetString(ammunition.nameId);
			SimpleSpriteUtils.ChangeTexture(AmmunitionSprite, ammunition.ItemSprite);
			AmmunitionDesc.Text = Strings.GetString(ammunition.DescriptionId);
			string @string = Strings.GetString("IDS_AMMUNITION_OWNED");
			AmmunitionCount.Text = string.Format(@string, ammunition.GetCount());
			AmmunitionButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
		}
		else
		{
			MonoUtils.SetActive(AmmunitionName, false);
			MonoUtils.SetActive(AmmunitionSprite, false);
			MonoUtils.SetActive(AmmunitionDesc, false);
			MonoUtils.SetActive(AmmunitionCount, false);
			MonoUtils.SetActive(TextTapToChange, false);
			MonoUtils.SetActive(TextTapToEquip, true);
			MonoUtils.SetActive(TextNothingEquipped, true);
			AmmunitionButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
	}
}
