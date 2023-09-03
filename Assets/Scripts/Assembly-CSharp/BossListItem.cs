using Glu.Localization;
using UnityEngine;

public class BossListItem : MonoBehaviour
{
	public SpriteText ItemPersonText;

	public PackedSprite ItemPersonNameBG;

	public SpriteText ItemName;

	public SpriteText FightNumberText;

	public UIBorderSprite FightNumberBg;

	public SpriteText LockText;

	public UISingleSprite VehicleSprite;

	public UISingleSprite PortraitSprite;

	public UISingleSprite IconSprite;

	public PackedSprite DefeatedSprite;

	public UISingleSprite FullSizePicture;

	public UIExpandSprite ItemBackground;

	public GameObject ObjectItemBackground;

	public GameObject ObjectItemStripesItem;

	public string LockedString = string.Empty;

	public string DefeatedString = string.Empty;

	protected bool _currentBoss;

	protected CampaignScrollList _campaignLine;

	protected bool _equipBoughtOnBuyButton = true;

	protected bool _equipBoughtOnDualBuyButton;

	public virtual void ReleaseData()
	{
		SimpleSpriteUtils.UnloadTexture(VehicleSprite);
		SimpleSpriteUtils.UnloadTexture(PortraitSprite);
		if (!_currentBoss)
		{
			SimpleSpriteUtils.UnloadTexture(IconSprite);
		}
	}

	public virtual void SetData(CampaignScrollList list, BossFightConfig boss, int boosNumber, bool currentBoss, bool defeated)
	{
		_currentBoss = currentBoss;
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0.4f, 0.4f, 0.4f, 1f);
		Color color3 = ((!currentBoss) ? color2 : color);
		if (DefeatedSprite != null)
		{
			MonoUtils.SetActive(DefeatedSprite, defeated);
		}
		if ((bool)FightNumberText)
		{
			FightNumberText.Text = boosNumber.ToString();
			FightNumberText.Color = color3;
		}
		if ((bool)FightNumberBg)
		{
			FightNumberBg.Color = color3;
		}
		if ((bool)LockText)
		{
			MonoUtils.SetActive(LockText, !_currentBoss);
			LockText.Text = Strings.GetString((!defeated) ? LockedString : DefeatedString);
		}
		if ((bool)ItemBackground)
		{
			ItemBackground.Color = color3;
		}
		if ((bool)ItemPersonNameBG)
		{
			ItemPersonNameBG.Color = color3;
		}
		if (!string.IsNullOrEmpty(boss.ShopVehicleBody))
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
				MonoUtils.SetActive(FullSizePicture, false);
				VehicleSprite.Color = color3;
				PortraitSprite.Color = color3;
				IconSprite.Color = color3;
				ItemPersonText.Color = color3;
				SimpleSpriteUtils.ChangeTexture(VehicleSprite, itemBody.GarageSprite);
				SimpleSpriteUtils.ChangeTexture(PortraitSprite, itemBody.PersonSprite);
				SimpleSpriteUtils.ChangeTexture(IconSprite, itemBody.PersonIcon);
			}
		}
		else
		{
			MonoUtils.SetActive(VehicleSprite, false);
			MonoUtils.SetActive(PortraitSprite, false);
			MonoUtils.SetActive(IconSprite, false);
			MonoUtils.SetActive(FullSizePicture, true);
			FullSizePicture.Color = color3;
			ItemPersonText.Color = color3;
			ItemPersonText.Text = Strings.GetString(boss.BossName);
			SimpleSpriteUtils.ChangeTexture(FullSizePicture, boss.BackgroundImage);
		}
	}
}
