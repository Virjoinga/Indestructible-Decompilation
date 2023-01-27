public class DeathmatchPracticeConf : GameModeConf
{
	public bool Tutorial = true;

	public override void Configure(MultiplayerMatch match)
	{
		DeathmatchGame deathmatchGame = match.gameObject.AddComponent<DeathmatchGame>();
		deathmatchGame.SetIsTutorial(Tutorial);
		Configure(deathmatchGame);
		match.gameObject.AddComponent<AIDMManager>();
		GameModeConf.ConfigureScene("[dm]");
	}
}
