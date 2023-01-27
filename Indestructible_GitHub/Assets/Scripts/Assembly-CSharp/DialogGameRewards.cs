using Glu.Localization;
using UnityEngine;

public class DialogGameRewards : UIDialog
{
	public SpriteText Title;

	public SpriteText CoinsEarned;

	public SpriteText ExperienceEarned;

	public SpriteText InfluencePointsEarned;

	public SpriteText InfluencePointsText;

	public SpriteText InfluencePointsRemaining;

	public SpriteText WavesCleared;

	public SpriteText EnemiesKilled;

	public PackedSprite LeagueCurrentSmall;

	public PackedSprite LeagueNextSmall;

	public InclinedProgressBar MeterGreen;

	public InclinedProgressBar MeterRed;

	public InclinedProgressBar MeterBlue;

	public PlayerBoostButton BoostButton;

	public UIButton CloseButton;

	public GameObject ObjectGameSurvival;

	public Transform ObjectInfluencePoints;

	public Transform ObjectRewardItems;

	public Transform ObjectContent;

	public AudioClip MoneyAudioClip;

	public AudioClip XPAudioClip;

	public AudioClip LeagueAudioClip;

	private bool _networkGame;

	private int _leagueChange;

	private int _oldPoints;

	private int _newPoints;

	private float _time = -1f;

	private int _pointsCached = -1;

	private int _leagueCached = -1;

	private int _levelsGained;

	private int _money;

	private int _moneyCached = -1;

	private int _experience;

	private int _experienceCached = -1;

	private bool _experienceFilled;

	private bool _leaguesFilled;

	private bool _moneyFilled;

	private IDTGame.Reward _reward;

	private AudioSource _audio;

	private AudioHelper _audioHelper;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void SetLeagueIcon(PackedSprite sprite, int league)
	{
		string leagueSpriteName = Player.GetLeagueSpriteName(league);
		sprite.PlayAnim(leagueSpriteName);
	}

	protected override void Awake()
	{
		base.Awake();
		_audio = GetComponent<AudioSource>();
		if ((bool)_audio)
		{
			_audioHelper = new AudioHelper(_audio, false, false);
		}
	}

	private void UpdatePoints(int points)
	{
		if (points == _pointsCached)
		{
			return;
		}
		_pointsCached = points;
		int num = points - _oldPoints;
		InfluencePointsEarned.Text = num.ToString("+0;-0");
		int league = MonoSingleton<Player>.Instance.GetLeague(points);
		if (points > _oldPoints)
		{
			SetMeterPosition(MeterGreen, league, points);
			SetMeterPosition(MeterRed, 0, 0f);
			SetMeterPosition(MeterBlue, league, _oldPoints);
		}
		else
		{
			SetMeterPosition(MeterGreen, 0, 0f);
			SetMeterPosition(MeterRed, league, _oldPoints);
			SetMeterPosition(MeterBlue, league, points);
		}
		string @string = Strings.GetString("IDS_GAME_REWARDS_IP_CURRENT");
		InfluencePointsText.Text = string.Format(@string, "\u001d", points);
		if (league < 2)
		{
			string leagueName = Player.GetLeagueName(league + 1);
			@string = Strings.GetString("IDS_GAME_REWARDS_IP_REMAINING");
			int num2 = MonoSingleton<Player>.Instance.GetLeagueMin(league + 1) - points;
			InfluencePointsRemaining.Text = string.Format(@string, num2, leagueName);
			return;
		}
		MonoUtils.DetachAndDestroy(LeagueNextSmall);
		MeterBlue.Position = 1f;
		int num3 = points - MonoSingleton<Player>.Instance.GetLeagueMin(league);
		if (num3 > 0)
		{
			@string = Strings.GetString("IDS_GAME_REWARDS_IP_ABOVE");
			InfluencePointsRemaining.Text = string.Format(@string, num3);
		}
		else
		{
			InfluencePointsRemaining.Text = string.Empty;
		}
	}

	private void SetRewardData(ref IDTGame.Reward reward)
	{
		_money = reward.MoneySoft;
		_experience = reward.ExperiencePoints;
		_levelsGained += reward.LevelsGained;
		_newPoints = MonoSingleton<Player>.Instance.InfluencePoints;
		_oldPoints = _newPoints - reward.InfluencePoints;
		_leagueChange = reward.LeagueChange;
	}

