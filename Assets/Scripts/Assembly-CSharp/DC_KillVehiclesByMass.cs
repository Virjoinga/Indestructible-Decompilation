public class DC_KillVehiclesByMass : DailyChallenges.DailyChallenge
{
	private float _minMass;

	private float _maxMass;

	private int _kills;

	public DC_KillVehiclesByMass(string id, int goal, float minMass, float maxMass)
		: base(id)
	{
		_goal = goal;
		_minMass = minMass;
		_maxMass = maxMass;
	}

	public override void Reset()
	{
		base.Reset();
		_kills = 0;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
		if (multiplayerGame != null)
		{
			multiplayerGame.playerKillEnemyEvent += PlayerKillEnemy;
		}
		_kills = 0;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null)
		{
			multiplayerGame.playerKillEnemyEvent -= PlayerKillEnemy;
		}
		_value += _kills;
	}

	private void PlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
		if (enemy != null && !(enemy.vehicle == null) && player == multiplayerGame.localPlayer && player != enemy)
		{
			float mass = enemy.vehicle.GetComponent<UnityEngine.Rigidbody>().mass;
			if (mass >= _minMass && mass < _maxMass)
			{
				_kills++;
			}
		}
	}
}
