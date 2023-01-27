using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PlayerAchievements
{
	public class Achievement
	{
		public string Id;

		public string TitleId;

		public string DescriptionId;

		private int _value;

		private int _maximum;

		private bool _changed;

		private bool _shown;

		public Achievement(string id, string textId, int maximum)
		{
			Id = id;
			TitleId = textId + "_TITLE";
			DescriptionId = textId + "_DESCRIPTION";
			_maximum = maximum;
			Reset();
		}

		public void SetShown()
		{
			_shown = true;
		}

		public void Reset()
		{
			_value = 0;
			_shown = false;
			_changed = true;
		}

		public void SetValue(int v)
		{
			v = Mathf.Clamp(v, _value, _maximum);
			if (!_changed)
			{
				_changed = _value != v;
			}
			_value = v;
			Show();
		}

		private void Show()
		{
			if (_changed && !_shown && _value >= _maximum)
			{
				GameAnalytics.EventAchievementReceived(this);
			}
		}

		private float GetPercent()
		{
			if (_maximum <= 0)
			{
				return 0f;
			}
			float value = (float)_value / (float)_maximum;
			return Mathf.Clamp01(value) * 100f;
		}

		public bool LoadXml(XmlElement root)
		{
			if (root == null)
			{
				return false;
			}
			_shown = XmlUtils.GetAttribute<bool>(root, "shown");
			_value = XmlUtils.GetAttribute<int>(root, "value");
			_value = Mathf.Clamp(_value, 0, _maximum);
			return true;
		}

		public void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlUtils.SetAttribute(root, "id", Id);
			XmlUtils.SetAttribute(root, "value", _value);
			XmlUtils.SetAttribute(root, "shown", _shown);
		}
	}

	private const string SAVE_ACHIEVEMENTS_ID = "id";

	private const string SAVE_ACHIEVEMENTS_VALUE = "value";

	private const string SAVE_ACHIEVEMENTS_SHOWN = "shown";

	private const string SAVE_ACHIEVEMENTS_ACHIEVEMENT = "achievement";

	private Achievement AchievementFirstWin = new Achievement("INDESTTM_ACHIEVE_FIRST_WIN", "IDS_ACHIEVEMENT_FIRST_WIN", 1);

	private Achievement Achievement100Wins = new Achievement("INDESTTM_ACHIEVE_100_WINS", "IDS_ACHIEVEMENT_100_WINS", 100);

	private Achievement AchievementLevel20 = new Achievement("INDESTTM_ACHIEVE_LEVEL_20", "IDS_ACHIEVEMENT_LEVEL_20", 20);

	private Achievement AchievementSilverLeague = new Achievement("INDESTTM_ACHIEVE_SILVER_LEAGUE", "IDS_ACHIEVEMENT_SILVER_LEAGUE", 1);

	private Achievement AchievementGoldLeague = new Achievement("INDESTTM_ACHIEVE_GOLD_LEAGUE", "IDS_ACHIEVEMENT_GOLD_LEAGUE", 1);

	private Achievement Achievement50CollisionKills = new Achievement("INDESTTM_ACHIEVE_50_COLLISION_KILLS", "IDS_ACHIEVEMENT_50_COLLISION_KILLS", 50);

	private Achievement Achievement50Flags = new Achievement("INDESTTM_ACHIEVE_50_FLAGS", "IDS_ACHIEVEMENT_50_FLAGS", 50);

	private Achievement Achievement50FlagsCarriersKills = new Achievement("INDESTTM_ACHIEVE_KILL_50_FLAGS_CARRIERS", "IDS_ACHIEVEMENT_KILL_50_FLAGS_CARRIERS", 50);

	private Achievement Achievement50Charges = new Achievement("INDESTTM_ACHIEVE_50_CHARGES", "IDS_ACHIEVEMENT_50_CHARGES", 50);

	private Achievement AchievementDM5Kills0Deaths = new Achievement("INDESTTM_ACHIEVE_DM_5_KILLS_0_DEATHS", "IDS_ACHIEVEMENT_DM_5_KILLS_0_DEATHS", 1);

	private Achievement Achievement5KillsOn5Vehicles = new Achievement("INDESTTM_ACHIEVE_5_KILLS_ON_5_VEHICLES", "IDS_ACHIEVEMENT_5_KILLS_ON_5_VEHICLES", 5);

	private Achievement AchievementNewVehicle = new Achievement("INDESTTM_ACHIEVE_NEW_VEHICLE", "IDS_ACHIEVEMENT_NEW_VEHICLE", 1);

	private Achievement AchievementPaitJob = new Achievement("INDESTTM_ACHIEVE_PAINT_JOB", "IDS_ACHIEVEMENT_PAINT_JOB", 1);

	private Achievement AchievementLevel30 = new Achievement("INDESTTM_ACHIEVE_LEVEL_30", "IDS_ACHIEVEMENT_LEVEL_30", 30);

	private Achievement Achievement30BossesCampaign = new Achievement("INDESTTM_ACHIEVE_30_BOSSES_CAMPAIGN", "IDS_ACHIEVEMENT_30_BOSSES", 30);

	private Achievement AchievementShooterTruck = new Achievement("INDESTTM_ACHIEVE_SHOOTER_TRUCK", "IDS_ACHIEVEMENT_SHOOTER_TRUCK", 1);

	private Achievement AchievementDefeatShooter = new Achievement("INDESTTM_ACHIEVE_DEFEAT_SHOOTER", "IDS_ACHIEVEMENT_DEFEAT_SHOOTER", 1);

	private List<Achievement> _achievements = new List<Achievement>();

	public PlayerAchievements()
	{
		_achievements.Add(AchievementFirstWin);
		_achievements.Add(Achievement100Wins);
		_achievements.Add(AchievementLevel20);
		_achievements.Add(AchievementSilverLeague);
		_achievements.Add(AchievementGoldLeague);
		_achievements.Add(Achievement50CollisionKills);
		_achievements.Add(Achievement50Flags);
		_achievements.Add(Achievement50FlagsCarriersKills);
		_achievements.Add(Achievement50Charges);
		_achievements.Add(AchievementDM5Kills0Deaths);
		_achievements.Add(Achievement5KillsOn5Vehicles);
		_achievements.Add(AchievementNewVehicle);
		_achievements.Add(AchievementPaitJob);
		_achievements.Add(AchievementLevel30);
		_achievements.Add(Achievement30BossesCampaign);
		_achievements.Add(AchievementShooterTruck);
		_achievements.Add(AchievementDefeatShooter);
	}

	public Achievement Find(string id)
	{
		Achievement achievement = _achievements.Find((Achievement a) => a.Id == id);
		if (achievement == null)
		{
			return null;
		}
		return achievement;
	}

	public void UpdateGarage()
	{
		AchievementSilverLeague.SetValue((MonoSingleton<Player>.Instance.League > 0) ? 1 : 0);
		AchievementGoldLeague.SetValue((MonoSingleton<Player>.Instance.League > 1) ? 1 : 0);
		AchievementLevel20.SetValue(MonoSingleton<Player>.Instance.Level);
		AchievementLevel30.SetValue(MonoSingleton<Player>.Instance.Level);
		Achievement30BossesCampaign.SetValue(BossFightConfiguration.Instance.BossesCount(MonoSingleton<Player>.Instance.LastWonBossFight + 1));
		AchievementDefeatShooter.SetValue(MonoSingleton<Player>.Instance.CampaignCompleted ? 1 : 0);
		int num = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Vehicle);
		AchievementNewVehicle.SetValue((num > 1) ? 1 : 0);
		GarageVehicle garageVehicle = MonoSingleton<Player>.Instance.FindBoughtVehicle("vehicle_ckz");
		AchievementShooterTruck.SetValue((garageVehicle != null) ? 1 : 0);
		int num2 = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Body);
		int num3 = num2 - num;
		AchievementPaitJob.SetValue((num3 > 0) ? 1 : 0);
	}

	public void UpdateGameOver()
	{
		PlayerStatistics statistics = MonoSingleton<Player>.Instance.Statistics;
		AchievementFirstWin.SetValue(statistics.MultiplayerGamesWon);
		Achievement100Wins.SetValue(statistics.MultiplayerGamesWon);
		Achievement50CollisionKills.SetValue(statistics.MultiplayerTotalKillsCollision);
		Achievement50Flags.SetValue(statistics.MultiplayerTotalFlagsCaptured);
		Achievement50FlagsCarriersKills.SetValue(statistics.MultiplayerTotalKillsFlagCarriers);
		Achievement50Charges.SetValue(statistics.MultiplayerTotalChargesCaptured);
		Achievement5KillsOn5Vehicles.SetValue(statistics.Vehikill());
	}

	public void UpdateGameSpecific(IDTGame game)
	{
		DeathmatchGame deathmatchGame = game as DeathmatchGame;
		if (deathmatchGame != null && deathmatchGame.match.isOnline)
		{
			GamePlayer localPlayer = deathmatchGame.localPlayer;
			bool flag = localPlayer.killCount > 4 && localPlayer.deathCount < 1;
			AchievementDM5Kills0Deaths.SetValue(flag ? 1 : 0);
		}
	}

	public void Send()
	{
	}

	public void SetDefault()
	{
		foreach (Achievement achievement in _achievements)
		{
			achievement.Reset();
		}
	}

	public bool LoadXml(XmlElement root)
	{
		if (root == null)
		{
			return true;
		}
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			Achievement achievement = Find(attribute);
			if (achievement != null)
			{
				achievement.LoadXml(item);
			}
		}
		return true;
	}

	public void SaveXml(XmlDocument document, XmlElement root)
	{
		foreach (Achievement achievement in _achievements)
		{
			XmlElement xmlElement = document.CreateElement("achievement");
			achievement.SaveXml(document, xmlElement);
			root.AppendChild(xmlElement);
		}
	}
}
