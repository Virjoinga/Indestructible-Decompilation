using Glu.Localization;
using UnityEngine;

public class DialogBossFightFinished : UIDialog
{
	public SpriteText TitleText;

	public SpriteText WinText;

	public SpriteText ItemPersonText;

	public UISingleSprite VehicleSprite;

	public UISingleSprite PortraitSprite;

	public UISingleSprite IconSprite;

	public UISingleSprite BackgroundSprite;

	public SpriteText CongratulationText;

	private bool _winner;

	protected override void Awake()
	{
		base.Awake();
	}

	public void SetData(bool winner)
	{
		_winner = winner;
	}

	public override void Activate()
	{
		base.Activate();
		int num = ((!_winner) ? (MonoSingleton<Player>.Instance.LastWonBossFight + 1) : MonoSingleton<Player>.Instance.LastWonBossFight);
		BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[num];
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(bossFightConfig.ShopVehicleBody);
		if (itemBody != null)
		{
			MonoUtils.SetActive(VehicleSprite, true);
			MonoUtils.SetActive(PortraitSprite, true);
			MonoUtils.SetActive(IconSprite, true);
			MonoUtils.SetActive(BackgroundSprite, false);
			if (string.IsNullOrEmpty(bossFightConfig.BossName))
			{
				ItemPersonText.Text = Strings.GetString(itemBody.nameId);
			}
			else
			{
				ItemPersonText.Text = Strings.GetString(bossFightConfig.BossName);
			}
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
			SimpleSpriteUtils.ChangeTexture(BackgroundSprite, bossFightConfig.BackgroundImage);
			ItemPersonText.Text = Strings.GetString(bossFightConfig.BossName);
		}
		BossTextsConfig texts = bossFightConfig.Texts;
		if (texts != null)
		{
			string text = ((!_winner) ? texts.Defeat : texts.Win);
			if (!string.IsNullOrEmpty(text))
			{
				CongratulationText.Text = Strings.GetString(text);
			}
		}
		string id = "IDS_CAMPAIGN_RESULT_DEFEAT";
		if (_winner)
		{
			id = "IDS_CAMPAIGN_RESULT_VICTORY";
		}
		TitleText.Text = Strings.GetString(id);
		if (_winner)
		{
			WinText.Text = Strings.GetString("IDS_CAMPAIGN_BADGE_DEFEATED");
		}
		else
		{
			WinText.Text = string.Empty;
		}
	}

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
