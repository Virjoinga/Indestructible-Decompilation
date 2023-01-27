public class DC_KillWithActiveAbility : DailyChallenges.DailyChallenge
{
	private MultiplayerGame _mpGame;

	public DC_KillWithActiveAbility(string id, int targetKills)
		: base(id)
	{
		_goal = targetKills;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_mpGame = IDTGame.Instance as MultiplayerGame;
		if (_mpGame != null)
		{
			_mpGame.playerKillEnemyEvent += OnPlayerKillEnemy;
		}
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (player == _mpGame.localPlayer && enemy != _mpGame.localPlayer && Weapon.IsSecondaryDamageType(damageType))
		{
			_value++;
		}
	}
}
