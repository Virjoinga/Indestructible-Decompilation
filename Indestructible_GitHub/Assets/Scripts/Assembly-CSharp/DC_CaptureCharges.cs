public class DC_CaptureCharges : DailyChallenges.DailyChallenge
{
	public DC_CaptureCharges(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		CRTeamGame cRTeamGame = game as CRTeamGame;
		if (cRTeamGame != null)
		{
			_value += cRTeamGame.localPlayer.score;
		}
	}
}
