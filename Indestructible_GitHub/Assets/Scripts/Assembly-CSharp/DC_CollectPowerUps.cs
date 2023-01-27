using UnityEngine;

public class DC_CollectPowerUps : DailyChallenges.DailyChallenge
{
	private int _powerupsCollectedOnStart;

	public DC_CollectPowerUps(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_powerupsCollectedOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollected;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		_value += MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollected - _powerupsCollectedOnStart;
		Debug.Log("Powerups _value: " + _value + " now: " + MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollected + " onStart: " + _powerupsCollectedOnStart);
	}
}
