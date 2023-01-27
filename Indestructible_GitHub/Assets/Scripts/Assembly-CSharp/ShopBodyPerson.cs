using Glu.Localization;
using UnityEngine;

public class ShopBodyPerson : MonoBehaviour
{
	public UIBorderSprite PersonBackground;

	public UISingleSprite PersonBodySprite;

	public UISingleSprite PersonSprite;

	public UISingleSprite PersonIcon;

	public SpriteText PersonName;

	public void SetData(ShopItemBody body)
	{
		PersonName.Text = Strings.GetString(body.nameId);
		PersonBackground.SetWidth(PersonName.TotalWidth);
		SimpleSpriteUtils.ChangeTexture(PersonSprite, body.PersonSprite);
		SimpleSpriteUtils.ChangeTexture(PersonBodySprite, body.ItemSprite);
	}
}
