public class DC_CRWin : DailyChallenges.DailyChallenge
{
	private int _targetPlayerScore;

	private int _targetEnemyScore;

	public DC_CRWin(string id, int playerScore, int enemyScore)
		: base(id)
	{
		_goal = 1;
		_targetPlayerScore = playerScore;
		_targetEnemyScore = enemyScore;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		CRTeamGame cRTeamGame = game as CRTeamGame;
		if (cRTeamGame != null)
		{
			int id = (cRTeamGame.match.localTeam.id + 1) % 2;
			if (TeamGame.GetData(cRTeamGame.match.localTeam).score == _targetPlayerScore && TeamGame.GetData(cRTeamGame.match.GetTeam(id)).score == _targetEnemyScore)
			{
				_value = 1;
			}
		}
	}
}
