public class DC_KillsInDM : DailyChallenges.DailyChallenge
{
	public DC_KillsInDM(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		DeathmatchGame deathmatchGame = game as DeathmatchGame;
		if (deathmatchGame != null)
		{
			_value += deathmatchGame.localPlayer.score;
		}
	}
}
