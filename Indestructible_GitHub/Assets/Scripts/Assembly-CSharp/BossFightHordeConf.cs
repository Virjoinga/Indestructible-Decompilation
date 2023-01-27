using UnityEngine;

public class BossFightHordeConf : GameModeConf
{
	public int HordeWave;

	public override void Configure(MultiplayerMatch match)
	{
	}

	public override void Configure(GameObject gameGO)
	{
		SurvivalGame survivalGame = gameGO.AddComponent<SurvivalGame>();
		survivalGame.SetBossFightIdx(MonoSingleton<Player>.Instance.LastWonBossFight + 1);
		Configure(survivalGame);
		GameModeConf.ConfigureScene("[single]");
		EnemyWaveController enemyWaveController = Object.FindObjectOfType(typeof(EnemyWaveController)) as EnemyWaveController;
		if ((bool)enemyWaveController)
		{
			enemyWaveController.SetCustomWave(HordeWave);
		}
	}
}
