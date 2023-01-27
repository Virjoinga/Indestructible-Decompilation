public class DC_FirstKill : DailyChallenges.DailyChallenge
{
	private MultiplayerGame _mpGame;

	private bool _firstKillDone;

	public DC_FirstKill(string id)
		: base(id)
	{
		_goal = 1;
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
		if (player == _mpGame.localPlayer && player != enemy && !_firstKillDone)
		{
			_value = 1;
		}
		if (player != enemy)
		{
			_firstKillDone = true;
		}
	}
}
