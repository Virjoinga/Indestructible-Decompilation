using System.Collections;
using Glu.Localization;
using UnityEngine;

public class GameplayGUIScene : MonoBehaviour
{
	private DestructionReason _lastDestructionReason;

	private void Awake()
	{
		Input.multiTouchEnabled = true;
	}

	private void OnDestroy()
	{
		Input.multiTouchEnabled = false;
		RemoveGameDelegates();
		DestroyGameInstance();
		if (MonoSingleton<GameController>.Exists())
		{
			MonoSingleton<GameController>.Instance.suspendEvent -= OnPauseGameTap;
		}
	}

	private void Start()
	{
		IDTGame instance = IDTGame.Instance;
		if (!(instance == null))
		{
			string prefabName = null;
			if (instance is CTFGame)
			{
				prefabName = "GUIGameCTF";
			}
			else if (instance is CRTeamGame)
			{
				prefabName = "GUIGameCRS";
			}
			else if (instance is KOHGame)
			{
				prefabName = "GUIGameKOH";
			}
			else if (instance is DeathmatchGame)
			{
				prefabName = "GUIGameDeathmatch";
			}
			else if (instance is TeamDeathmatchGame)
			{
				prefabName = "GUIGameTeamDeathmatch";
			}
			else if (instance is SingleGame)
			{
				prefabName = "GUIGameSingle";
			}
			InstantiateDialog(prefabName);
			AddGameDelegates();
		}
	}

