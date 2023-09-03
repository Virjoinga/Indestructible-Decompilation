using UnityEngine;

public class DC_KillEnemies : DailyChallenges.DailyChallenge
{
	private int _totalKillsOnStart;

	public DC_KillEnemies(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_totalKillsOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKills;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		_value += MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKills - _totalKillsOnStart;
		Debug.Log("Kills _value: " + _value + " now: " + MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKills + " onStart: " + _totalKillsOnStart);
	}
}
