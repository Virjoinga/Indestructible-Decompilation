public class DC_TDMWin : DailyChallenges.DailyChallenge
{
	private int _targetPlayerScore;

	public DC_TDMWin(string id, int targetScore)
		: base(id)
	{
		_goal = 1;
		_targetPlayerScore = targetScore;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		TeamDeathmatchGame teamDeathmatchGame = game as TeamDeathmatchGame;
		if (teamDeathmatchGame != null && reward.Victory && teamDeathmatchGame.localPlayer.score >= _targetPlayerScore)
		{
			_value = 1;
		}
	}
}
