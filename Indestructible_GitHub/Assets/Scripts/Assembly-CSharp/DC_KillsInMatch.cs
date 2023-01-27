public class DC_KillsInMatch : DailyChallenges.DailyChallenge
{
	private int _targetKills;

	public DC_KillsInMatch(string id, int targetKills)
		: base(id)
	{
		_goal = 1;
		_targetKills = targetKills;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null && multiplayerGame.localPlayer.killCount >= _targetKills)
		{
			_value = 1;
		}
	}
}
