public class DC_Wins : DailyChallenges.DailyChallenge
{
	public DC_Wins(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (reward.Victory)
		{
			_value++;
		}
	}
}
