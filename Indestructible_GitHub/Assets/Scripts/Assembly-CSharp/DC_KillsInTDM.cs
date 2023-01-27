public class DC_KillsInTDM : DailyChallenges.DailyChallenge
{
	public DC_KillsInTDM(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		TeamDeathmatchGame teamDeathmatchGame = game as TeamDeathmatchGame;
		if (teamDeathmatchGame != null)
		{
			_value += teamDeathmatchGame.localPlayer.score;
		}
	}
}
