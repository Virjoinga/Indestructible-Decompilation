public class DC_KillWhileCarryFlag : DailyChallenges.DailyChallenge
{
	private int _targetKills;

	private int _kills;

	private CTFGame _ctfGame;

	private bool _isFlagCarrier;

	public DC_KillWhileCarryFlag(string id, int targetKills)
		: base(id)
	{
		_goal = 1;
		_targetKills = targetKills;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_ctfGame = IDTGame.Instance as CTFGame;
		if (_ctfGame != null)
		{
			_ctfGame.playerKillEnemyEvent += OnPlayerKillEnemy;
			_ctfGame.flagCapturedEvent += OnFlagCaptured;
			_ctfGame.courierKilledEvent += OnFlagDropped;
			_ctfGame.flagDeliveredEvent += OnFlagDropped;
		}
	}

	private void OnFlagCaptured(GamePlayer player)
	{
		if (player == _ctfGame.localPlayer)
		{
			_isFlagCarrier = true;
		}
	}

	private void OnFlagDropped(GamePlayer player)
	{
		_isFlagCarrier = false;
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (_isFlagCarrier && player == _ctfGame.localPlayer && enemy != _ctfGame.localPlayer)
		{
			_kills++;
			if (_kills >= _targetKills)
			{
				_value = 1;
			}
		}
	}
}
