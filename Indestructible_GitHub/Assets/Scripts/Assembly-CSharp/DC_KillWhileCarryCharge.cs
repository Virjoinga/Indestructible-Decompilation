public class DC_KillWhileCarryCharge : DailyChallenges.DailyChallenge
{
	private int _targetKills;

	private int _kills;

	private CRTeamGame _crGame;

	private bool _isChargeCarrier;

	public DC_KillWhileCarryCharge(string id, int targetKills)
		: base(id)
	{
		_goal = 1;
		_targetKills = targetKills;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_crGame = IDTGame.Instance as CRTeamGame;
		if (_crGame != null)
		{
			_crGame.playerKillEnemyEvent += OnPlayerKillEnemy;
			_crGame.chargeCapturedEvent += OnChargeCaptured;
			_crGame.chargeDroppedEvent += OnChargeDropped;
		}
	}

	private void OnChargeCaptured(MatchPlayer player)
	{
		if ((GamePlayer)player == _crGame.localPlayer)
		{
			_isChargeCarrier = true;
		}
	}

	private void OnChargeDropped()
	{
		_isChargeCarrier = false;
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (_isChargeCarrier && player == _crGame.localPlayer && enemy != _crGame.localPlayer)
		{
			_kills++;
			if (_kills >= _targetKills)
			{
				_value = 1;
			}
		}
	}
}
