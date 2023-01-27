public class CRConf : GameModeConf
{
	public bool Tutorial;

	public override void Configure(MultiplayerMatch match)
	{
		CRTeamGame cRTeamGame = match.gameObject.AddComponent<CRTeamGame>();
		cRTeamGame.SetIsTutorial(Tutorial);
		Configure(cRTeamGame);
		GameModeConf.ConfigureScene("[cr]");
	}
}
