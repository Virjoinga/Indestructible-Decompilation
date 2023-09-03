using UnityEngine;

public class DC_KillFlagsCarrier : DailyChallenges.DailyChallenge
{
	private int _totalKillsOnStart;

	public DC_KillFlagsCarrier(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_totalKillsOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsFlagCarriers;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		_value += MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsFlagCarriers - _totalKillsOnStart;
		Debug.Log("KillsFlagCarriers _value: " + _value + " now: " + MonoSingleton<Player>.Instance.Statistics.MultiplayerTotalKillsFlagCarriers + " onStart: " + _totalKillsOnStart);
	}
}
