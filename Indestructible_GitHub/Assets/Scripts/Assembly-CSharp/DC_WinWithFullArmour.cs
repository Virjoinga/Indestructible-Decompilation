public class DC_WinWithFullArmour : DailyChallenges.DailyChallenge
{
	public DC_WinWithFullArmour(string id)
		: base(id)
	{
		_goal = 1;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null)
		{
			Destructible component = multiplayerGame.localPlayer.vehicle.GetComponent<Destructible>();
			if (reward.Victory && component.hp == component.GetMaxHP())
			{
				_value = 1;
			}
		}
	}
}
