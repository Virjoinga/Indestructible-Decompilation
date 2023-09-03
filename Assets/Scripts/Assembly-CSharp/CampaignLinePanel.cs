using Glu.Localization;
using UnityEngine;

public class CampaignLinePanel : PanelManagerPanel
{
	public SpriteText BattlesTextLabel;

	public CampaignScrollList BattlesList;

	public SpriteText BattleDescLabel;

	public SpriteText BossNameLabel;

	public UISingleSprite BossIconSprite;

	private int _fightIdx = -1;

	public override void OnActivate()
	{
		if (BossFightConfiguration.Instance.BossFights.Length <= 0)
		{
			return;
		}
		_fightIdx = MonoSingleton<Player>.Instance.LastWonBossFight + 1;
		if (_fightIdx >= BossFightConfiguration.Instance.BossFights.Length)
		{
			MonoSingleton<Player>.Instance.LastWonBossFight = -1;
			_fightIdx = 0;
		}
		BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[_fightIdx];
		if ((bool)BattlesTextLabel)
		{
			string @string = Strings.GetString("IDS_CAMPAIGN_BATTLE_LIST");
			BattlesTextLabel.Text = string.Format(@string, _fightIdx + 1, BossFightConfiguration.Instance.BossFights.Length);
		}
		if ((bool)BattleDescLabel)
		{
			string id = ((bossFightConfig.Texts == null || string.IsNullOrEmpty(bossFightConfig.Texts.Description)) ? string.Empty : bossFightConfig.Texts.Description);
			BattleDescLabel.Text = Strings.GetString(id);
		}
		if ((bool)BattlesList)
		{
			BattlesList.FeedBossFights(MonoSingleton<Player>.Instance.LastWonBossFight);
		}
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(bossFightConfig.ShopVehicleBody);
		if (itemBody != null)
		{
			if ((bool)BossNameLabel)
			{
				if (string.IsNullOrEmpty(bossFightConfig.BossName))
				{
					BossNameLabel.Text = Strings.GetString(itemBody.nameId);
				}
				else
				{
					BossNameLabel.Text = Strings.GetString(bossFightConfig.BossName);
				}
			}
			if ((bool)BossIconSprite)
			{
				MonoUtils.SetActive(BossIconSprite, true);
				SimpleSpriteUtils.ChangeTexture(BossIconSprite, itemBody.PersonIcon);
			}
		}
		else
		{
			if ((bool)BossNameLabel)
			{
				BossNameLabel.Text = Strings.GetString(bossFightConfig.BossName);
			}
			MonoUtils.SetActive(BossIconSprite, false);
		}
	}

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}

	private void OnContinueButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (BossFightConfiguration.Instance.BossFights.Length > 0)
		{
			SelectManager selectManager = Owner as SelectManager;
			selectManager.ActivatePanel("BossComparePanel");
		}
	}
}
