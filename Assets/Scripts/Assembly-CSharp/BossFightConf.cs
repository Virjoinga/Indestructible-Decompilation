public class BossFightConf : GameModeConf
{
	public override void Configure(MultiplayerMatch match)
	{
		TeamDeathmatchGame teamDeathmatchGame = match.gameObject.AddComponent<TeamDeathmatchGame>();
		teamDeathmatchGame.SetBossFightIdx(MonoSingleton<Player>.Instance.LastWonBossFight + 1);
		Configure(teamDeathmatchGame);
		GameModeConf.ConfigureScene("[boss]");
	}
}