	public void SetData(ref IDTGame.Reward reward, IDTGame game)
	{
		CoinsEarned.Text = string.Empty;
		ExperienceEarned.Text = string.Empty;
		_reward = reward;
		SetRewardData(ref reward);
		Color color = ((_newPoints <= _oldPoints) ? Color.red : Color.green);
		InfluencePointsEarned.SetColor(color);
		InfluencePointsEarned.Text = string.Empty;
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		bool isBossFight = game.IsBossFight;
		if (multiplayerGame != null && !isBossFight)
		{
			string id = "IDS_GAME_REWARDS_DEFEAT";
			if (reward.Victory)
			{
				id = "IDS_GAME_REWARDS_VICTORY";
			}
			Title.Text = Strings.GetString(id);
			MonoUtils.DetachAndDestroy(ObjectGameSurvival);
			UpdatePoints(_oldPoints);
			_networkGame = true;
		}
		else if (game is SurvivalGame || isBossFight)
		{
			string id2 = "IDS_GAME_REWARDS_GAME_OVER";
			if (isBossFight)
			{
				id2 = ((!reward.Victory) ? "IDS_GAME_REWARDS_DEFEAT" : "IDS_GAME_REWARDS_VICTORY");
				WavesCleared.Text = string.Empty;
				EnemiesKilled.Text = string.Empty;
			}
			else
			{
				SurvivalGame survivalGame = game as SurvivalGame;
				string @string = Strings.GetString("IDS_GAME_REWARDS_WAVES_CLEARED");
				WavesCleared.Text = string.Format(@string, survivalGame.lastCompleteWaveIndex + 1);
				@string = Strings.GetString("IDS_GAME_REWARDS_ENEMIES_KILLED");
				EnemiesKilled.Text = string.Format(@string, survivalGame.killCount);
			}
			Title.Text = Strings.GetString(id2);
			MonoUtils.DetachAndDestroy(ObjectInfluencePoints);
			UIExpandSprite component = GetComponent<UIExpandSprite>();
			component.SetHeight(140f);
			Vector3 localPosition = ObjectRewardItems.localPosition;
			ObjectRewardItems.localPosition = new Vector3(localPosition.x, 7f, localPosition.z);
			Transform component2 = Title.GetComponent<Transform>();
			localPosition = component2.localPosition;
			component2.localPosition = new Vector3(localPosition.x, 54f, localPosition.z);
			_leaguesFilled = true;
		}
	}

	private void SetLeagueIcons(int league)
	{
		SetLeagueIcon(LeagueCurrentSmall, league);
		SetLeagueIcon(LeagueNextSmall, league + 1);
	}

	private void OnDoubleRewardsButtonTap()
	{
		BoostButton.Buy();
	}

	public void OnBought(IAPShopItemBoost item, bool boostPresent)
	{
		if (!boostPresent)
		{
			IDTGame.Reward reward = default(IDTGame.Reward);
			reward.ExperiencePoints = _reward.ExperiencePoints;
			reward.MoneySoft = _reward.MoneySoft;
			MonoSingleton<Player>.Instance.AddReward(ref reward, "CREDIT_SC", "Boost Bought");
			MonoSingleton<Player>.Instance.BoostReward(ref reward);
			SetRewardData(ref reward);
			_experienceFilled = false;
			_moneyFilled = false;
			_time = 0f;
		}
		BoostButton.GetNextBoost();
	}

	private void UpdateLeagueIcons(int points)
	{
		int league = MonoSingleton<Player>.Instance.GetLeague(points);
		if (league != _leagueCached)
		{
			_leagueCached = league;
			SetLeagueIcons(league);
			Dialogs.LeagueChange(_leagueChange);
			_leagueChange = 0;
		}
	}

	public override void Activate()
	{
		base.Activate();
		int league = MonoSingleton<Player>.Instance.GetLeague(_oldPoints);
		SetLeagueIcons(league);
		_leagueCached = league;
		BoostButton.GetNextBoost();
	}

	private void SetMeterPosition(InclinedProgressBar meter, int league, float points)
	{
		float num = MonoSingleton<Player>.Instance.GetLeagueMin(league);
		float num2 = MonoSingleton<Player>.Instance.GetLeagueRange(league);
		if (num2 == 0f)
		{
			meter.Position = 1f;
		}
		else
		{
			meter.Position = Mathf.Clamp01((points - num) / num2);
		}
	}

	private void UpdateExperience()
	{
		float num = Mathf.Clamp01(_time);
		int num2 = Mathf.RoundToInt(num * (float)_experience);
		if (num2 != _experienceCached)
		{
			_experienceCached = num2;
			ExperienceEarned.Text = num2.ToString("+0;-0");
		}
	}

	private void UpdateMoney()
	{
		float num = Mathf.Clamp01(_time);
		int num2 = Mathf.RoundToInt(num * (float)_money);
		if (num2 != _moneyCached)
		{
			_moneyCached = num2;
			CoinsEarned.Text = num2.ToString("+0;-0");
		}
	}

	public void Update()
	{
		if (_time < 1f)
		{
			if (_time < 0f)
			{
				_time = Mathf.Min(_time + Time.deltaTime, 0f);
			}
			else if (!_leaguesFilled)
			{
				if (_time == 0f && _audioHelper != null)
				{
					_audioHelper.clip = LeagueAudioClip;
					_audioHelper.PlayIfEnabled();
				}
				_time += Time.deltaTime / 0.8f;
				if (_networkGame)
				{
					float num = _newPoints - _oldPoints;
					float f = (float)_oldPoints + num * Mathf.Clamp01(_time);
					int points = Mathf.RoundToInt(f);
					UpdateLeagueIcons(points);
					UpdatePoints(points);
				}
				if (_time >= 1f)
				{
					_leaguesFilled = true;
					_time = 0f;
				}
			}
			else if (!_experienceFilled)
			{
				if (_time == 0f && _audioHelper != null)
				{
					_audioHelper.clip = XPAudioClip;
					_audioHelper.PlayIfEnabled();
				}
				_time += Time.deltaTime / 0.8f;
				UpdateExperience();
				if (_time >= 1f)
				{
					_experienceFilled = true;
					_time = 0f;
				}
			}
			else if (!_moneyFilled)
			{
				if (_time == 0f && _audioHelper != null)
				{
					_audioHelper.clip = MoneyAudioClip;
					_audioHelper.PlayIfEnabled();
				}
				_time += Time.deltaTime / 0.8f;
				UpdateMoney();
				if (_time >= 1f)
				{
					_moneyFilled = true;
					_time = 0f;
				}
			}
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
