public class DC_KillChargeCarriers : DailyChallenges.DailyChallenge
{
	public DC_KillChargeCarriers(string id, int goal)
		: base(id)
	{
		_goal = goal;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		CRTeamGame cRTeamGame = game as CRTeamGame;
		if (cRTeamGame != null)
		{
			_value += cRTeamGame.chargeCourierKills;
		}
	}
}
