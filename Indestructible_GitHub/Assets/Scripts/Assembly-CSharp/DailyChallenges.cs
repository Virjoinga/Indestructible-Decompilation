using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Glu.Localization;
using UnityEngine;

public class DailyChallenges
{
	public class DailyChallenge
	{
		protected string _id;

		protected string _titleId;

		protected string _progressId;

		protected int _goal;

		protected int _value;

		protected bool _rewarded;

		public ShopItemCondition Lock = new ShopItemCondition();

		public ShopItemReward Reward = new ShopItemReward();

		public bool Rewarded;

		public int Group;

		public string Id
		{
			get
			{
				return _id;
			}
		}

		public DailyChallenge(string id)
		{
			_id = id;
			string arg = id.ToUpper();
			_titleId = string.Format("IDS_{0}_TITLE", arg);
			_progressId = string.Format("IDS_{0}_PROGRESS", arg);
		}

		public virtual int GetGoal()
		{
			return _goal;
		}

		public virtual int GetValue()
		{
			return _value;
		}

		public virtual bool IsCompleted()
		{
			return _value >= _goal;
		}

		public virtual string GetTitle()
		{
			return Strings.GetString(_titleId);
		}

		public virtual string GetProgressText()
		{
			string @string = Strings.GetString(_progressId);
			return string.Format(@string, _value, _goal);
		}

		public virtual void Reset()
		{
			_value = 0;
			_rewarded = false;
		}

		public virtual void OnGameStarted()
		{
		}

		public virtual void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
		{
		}

		public virtual void LoadXml(XmlElement root)
		{
			_value = XmlUtils.GetAttribute<int>(root, "value");
			Rewarded = XmlUtils.GetAttribute<bool>(root, "rewarded");
		}

