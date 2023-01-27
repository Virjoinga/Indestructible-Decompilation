public class DC_WinWithPowerups : DailyChallenges.DailyChallenge
{
	private int _powerupsCollectedOnStart;

	private int _targetPowerups;

	public DC_WinWithPowerups(string id, int targetPowerups)
		: base(id)
	{
		_goal = 1;
		_targetPowerups = targetPowerups;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_powerupsCollectedOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollected;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (reward.Victory)
		{
			int num = MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollected - _powerupsCollectedOnStart;
			if (num == _targetPowerups)
			{
				_value = 1;
			}
		}
	}
}
