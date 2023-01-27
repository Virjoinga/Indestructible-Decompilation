using Glu.Localization;
using UnityEngine;

public class DialogShopVehicleAbilities : MonoBehaviour
{
	public SpriteText AbilityActiveLabel;

	public SpriteText AbilityActiveText;

	public UISingleSprite AbilityActiveIcon;

	public SpriteText AbilityPassiveLabel;

	public SpriteText AbilityPassiveText;

	public UISingleSprite AbilityPassiveIcon;

	public void SetData(ShopItemVehicle vehicle)
	{
		AbilityActiveLabel.Text = Strings.GetString(vehicle.Abilities.Active.NameId);
		AbilityActiveText.Text = Strings.GetString(vehicle.Abilities.Active.DescriptionId);
		SimpleSpriteUtils.ChangeTexture(AbilityActiveIcon, vehicle.Abilities.Active.Icon);
		AbilityPassiveLabel.Text = Strings.GetString(vehicle.Abilities.Passive.NameId);
		AbilityPassiveText.Text = Strings.GetString(vehicle.Abilities.Passive.DescriptionId);
		SimpleSpriteUtils.ChangeTexture(AbilityPassiveIcon, vehicle.Abilities.Passive.Icon);
	}
}
