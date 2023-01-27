using Glu.Localization;
using UnityEngine;

public class BossComparePanel : PanelManagerPanel
{
	public BossPlayerInfo PlayerInfo;

	public BossPlayerInfo BossInfo;

	private int _fightIdx = -1;

	public override void OnActivate()
	{
		if (BossFightConfiguration.Instance.BossFights.Length > 0)
		{
			_fightIdx = MonoSingleton<Player>.Instance.LastWonBossFight + 1;
			BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[_fightIdx];
			if (PlayerInfo != null)
			{
				PlayerInfo.SetPlayerDataCompared(bossFightConfig);
			}
			if (BossInfo != null)
			{
				BossInfo.SetDataCompared(bossFightConfig);
			}
		}
	}

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void OnBossFightButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (BossFightConfiguration.Instance.BossFights.Length <= 0)
		{
			return;
		}
		BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[_fightIdx];
		Player instance = MonoSingleton<Player>.Instance;
		instance.LastPlayedType = "single";
		instance.LastPlayedGame = "TeamDeathmatchConf";
		instance.LastPlayedMode = "boss";
		instance.LastPlayedPlayers = 2;
		string[] array = new string[4] { "ctf_aircrash", "dtb_rocketbase", "koh_iceberg", "island" };
		if (string.IsNullOrEmpty(bossFightConfig.MapName))
		{
			instance.LastPlayedMap = array[Random.Range(0, array.Length)];
		}
		else
		{
			instance.LastPlayedMap = bossFightConfig.MapName;
		}
		int num = ((bossFightConfig.Bosses != null) ? Mathf.Min(bossFightConfig.Bosses.Length, 2) : 0);
		if (num > 0)
		{
			GameObject gameObject = new GameObject("Match");
			MultiplayerMatch multiplayerMatch = gameObject.AddComponent<MultiplayerMatch>();
			GamePlayer player = new OfflineGamePlayer(1, instance.Name, instance.EloRate, instance.League, instance.SelectedVehicle.Vehicle.prefab);
			string game = ((!string.IsNullOrEmpty(bossFightConfig.GameConfig)) ? bossFightConfig.GameConfig : "BossFightConf");
			multiplayerMatch.CreateOfflineMatch(2, 4, 2, instance.LastPlayedMap, SelectManager.GetGameModeConfig(game), player);
			for (int i = 0; i < num; i++)
			{
				BossConfig bossConfig = bossFightConfig.Bosses[i];
				multiplayerMatch.JoinPlayer(new OfflineGamePlayer(i + 2, Strings.GetString(bossConfig.Name), 0, 0, bossConfig.Prefab), 1, false);
			}
			multiplayerMatch.StartMatch();
		}
		else
		{
			string text = ((!string.IsNullOrEmpty(bossFightConfig.GameConfig)) ? bossFightConfig.GameConfig : "BossFightHordeConf");
			if (string.IsNullOrEmpty(bossFightConfig.GameConfig))
			{
				BossFightHordeConf bossFightHordeConf = ScriptableObject.CreateInstance<BossFightHordeConf>();
				bossFightHordeConf.HordeWave = bossFightConfig.WaveNumber;
				CustomSinglePlayStarter.StartCustomGame(bossFightHordeConf, instance.LastPlayedMap);
			}
			else
			{
				CustomSinglePlayStarter.StartCustomGame(bossFightConfig.GameConfig, instance.LastPlayedMap);
			}
		}
	}

	private void OnCustomizeButtonTap()
	{
		MonoSingleton<Player>.Instance.StartMatchLeftLevel("GarageScene");
		MonoSingleton<DialogsQueue>.Instance.Pause();
		Dialogs.OpenCustomizeLoadoutPanelDummy();
	}
}
