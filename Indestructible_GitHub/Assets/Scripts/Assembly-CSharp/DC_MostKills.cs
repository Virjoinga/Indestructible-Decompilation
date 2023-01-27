public class DC_MostKills : DailyChallenges.DailyChallenge
{
	public DC_MostKills(string id)
		: base(id)
	{
		_goal = 1;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (_value > 0)
		{
			return;
		}
		_value = 1;
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (!(multiplayerGame != null))
		{
			return;
		}
		foreach (MatchPlayer player in multiplayerGame.match.players)
		{
			GamePlayer gamePlayer = player as GamePlayer;
			if (gamePlayer != null && multiplayerGame.localPlayer != gamePlayer && multiplayerGame.localPlayer.killCount <= gamePlayer.killCount)
			{
				_value = 0;
				break;
			}
		}
	}
}
