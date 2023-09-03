using Glu.Localization;
using UnityEngine;

public class ProfilePlayerInformation : MonoBehaviour
{
	public UIBorderSprite NameBackground;

	public SpriteText NameLabel;

	public GarageLeagueIcon LeagueIcon;

	public UIButton FacebookButton;

	public SpriteText FacebookRewardText;

	public ProfilePlayerStatisticsItem[] StatisticsItems;

	private int _index;

	private bool _loggedIn;

	private void SetItem(string nameId, string amount)
	{
		ProfilePlayerStatisticsItem profilePlayerStatisticsItem = StatisticsItems[_index];
		profilePlayerStatisticsItem.ValueText.Text = amount;
		profilePlayerStatisticsItem.NameText.Text = Strings.GetString(nameId);
		_index++;
	}

	public void UpdateFacebookButton(bool force)
	{
		bool flag = ASocial.Facebook.IsLoggedIn();
		if (flag == _loggedIn && !force)
		{
			return;
		}
		_loggedIn = flag;
		if (ASocial.Facebook.IsLoggedIn())
		{
			FacebookButton.Text = Strings.GetString("IDS_PROFILE_FACEBOOK_LOGOUT");
			bool facebookLoginRewarded = MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded;
			GameAnalytics.EventFacebookLoggedIn("PanelProfile", facebookLoginRewarded);
			if (!facebookLoginRewarded)
			{
				MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded = true;
				MonoSingleton<Player>.Instance.AddMoneyHard(10, "CREDIT_IN_GAME_AWARD", "Facebook Login", "FACEBOOK");
				MonoSingleton<Player>.Instance.Save();
			}
		}
		else
		{
			FacebookButton.Text = Strings.GetString("IDS_PROFILE_FACEBOOK_LOGIN");
		}
		if (!MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded)
		{
			string @string = Strings.GetString("IDS_PROFILE_FACEBOOK_FREE_GOLD");
			FacebookRewardText.Text = string.Format(@string, "\u001f");
		}
		else
		{
			FacebookRewardText.Text = string.Empty;
		}
	}

	public void UpdateData()
	{
		LeagueIcon.UpdateIcon();
		UpdateFacebookButton(true);
		NameLabel.Text = MonoSingleton<Player>.Instance.Name;
		NameBackground.SetInternalWidth(NameLabel.TotalWidth);
		PlayerStatistics statistics = MonoSingleton<Player>.Instance.Statistics;
		int num = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Vehicle);
		int num2 = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Weapon);
		int count = MonoSingleton<ShopController>.Instance.GetGroup("vehicles").itemRefs.Count;
		int count2 = MonoSingleton<ShopController>.Instance.GetGroup("weapons").itemRefs.Count;
		int num3 = 0;
		int num4 = 0;
		if (statistics.MultiplayerGamesPlayed > 0)
		{
			float num5 = statistics.MultiplayerTotalKills;
			float num6 = statistics.MultiplayerGamesPlayed;
			float num7 = statistics.MultiplayerTotalDeaths;
			num3 = Mathf.RoundToInt(num5 / num6);
			num4 = Mathf.RoundToInt(num7 / num6);
		}
		_index = 0;
		SetItem("IDS_STATISTICS_MULTIPLAYER_GAMES_PLAYED", statistics.MultiplayerGamesPlayed.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_GAMES_WON", statistics.MultiplayerGamesWon.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_TOTAL_KILLS", statistics.MultiplayerTotalKills.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_TOTAL_DEATHS", statistics.MultiplayerTotalDeaths.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_AVERAGE_KILLS", num3.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_AVERAGE_DEATHS", num4.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_POWERUPS_COLLECTED", statistics.MultiplayerPowerupsCollected.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_TOTAL_FLAGS_CAPTURED", statistics.MultiplayerTotalFlagsCaptured.ToString());
		SetItem("IDS_STATISTICS_MULTIPLAYER_TOTAL_CHARGES_CAPTURED", statistics.MultiplayerTotalChargesCaptured.ToString());
		SetItem("IDS_STATISTICS_TOTAL_EXPERIENCE_EARNED", statistics.TotalExperienceEarned.ToString());
		SetItem("IDS_STATISTICS_TOTAL_INFLUENCE_POINTS_EARNED", statistics.TotalInfluencePointsEarned.ToString());
		SetItem("IDS_STATISTICS_TOTAL_MONEY_SOFT_EARNED", statistics.TotalMoneySoftEarned.ToString());
		SetItem("IDS_STATISTICS_TOTAL_VEHICLES_OWNED", num + "/" + count);
		SetItem("IDS_STATISTICS_TOTAL_WEAPONS_OWNED", num2 + "/" + count2);
	}
}