	private void InstantiateDialog(string prefabName)
	{
		if (!string.IsNullOrEmpty(prefabName))
		{
			string path = "Dialogs/" + prefabName;
			Object @object = Resources.Load(path);
			if (@object != null)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(@object);
				if (gameObject != null)
				{
					Transform component = gameObject.GetComponent<Transform>();
					component.position = Vector3.zero;
				}
			}
		}
		MonoSingleton<GameController>.Instance.suspendEvent += OnPauseGameTap;
	}

	private void AddGameDelegates()
	{
		IDTGame instance = IDTGame.Instance;
		instance.gameOverEvent += OnGameOver;
		if (instance is SurvivalGame)
		{
			SurvivalGame survivalGame = instance as SurvivalGame;
			survivalGame.waveCompleteEvent += SurvivalGameWaveComplete;
		}
		else if (instance is MultiplayerGame)
		{
			MultiplayerGame multiplayerGame = instance as MultiplayerGame;
			multiplayerGame.playerKillEnemyEvent += PlayerKillEnemy;
			if (multiplayerGame.match != null && multiplayerGame.match.isOnline)
			{
				multiplayerGame.match.matchStartedEvent += MultiplayerMatchStarted;
			}
		}
		VehiclesManager instance2 = VehiclesManager.instance;
		instance2.playerVehicleActivatedEvent += PlayerActivated;
	}

	private void RemoveGameDelegates()
	{
		IDTGame instance = IDTGame.Instance;
		if (instance == null)
		{
			return;
		}
		instance.gameOverEvent -= OnGameOver;
		if (instance is SurvivalGame)
		{
			SurvivalGame survivalGame = instance as SurvivalGame;
			survivalGame.waveCompleteEvent -= SurvivalGameWaveComplete;
		}
		else if (instance is MultiplayerGame)
		{
			MultiplayerGame multiplayerGame = instance as MultiplayerGame;
			multiplayerGame.playerKillEnemyEvent -= PlayerKillEnemy;
			if (multiplayerGame.match != null)
			{
				multiplayerGame.match.matchStartedEvent -= MultiplayerMatchStarted;
			}
		}
		VehiclesManager instance2 = VehiclesManager.instance;
		if (instance2 != null)
		{
			instance2.playerVehicleActivatedEvent -= PlayerActivated;
		}
	}

	private void PlayerActivated(Vehicle vehicle)
	{
		_lastDestructionReason = DestructionReason.Weapon;
		Destructible component = vehicle.GetComponent<Destructible>();
		component.forceDestructedEvent += PlayerForcedDestructed;
	}

	private void PlayerForcedDestructed(Destructible destructed, DestructionReason reason)
	{
		_lastDestructionReason = reason;
		destructed.forceDestructedEvent -= PlayerForcedDestructed;
	}

	private void PlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (IDTGame.Instance is MultiplayerGame)
		{
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			if (enemy == multiplayerGame.localPlayer)
			{
				StartCoroutine(StartRespawnNotification(player, enemy));
			}
		}
	}

	private IEnumerator StartRespawnNotification(GamePlayer player, GamePlayer enemy)
	{
		yield return null;
		if (IDTGame.Instance is MultiplayerGame)
		{
			Object respawnPrefab = Resources.Load("Dialogs/NotificationRespawnMultiplayer");
			GameObject o = (GameObject)Object.Instantiate(respawnPrefab);
			NotificationRespawnMultiplayer i = o.GetComponent<NotificationRespawnMultiplayer>();
			MonoSingleton<NotificationsQueue>.Instance.Show(i);
			i.SetKiller(player, enemy);
		}
	}

	private void DestroyGameInstance()
	{
		IDTGame instance = IDTGame.Instance;
		if (!(instance == null))
		{
			Object.Destroy(instance.gameObject);
		}
	}

	private void SurvivalGameWaveComplete(int waveIndex, int rewardSC, int rewardXP)
	{
		string @string = Strings.GetString("IDS_SURVIVAL_WAVE_COMPLETE");
		MonoSingleton<NotificationsQueue>.Instance.AddText(string.Format(@string, waveIndex + 1));
		@string = Strings.GetString("IDS_SURVIVAL_WAVE_STARTED");
		MonoSingleton<NotificationsQueue>.Instance.AddText(string.Format(@string, waveIndex + 2));
	}

	private void StartGameOverText(ref IDTGame.Reward reward)
	{
		string id = "IDS_GAME_RESULT_LOSE";
		UISounds.Type type = UISounds.Type.Defeat;
		if (reward.Victory)
		{
			id = "IDS_GAME_RESULT_WIN";
			type = UISounds.Type.Victory;
		}
		MonoSingleton<UISounds>.Instance.PlayWithMusicMute(type);
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("Dialogs/NotificationGameOver"));
		AnimatedNotificationText component = gameObject.GetComponent<AnimatedNotificationText>();
		component.GetComponent<Transform>().localPosition = Vector3.zero;
		component.Notification.Text = Strings.GetString(id);
		component.Activate();
	}

	public static string DestructionReasonString(DestructionReason reason)
	{
		switch (reason)
		{
		case DestructionReason.Disconnect:
			return "disconnect";
		case DestructionReason.Killbox:
			return "cliffDeath";
		case DestructionReason.Weapon:
			return "destroyed";
		default:
			return "unknown";
		}
	}

	private bool IsCustomGame(IDTGame game)
	{
		if (game != null && game is MultiplayerGame)
		{
			return game.GetComponent<GCMatchmaker>() != null;
		}
		return false;
	}

	private bool IsOnlineGame(IDTGame game)
	{
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		return multiplayerGame != null && multiplayerGame.match.isOnline;
	}

	private void RemoveCancelEvent(IDTGame game)
	{
		TeamGame teamGame = game as TeamGame;
		if (teamGame != null && teamGame.match != null)
		{
			teamGame.match.canceledEvent -= SelectMatchmakingPanel.MatchCancelledEvent;
		}
	}

	private void MultiplayerMatchStarted(MultiplayerMatch match)
	{
		if (!IDTGame.Instance.IsTutorial && !IDTGame.Instance.IsBossFight)
		{
			MonoSingleton<Player>.Instance.Challenges.OnGameStarted();
		}
	}

	private void OnGameOver(IDTGame game, ref IDTGame.Reward reward)
	{
		StopAllCoroutines();
		RemoveCancelEvent(game);
		MonoSingleton<NotificationsQueue>.Instance.Clear();
		StartGameOverText(ref reward);
		string text = string.Empty;
		if (!MonoSingleton<Player>.Instance.Tutorial.IsFirstGamePlayed())
		{
			AStats.MobileAppTracking.TrackAction("tutorial_complete");
		}
		MonoSingleton<Player>.Instance.Statistics.TotalGamesPlayed++;
		MonoSingleton<Player>.Instance.Tutorial.SetFirstGamePlayed();
		MonoSingleton<Player>.Instance.ConsumeConsumable();
		bool isBossFight = game.IsBossFight;
		string bundleName = null;
		if (reward.Victory && game.BossFightIdx > MonoSingleton<Player>.Instance.LastWonBossFight)
		{
			MonoSingleton<Player>.Instance.LastWonBossFight = game.BossFightIdx;
			if (MonoSingleton<Player>.Instance.LastWonBossFight >= BossFightConfiguration.Instance.BossFights.Length - 1)
			{
				MonoSingleton<Player>.Instance.CampaignCompleted = true;
			}
			BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[game.BossFightIdx];
			if (bossFightConfig.Reward != null)
			{
				bundleName = bossFightConfig.Reward.BundleId;
			}
		}
		if (isBossFight)
		{
			GameAnalytics.EventCampaignBattleResult(reward.Victory);
		}
		if (IsOnlineGame(game) && !IsCustomGame(game))
		{
			MonoSingleton<Player>.Instance.Statistics.Update(game, ref reward);
			MonoSingleton<Player>.Instance.Challenges.OnGameFinished(game, ref reward);
			MonoSingleton<Player>.Instance.Achievements.UpdateGameSpecific(game);
			MonoSingleton<Player>.Instance.Achievements.UpdateGameOver();
			MonoSingleton<Player>.Instance.Achievements.Send();
		}
		else
		{
			text = "Custom ";
		}
		if (IsOnlineGame(game))
		{
			text = "Online ";
			GameAnalytics.EventMultiplayerFinish(ref reward);
			GameAnalytics.EventMultiplayerKillsInGame();
			if (game is CTFGame)
			{
				text = "Online CTF ";
				GameAnalytics.EventMultiplayerFlagsInGame();
				GameAnalytics.EventCTFFinish(game as CTFGame, ref reward);
			}
			if (game is TeamGame)
			{
				text = "Online Team ";
				TeamGame game2 = game as TeamGame;
				MonoSingleton<Player>.Instance.UpdateEloRate(game2, reward.Victory);
			}
			Leaderboards.Update();
		}
		else
		{
			if (game is SingleGame)
			{
				text = "Single ";
				string reason = DestructionReasonString(_lastDestructionReason);
				GameAnalytics.EventSingleFinish(game, reason);
			}
			GameAnalytics.EventTutorialMatchFinished("GameOver");
		}
		MonoSingleton<Player>.Instance.Save();
		StartCoroutine(StartGameOverDialogs(game, reward, isBossFight, bundleName, 4f));
		if (GameConstants.BuildType == "amazon")
		{
			ASocial.Amazon.Sync("Finished " + text + "Game", string.Empty);
		}
	}

	private IEnumerator StartGameOverDialogs(IDTGame game, IDTGame.Reward reward, bool isBossFight, string bundleName, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (IsOnlineGame(game))
		{
			Object prefab = Resources.Load("Dialogs/DialogMultiplayerGameEnd");
			if (prefab != null)
			{
				GameObject o = (GameObject)Object.Instantiate(prefab);
				DialogMultiplayerGameEnd dialog = o.GetComponent<DialogMultiplayerGameEnd>();
				MonoSingleton<DialogsQueue>.Instance.Add(dialog);
				dialog.Fill();
			}
		}
		else
		{
			MonoSingleton<Player>.Instance.StartMatchLeftLevel("GarageScene");
			MonoSingleton<DialogsQueue>.Instance.Pause();
		}
		if (isBossFight)
		{
			Dialogs.BossVictory(reward.Victory);
		}
		if (!IsCustomGame(game))
		{
			Dialogs.GameRewards(ref reward, game);
			Dialogs.LevelUpDialog(reward.LevelsGained);
		}
		if (!string.IsNullOrEmpty(bundleName))
		{
			Dialogs.OpenBundle(bundleName, true);
		}
	}

	private void OnPauseGameTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (!DialogPause.exists)
		{
			Dialogs.PauseGame();
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnPauseGameTap();
		}
	}
}
