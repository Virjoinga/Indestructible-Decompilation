public class DC_ScoreWinningPoint : DailyChallenges.DailyChallenge
{
	private MultiplayerGame _game;

	private bool _lastChargeMine;

	private bool _lastFlagMine;

	private bool _lastKillMine;

	private bool _chargeCarrier;

	private float _maxHealthPercent;

	public DC_ScoreWinningPoint(string id, float maxHealthPercent)
		: base(id)
	{
		_goal = 1;
		_maxHealthPercent = maxHealthPercent;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_game = IDTGame.Instance as MultiplayerGame;
		if (_game is DeathmatchGame || _game is TeamDeathmatchGame)
		{
			_game.playerKillEnemyEvent += OnPlayerKillEnemy;
		}
		else if (_game is CTFGame)
		{
			CTFGame cTFGame = _game as CTFGame;
			cTFGame.flagDeliveredEvent += OnFlagDelivered;
		}
		else if (_game is CRTeamGame)
		{
			CRTeamGame cRTeamGame = _game as CRTeamGame;
			cRTeamGame.chargeCapturedEvent += OnChargeCaptured;
			cRTeamGame.teamScoreChangedEvent += OnCRScoreChanged;
		}
	}

	private void OnFlagDelivered(GamePlayer player)
	{
		_lastFlagMine = player == _game.localPlayer;
	}

	private void OnChargeCaptured(MatchPlayer player)
	{
		_chargeCarrier = player == _game.localPlayer;
	}

	private void OnCRScoreChanged(MatchTeam team)
	{
		_lastChargeMine = _chargeCarrier;
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		_lastKillMine = player == _game.localPlayer && enemy != _game.localPlayer;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		Destructible destructible = _game.localPlayer.vehicle.destructible;
		float num = destructible.hp / destructible.GetMaxHP() * 100f;
		if (!reward.Victory || !(num <= _maxHealthPercent))
		{
			return;
		}
		if (_game is DeathmatchGame || _game is TeamDeathmatchGame)
		{
			_game.playerKillEnemyEvent -= OnPlayerKillEnemy;
			if (_lastKillMine)
			{
				_value = 1;
			}
		}
		else if (_game is CTFGame)
		{
			CTFGame cTFGame = _game as CTFGame;
			cTFGame.flagDeliveredEvent -= OnFlagDelivered;
			if (_lastFlagMine)
			{
				_value = 1;
			}
		}
		else if (_game is CRTeamGame)
		{
			CRTeamGame cRTeamGame = _game as CRTeamGame;
			cRTeamGame.chargeCapturedEvent -= OnChargeCaptured;
			cRTeamGame.teamScoreChangedEvent -= OnCRScoreChanged;
			if (_lastChargeMine)
			{
				_value = 1;
			}
		}
	}
}
