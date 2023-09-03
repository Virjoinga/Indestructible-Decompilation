public class DC_WinsInRow : DailyChallenges.DailyChallenge
{
	public DC_WinsInRow(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (IsCompleted())
		{
			return;
		}
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null)
		{
			if (reward.Victory)
			{
				_value++;
			}
			else
			{
				_value = 0;
			}
		}
	}
}
