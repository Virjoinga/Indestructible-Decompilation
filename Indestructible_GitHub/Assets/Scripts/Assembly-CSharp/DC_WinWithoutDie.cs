public class DC_WinWithoutDie : DailyChallenges.DailyChallenge
{
	public DC_WinWithoutDie(string id)
		: base(id)
	{
		_goal = 1;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (reward.Victory && multiplayerGame != null && multiplayerGame.localPlayer.deathCount <= 0)
		{
			_value = 1;
		}
	}
}
