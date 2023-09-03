public class DC_KillsInRow : DailyChallenges.DailyChallenge
{
	private int _targetKills;

	private MultiplayerGame _mpGame;

	private int _killsCountRow;

	public DC_KillsInRow(string id, int targetKills)
		: base(id)
	{
		_goal = 1;
		_targetKills = targetKills;
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
		if (player == _mpGame.localPlayer && enemy != _mpGame.localPlayer)
		{
			_killsCountRow++;
			if (_killsCountRow >= _targetKills)
			{
				_value = 1;
			}
		}
		else
		{
			_killsCountRow = 0;
		}
	}
}
