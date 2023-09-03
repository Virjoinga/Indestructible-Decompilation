public class DC_CTFWin : DailyChallenges.DailyChallenge
{
	private int _targetPlayerScore;

	private int _targetEnemyScore;

	public DC_CTFWin(string id, int playerScore, int enemyScore)
		: base(id)
	{
		_goal = 1;
		_targetPlayerScore = playerScore;
		_targetEnemyScore = enemyScore;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		CTFGame cTFGame = game as CTFGame;
		if (cTFGame != null)
		{
			int id = (cTFGame.match.localTeam.id + 1) % 2;
			if (TeamGame.GetData(cTFGame.match.localTeam).score == _targetPlayerScore && TeamGame.GetData(cTFGame.match.GetTeam(id)).score == _targetEnemyScore)
			{
				_value = 1;
			}
		}
	}
}