		public virtual void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlUtils.SetAttribute(root, "value", _value);
			XmlUtils.SetAttribute(root, "rewarded", Rewarded);
		}
	}

	public class DailyChallengeSettings
	{
		[XmlAttribute("id")]
		public string Id;

		[XmlAttribute("group")]
		public int Group;

		[XmlElement("lock")]
		public ShopItemCondition Lock = new ShopItemCondition();

		[XmlElement("reward")]
		public ShopItemReward Reward = new ShopItemReward();
	}

	[XmlType("settings")]
	public class DailyChallengesSettings
	{
		[XmlElement("reward")]
		public ShopItemReward Reward = new ShopItemReward();

		[XmlArray("challenges")]
		[XmlArrayItem("challenge")]
		public DailyChallengeSettings[] Settings;
	}

	private const string SAVE_DAILY_CHALLENGES_START_TIME = "startTime";

	private const string SAVE_DAILY_CHALLENGES_REWARDED = "rewarded";

	private const string SAVE_DAILY_CHALLENGES_ACTIVE = "active";

	private const string SAVE_DAILY_CHALLENGES_SHOWN = "shown";

	private const string SAVE_DAILY_CHALLENGES_COMPLETE = "complete";

	private const string SAVE_DAILY_CHALLENGE = "challenge";

	private const string SAVE_DAILY_CHALLENGE_ID = "id";

	private const string SAVE_DAILY_CHALLENGE_VALUE = "value";

	private const string SAVE_DAILY_CHALLENGE_REWARDED = "rewarded";

	private long _challengeStartTime;

	private long _challengeDuration = 864000000000L;

	private List<string> _shownChallenges = new List<string>();

	private List<DailyChallenge> _activeChallenges = new List<DailyChallenge>();

	private List<DailyChallenge> _completedChallenges = new List<DailyChallenge>();

	private List<DailyChallenge> _challenges = new List<DailyChallenge>
	{
		new DC_CollectPowerUps("daily_challenge_collect_10_powerups", 10),
		new DC_KillEnemies("daily_challenge_5_kills", 5),
		new DC_Wins("daily_challenge_3_wins", 3),
		new DC_WinWithPowerups("daily_challenge_win_match_with_0_powerups", 0),
		new DC_CaptureFlags("daily_challenge_capture_3_flags", 3),
		new DC_CaptureCharges("daily_challenge_recover_3_charges", 3),
		new DC_KillsInDM("daily_challenge_5_kills_in_dm", 5),
		new DC_KillsInTDM("daily_challenge_5_kills_in_tdm", 5),
		new DC_WinMatchInEveryMode("daily_challenge_win_in_every_mod"),
		new DC_ScoreWinningPoint("daily_challenge_score_point", 100f),
		new DC_MatchOnEveryMap("daily_challenge_play_on_all_maps", false),
		new DC_MatchOnEveryMap("daily_challenge_win_on_all_maps", true),
		new DC_KillEnemiesOnVehicles("daily_challenge_kills_on_3_different_vehicles", 3),
		new DC_KillWithEachWeapon("daily_challenge_kills_with_each_weapon_type"),
		new DC_MostKills("daily_challenge_most_kills"),
		new DC_KillFlagsCarrier("daily_challenge_kill_5_flags_carrier", 5),
		new DC_KillChargeCarriers("daily_challenge_kill_5_charge_carrier", 5),
		new DC_RepairsInMatch("daily_challenge_5_repairs_in_match", 5),
		new DC_FirstKill("daily_challenge_first_kill"),
		new DC_KillWithWeapon("daily_challenge_10_kills_with_kinetic_weapon", DamageType.Kinetic, 10),
		new DC_KillWithWeapon("daily_challenge_10_kills_with_thermal_weapon", DamageType.Thermal, 10),
		new DC_KillWithWeapon("daily_challenge_10_kills_with_explosive_weapon", DamageType.Explosive, 10),
		new DC_WinMatchesOnVehicles("daily_challenge_win_on_3_different_vehicles", 3),
		new DC_KillEachEnemyInDM("daily_challenge_kill_each_opponent_in_dm"),
		new DC_KillWhileCarryFlag("daily_challenge_3_kills_with_flag", 3),
		new DC_KillWhileCarryCharge("daily_challenge_3_kills_with_charge", 3),
		new DC_KillVehiclesByMass("daily_challenge_kill_10_light_vehicles", 10, 0f, 5000f),
		new DC_KillVehiclesByMass("daily_challenge_kill_5_medium_vehicles", 5, 5000f, 20000f),
		new DC_KillVehiclesByMass("daily_challenge_kill_3_heavy_vehicles", 3, 20000f, float.MaxValue),
		new DC_KillEnemiesCollision("daily_challenge_5_collision_kills", 5),
		new DC_CTFWin("daily_challenge_win_ctf_with_3_0", 3, 0),
		new DC_CRWin("daily_challenge_win_rtc_with_3_0", 3, 0),
		new DC_DMWin("daily_challenge_win_dm_with_6_frags", 6),
		new DC_TDMWin("daily_challenge_win_tdm_with_4_or_more", 4),
		new DC_WinWithoutDie("daily_challenge_win_without_dying"),
		new DC_WinsInRow("daily_challenge_win_3_matches_in_row", 3),
		new DC_KillsInMatch("daily_challenge_5_kills_in_single_match", 5),
		new DC_WinWithFullArmour("daily_challenge_win_with_100p_armor"),
		new DC_KillsInRow("daily_challenge_kill_3_enemies_in_row", 3),
		new DC_KillUnderDD("daily_challenge_3_kills_under_dd", 3),
		new DC_KillWithActiveAbility("daily_challenge_kill_enemy_with_active_ability", 1),
		new DC_ScoreWinningPoint("daily_challenge_point_under_5p_armor", 5f)
	};

	public ShopItemReward Reward = new ShopItemReward();

	private bool _rewarded;

	private DailyChallengesSettings LoadXmlSettings()
	{
		TextAsset textAsset = BundlesUtils.Load("Assets/Bundles/Configuration/idt_challenges.xml") as TextAsset;
		if (textAsset != null)
		{
			MemoryStream stream = new MemoryStream(textAsset.bytes, false);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DailyChallengesSettings));
			try
			{
				object obj = xmlSerializer.Deserialize(stream);
				DailyChallengesSettings dailyChallengesSettings = obj as DailyChallengesSettings;
				if (dailyChallengesSettings != null)
				{
					return dailyChallengesSettings;
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}

	public void LoadSettings()
	{
		DailyChallengesSettings dailyChallengesSettings = LoadXmlSettings();
		if (dailyChallengesSettings == null)
		{
			return;
		}
		Reward = dailyChallengesSettings.Reward;
		DailyChallengeSettings[] settings = dailyChallengesSettings.Settings;
		foreach (DailyChallengeSettings dailyChallengeSettings in settings)
		{
			DailyChallenge dailyChallenge = Find(dailyChallengeSettings.Id);
			if (dailyChallenge != null)
			{
				dailyChallenge.Reward = dailyChallengeSettings.Reward;
				dailyChallenge.Group = dailyChallengeSettings.Group;
				dailyChallenge.Lock = dailyChallengeSettings.Lock;
			}
		}
	}

	public List<DailyChallenge> GetCompleted()
	{
		return _completedChallenges;
	}

	public List<DailyChallenge> GetActive()
	{
		return _activeChallenges;
	}

	public DailyChallenge Find(string id)
	{
		DailyChallenge dailyChallenge = _challenges.Find((DailyChallenge c) => c.Id == id);
		if (dailyChallenge == null)
		{
			return null;
		}
		return dailyChallenge;
	}

	public float GetSeconds()
	{
		long ticks = DateTime.Now.Ticks;
		long num = _challengeStartTime + _challengeDuration - ticks;
		if (num < 0)
		{
			num = 0L;
		}
		return num / 10000000;
	}

	private DailyChallenge GetRandomChallenge(int group)
	{
		List<DailyChallenge> list = _challenges.FindAll((DailyChallenge c) => c.Group == group);
		List<DailyChallenge> list2 = list.FindAll((DailyChallenge c) => !c.Lock.Lock());
		List<DailyChallenge> list3 = list2.FindAll((DailyChallenge c) => !_shownChallenges.Contains(c.Id));
		if (list3.Count == 0)
		{
			foreach (DailyChallenge item in list2)
			{
				_shownChallenges.Remove(item.Id);
			}
			list3 = list2;
		}
		if (list3.Count == 0)
		{
			return null;
		}
		System.Random random = new System.Random();
		int index = random.Next(0, list3.Count);
		return list3[index];
	}

	public void StartChallenge()
	{
		DateTime now = DateTime.Now;
		_challengeStartTime = new DateTime(now.Year, now.Month, now.Day).Ticks;
		_rewarded = false;
		_completedChallenges.Clear();
		foreach (DailyChallenge activeChallenge in _activeChallenges)
		{
			_shownChallenges.Add(activeChallenge.Id);
		}
		_activeChallenges.Clear();
		for (int i = 0; i < 3; i++)
		{
			DailyChallenge randomChallenge = GetRandomChallenge(i);
			if (randomChallenge != null)
			{
				_activeChallenges.Add(randomChallenge);
				randomChallenge.Reset();
			}
		}
	}

	public void Update()
	{
		int num = 0;
		if (_activeChallenges.Count > 0)
		{
			int num2 = 0;
			foreach (DailyChallenge activeChallenge in _activeChallenges)
			{
				if (activeChallenge.IsCompleted())
				{
					if (!activeChallenge.Rewarded)
					{
						activeChallenge.Rewarded = true;
						num += MonoSingleton<Player>.Instance.AddReward(activeChallenge.Reward, "GAME");
						GameAnalytics.EventDailyChallengeCompleted(activeChallenge.Id, activeChallenge.Group);
					}
					num2++;
				}
			}
			if (num2 == _activeChallenges.Count)
			{
				if (_activeChallenges.Count > 0)
				{
					_completedChallenges.Clear();
					_completedChallenges.AddRange(_activeChallenges);
					_activeChallenges.Clear();
				}
				if (!_rewarded)
				{
					_rewarded = true;
					num += MonoSingleton<Player>.Instance.AddReward(Reward, "GAME");
					GameAnalytics.EventDailyChallengeAllCompleted();
				}
			}
		}
		Dialogs.LevelUpDialog(num);
		if (GetSeconds() <= 0f)
		{
			StartChallenge();
			MonoSingleton<Player>.Instance.Save();
		}
	}

	public void SetDefault()
	{
		_shownChallenges.Clear();
		_activeChallenges.Clear();
		_completedChallenges.Clear();
		_challengeStartTime = 0L;
		_rewarded = false;
		foreach (DailyChallenge challenge in _challenges)
		{
			challenge.Reset();
		}
	}

	private void LoadXmlActive(XmlElement root)
	{
		if (root == null)
		{
			return;
		}
		_activeChallenges.Clear();
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			DailyChallenge dailyChallenge = Find(attribute);
			if (dailyChallenge != null)
			{
				_activeChallenges.Add(dailyChallenge);
				dailyChallenge.LoadXml(item);
			}
		}
	}

	private void LoadXmlShown(XmlElement root)
	{
		if (root == null)
		{
			return;
		}
		_shownChallenges.Clear();
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			_shownChallenges.Add(attribute);
		}
	}

	private void LoadXmlComplete(XmlElement root)
	{
		if (root == null)
		{
			return;
		}
		_completedChallenges.Clear();
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			DailyChallenge dailyChallenge = Find(attribute);
			if (dailyChallenge != null)
			{
				_completedChallenges.Add(dailyChallenge);
				dailyChallenge.LoadXml(item);
			}
		}
	}

	public bool LoadXml(XmlElement root)
	{
		if (root == null)
		{
			return true;
		}
		_challengeStartTime = XmlUtils.GetAttribute<long>(root, "startTime");
		_rewarded = XmlUtils.GetAttribute<bool>(root, "rewarded");
		LoadXmlActive(root["active"]);
		LoadXmlShown(root["shown"]);
		LoadXmlComplete(root["complete"]);
		return true;
	}

	public void SaveXml(XmlDocument document, XmlElement root)
	{
		XmlUtils.SetAttribute(root, "startTime", _challengeStartTime);
		XmlUtils.SetAttribute(root, "rewarded", _rewarded);
		XmlElement xmlElement = document.CreateElement("active");
		root.AppendChild(xmlElement);
		foreach (DailyChallenge activeChallenge in _activeChallenges)
		{
			XmlElement xmlElement2 = document.CreateElement("challenge");
			XmlUtils.SetAttribute(xmlElement2, "id", activeChallenge.Id);
			activeChallenge.SaveXml(document, xmlElement2);
			xmlElement.AppendChild(xmlElement2);
		}
		XmlElement xmlElement3 = document.CreateElement("shown");
		root.AppendChild(xmlElement3);
		foreach (string shownChallenge in _shownChallenges)
		{
			XmlElement xmlElement4 = document.CreateElement("challenge");
			XmlUtils.SetAttribute(xmlElement4, "id", shownChallenge);
			xmlElement3.AppendChild(xmlElement4);
		}
		XmlElement xmlElement5 = document.CreateElement("complete");
		root.AppendChild(xmlElement5);
		foreach (DailyChallenge completedChallenge in _completedChallenges)
		{
			XmlElement xmlElement6 = document.CreateElement("challenge");
			XmlUtils.SetAttribute(xmlElement6, "id", completedChallenge.Id);
			completedChallenge.SaveXml(document, xmlElement6);
			xmlElement5.AppendChild(xmlElement6);
		}
	}

	public void OnGameStarted()
	{
		foreach (DailyChallenge activeChallenge in _activeChallenges)
		{
			activeChallenge.OnGameStarted();
		}
	}

	public void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		foreach (DailyChallenge activeChallenge in _activeChallenges)
		{
			activeChallenge.OnGameFinished(game, ref reward);
		}
	}
}
