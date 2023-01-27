public class CTFConf : GameModeConf
{
	public bool Tutorial;

	public override void Configure(MultiplayerMatch match)
	{
		CTFGame cTFGame = match.gameObject.AddComponent<CTFGame>();
		cTFGame.SetIsTutorial(Tutorial);
		Configure(cTFGame);
		GameModeConf.ConfigureScene("[ctf]");
	}
}
