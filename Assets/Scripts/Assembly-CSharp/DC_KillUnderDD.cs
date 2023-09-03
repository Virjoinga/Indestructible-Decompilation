public class DC_KillUnderDD : DailyChallenges.DailyChallenge
{
	private MultiplayerGame _mpGame;

	private BuffSystem _buffSystem;

	public DC_KillUnderDD(string id, int targetKills)
		: base(id)
	{
		_goal = targetKills;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_mpGame = IDTGame.Instance as MultiplayerGame;
		_buffSystem = null;
		if (_mpGame != null)
		{
			_mpGame.playerKillEnemyEvent += OnPlayerKillEnemy;
		}
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		_buffSystem = _buffSystem ?? _mpGame.localPlayer.vehicle.GetComponent<BuffSystem>();
		if (_buffSystem != null && player == _mpGame.localPlayer && enemy != _mpGame.localPlayer)
		{
			DamageBoostBuff damageBoostBuff = _buffSystem.FindBuff<DamageBoostBuff>();
			if (damageBoostBuff != null)
			{
				_value++;
			}
		}
	}
}
