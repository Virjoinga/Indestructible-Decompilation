using Glu.Localization;
using UnityEngine;

public class DialogShopVehiclePerson : MonoBehaviour
{
	public UIBorderSprite PersonBackground;

	public UISingleSprite PersonBodySprite;

	public UISingleSprite PersonSprite;

	public UISingleSprite PersonIcon;

	public SpriteText PersonText;

	public SpriteText PersonName;

	public void SetData(ShopItemBody body)
	{
		PersonText.Text = Strings.GetString(body.DescriptionId);
		PersonName.Text = Strings.GetString(body.nameId);
		PersonBackground.SetWidth(PersonName.TotalWidth + 37f);
		SimpleSpriteUtils.ChangeTexture(PersonIcon, body.PersonIcon);
		SimpleSpriteUtils.ChangeTexture(PersonSprite, body.PersonSprite);
		SimpleSpriteUtils.ChangeTexture(PersonBodySprite, body.GarageSprite);
	}
}
