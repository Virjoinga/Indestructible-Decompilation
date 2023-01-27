public class DC_KillWithWeapon : DailyChallenges.DailyChallenge
{
	private DamageType _damageType;

	private MultiplayerGame _mpGame;

	public DC_KillWithWeapon(string id, DamageType type, int targetKills)
		: base(id)
	{
		_goal = targetKills;
		_damageType = type;
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
		if (player == _mpGame.localPlayer && enemy != _mpGame.localPlayer && Weapon.GetBaseDamageType(damageType) == _damageType)
		{
			_value++;
		}
	}
}
