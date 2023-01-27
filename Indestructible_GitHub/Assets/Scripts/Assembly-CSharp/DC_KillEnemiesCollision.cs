using UnityEngine;

public class DC_KillEnemiesCollision : DailyChallenges.DailyChallenge
{
	private int _totalKillsOnStart;

	public DC_KillEnemiesCollision(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_totalKillsOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsCollision;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		_value += MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsCollision - _totalKillsOnStart;
		Debug.Log("CollisionKills _value: " + _value + " now: " + MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsCollision + " onStart: " + _totalKillsOnStart);
	}
}
