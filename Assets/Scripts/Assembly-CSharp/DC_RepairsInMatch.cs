public class DC_RepairsInMatch : DailyChallenges.DailyChallenge
{
	private int _repairPowerupsCollectedOnStart;

	private int _targetRepairs;

	public DC_RepairsInMatch(string id, int targetRepairs)
		: base(id)
	{
		_goal = 1;
		_targetRepairs = targetRepairs;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_repairPowerupsCollectedOnStart = MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollectedRepair;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		int num = MonoSingleton<Player>.Instance.Statistics.MultiplayerPowerupsCollectedRepair - _repairPowerupsCollectedOnStart;
		if (num >= _targetRepairs)
		{
			_value = 1;
		}
	}
}
