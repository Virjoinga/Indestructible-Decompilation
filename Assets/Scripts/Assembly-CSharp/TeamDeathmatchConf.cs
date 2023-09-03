public class TeamDeathmatchConf : GameModeConf
{
	public bool Tutorial;

	public override void Configure(MultiplayerMatch match)
	{
		TeamDeathmatchGame teamDeathmatchGame = match.gameObject.AddComponent<TeamDeathmatchGame>();
		teamDeathmatchGame.SetIsTutorial(Tutorial);
		Configure(teamDeathmatchGame);
		match.gameObject.AddComponent<AIDMManager>();
		GameModeConf.ConfigureScene("[tdm]");
	}
}
