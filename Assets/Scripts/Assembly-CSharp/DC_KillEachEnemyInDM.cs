public class DC_KillEachEnemyInDM : DailyChallenges.DailyChallenge
{
	private DeathmatchGame _dmGame;

	private int _opponentsBits;

	private int _killsBits;

	public DC_KillEachEnemyInDM(string id)
		: base(id)
	{
		_goal = 1;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_dmGame = IDTGame.Instance as DeathmatchGame;
		if (!(_dmGame != null))
		{
			return;
		}
		_dmGame.playerKillEnemyEvent += OnPlayerKillEnemy;
		foreach (MatchPlayer player in _dmGame.match.players)
		{
			if (player.id != _dmGame.localPlayer.id)
			{
				_opponentsBits |= 1 << player.id;
			}
		}
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (_dmGame != null && player == _dmGame.localPlayer && player != enemy)
		{
			_killsBits |= 1 << enemy.id;
		}
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (_dmGame != null && _killsBits == _opponentsBits)
		{
			_value = 1;
		}
	}
}
