using Glu.Localization;
using UnityEngine;

public class LoadoutInformation : MonoBehaviour
{
	public SpriteText WeaponName;

	public UISingleSprite WeaponSprite;

	public ComponentsLoadoutInfo ComponentsInfo;

	public void UpdateData()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		ShopItemWeapon weapon = selectedVehicle.Weapon;
		WeaponName.Text = Strings.GetString(weapon.nameId);
		SimpleSpriteUtils.ChangeTexture(WeaponSprite, weapon.ItemSprite);
		int num = ComponentsInfo.ComponentsNames.Length;
		for (int i = 0; i < num; i++)
		{
			ShopItemComponent shopItemComponent = selectedVehicle.Components[i];
			SpriteText spriteText = ComponentsInfo.ComponentsNames[i];
			if (shopItemComponent != null)
			{
				spriteText.SetColor(Color.white);
				spriteText.Text = Strings.GetString(shopItemComponent.nameId);
			}
			else
			{
				spriteText.SetColor(new Color(1f, 1f, 1f, 0.5f));
				spriteText.Text = Strings.GetString("IDS_AMMUNITION_NO_COMPONENT_EQUIPPED");
			}
		}
	}
}
