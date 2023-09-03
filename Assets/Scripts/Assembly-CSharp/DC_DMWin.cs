public class DC_DMWin : DailyChallenges.DailyChallenge
{
	private int _targetKills;

	public DC_DMWin(string id, int targetKills)
		: base(id)
	{
		_goal = 1;
		_targetKills = targetKills;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		DeathmatchGame deathmatchGame = game as DeathmatchGame;
		if (deathmatchGame != null && reward.Victory && deathmatchGame.localPlayer.killCount >= _targetKills)
		{
			_value = 1;
		}
	}
}
