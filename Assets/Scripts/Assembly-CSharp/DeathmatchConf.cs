public class DeathmatchConf : GameModeConf
{
	public bool Tutorial;

	public override void Configure(MultiplayerMatch match)
	{
		DeathmatchGame deathmatchGame = match.gameObject.AddComponent<DeathmatchGame>();
		deathmatchGame.SetIsTutorial(Tutorial);
		Configure(deathmatchGame);
		GameModeConf.ConfigureScene("[dm]");
	}
}
