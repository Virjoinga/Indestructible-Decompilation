public class DC_CaptureFlags : DailyChallenges.DailyChallenge
{
	public DC_CaptureFlags(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		CTFGame cTFGame = game as CTFGame;
		if (cTFGame != null)
		{
			_value += cTFGame.localPlayer.score;
		}
	}
}
