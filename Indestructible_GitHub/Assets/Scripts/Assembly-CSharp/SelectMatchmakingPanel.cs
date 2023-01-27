using Glu.Localization;

public class SelectMatchmakingPanel : PanelManagerPanel
{
	public SpriteText Text;

	public SpriteText StateText;

	public SpriteText TitleText;

	public SpriteText ServerRegionText;

	public MultiplayerMatch match;

	public UIButton MinPlayerCountButton;

	private SelectManager _manager;

	private void StartMatchmaking(SelectManager m)
	{
		string text = BuildTag.Get();
		text = text.Replace("INDESTRUCTIBLE", string.Empty);
		string text2 = m.SelectedArea + text;
		RankedMatchmaker rankedMatchmaker = match.GetComponent<RankedMatchmaker>();
		if (rankedMatchmaker != null)
		{
			rankedMatchmaker.Cancel();
		}
		else
		{
			rankedMatchmaker = match.gameObject.AddComponent<RankedMatchmaker>();
		}
		string lobbyName = text2 + m.SelectedGame;
		rankedMatchmaker.StartMatchmaking(lobbyName, m.SelectedPlayers, (!(m.SelectedGame == "DeathmatchConf")) ? 2 : 0, m.SelectedMap, SelectManager.GetGameModeConfig(m.SelectedGame), MatchmakingDelegate);
	}

	private void MatchmakingDelegate(RankedMatchmaker matchmaker, RankedMatchmaker.State state)
	{
		if (RankedMatchmaker.IsFinished(state))
		{
			switch (state)
			{
			case RankedMatchmaker.State.Succeeded:
				_manager.SelectedMap = matchmaker.match.levelName;
				MonoSingleton<Player>.Instance.LastPlayedMap = matchmaker.match.levelName;
				matchmaker.match.canceledEvent += MatchCancelledEvent;
				break;
			case RankedMatchmaker.State.Failed:
				Dialogs.MatchmakingFailed(Owner);
				GameAnalytics.EventMatchmakingFailed("Connection error");
				break;
			}
			if (MonoSingleton<NotificationsQueue>.Exists())
			{
				MonoSingleton<NotificationsQueue>.Instance.Clear();
			}
			return;
		}
		switch (state)
		{
		case RankedMatchmaker.State.Connecting:
			UpdateServerRegion(false);
			StateText.Text = Strings.GetString("IDS_SELECT_MATCHMAKING_CONNECTING");
			MonoUtils.SetActive(MinPlayerCountButton, false);
			break;
		case RankedMatchmaker.State.Searching:
		{
			UpdateServerRegion(true);
			string string3 = Strings.GetString("IDS_SELECT_MATCHMAKING_PLAYERS_FOUND");
			StateText.Text = string.Format(string3, matchmaker.playerCount);
			break;
		}
		case RankedMatchmaker.State.WaitingRemotePlayersDisconnection:
			StateText.Text = Strings.GetString("IDS_SELECT_MATCHMAKING_WAITING_PLAYERS");
			MonoUtils.SetActive(MinPlayerCountButton, false);
			break;
		case RankedMatchmaker.State.Joining:
			if (matchmaker.match.readyPlayerCount == 0)
			{
				if (matchmaker.match.minPlayerCount == 0)
				{
					StateText.Text = Strings.GetString("IDS_SELECT_MATCHMAKING_JOINING");
				}
				else
				{
					string @string = Strings.GetString("IDS_SELECT_MATCHMAKING_PLAYERS_JOINED");
					StateText.Text = string.Format(@string, matchmaker.match.playerCount, matchmaker.match.minPlayerCount);
				}
			}
			else
			{
				string string2 = Strings.GetString("IDS_SELECT_MATCHMAKING_PLAYERS_READY");
				StateText.Text = string.Format(string2, matchmaker.match.readyPlayerCount, matchmaker.match.minPlayerCount);
			}
			MonoUtils.SetActive(MinPlayerCountButton, false);
			break;
		}
	}

	public static void StartPractice(MultiplayerMatch match)
	{
		Player instance = MonoSingleton<Player>.Instance;
		GamePlayer player = new OfflineGamePlayer(1, instance.Name, instance.EloRate, instance.League, instance.SelectedVehicle.Vehicle.prefab);
		if (instance.LastPlayedGame == "CTFConf")
		{
			match.CreateOfflineMatch(4, 4, 2, instance.LastPlayedMap, SelectManager.GetGameModeConfig("CTFPracticeConf"), player);
		}
		else if (instance.LastPlayedGame == "DeathmatchConf")
		{
			match.CreateOfflineMatch(4, 4, 0, instance.LastPlayedMap, SelectManager.GetGameModeConfig("DeathmatchPracticeConf"), player);
		}
		else if (instance.LastPlayedGame == "TeamDeathmatchConf")
		{
			match.CreateOfflineMatch(4, 4, 2, instance.LastPlayedMap, SelectManager.GetGameModeConfig("TDMPracticeConf"), player);
		}
		else
		{
			match.CreateOfflineMatch(4, 4, 2, instance.LastPlayedMap, SelectManager.GetGameModeConfig(instance.LastPlayedGame), player);
		}
		match.JoinPlayer(new OfflineGamePlayer(2, "Jack", 0, 0, "ZambeziAI"), -1, false);
		match.JoinPlayer(new OfflineGamePlayer(3, "John", 0, 0, "DemolisherAI"), -1, false);
		match.JoinPlayer(new OfflineGamePlayer(4, "Jane", 0, 0, "RavagerAI"), -1, false);
		match.StartMatch();
	}

	private void Cancel()
	{
		if (match != null)
		{
			RankedMatchmaker component = match.GetComponent<RankedMatchmaker>();
			if (component != null)
			{
				component.Cancel();
			}
		}
	}

	private void OnDestroy()
	{
		Cancel();
	}

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Cancel();
		Owner.ActivatePreviousPanel();
	}

	public static void MatchCancelledEvent(MultiplayerMatch match, MultiplayerMatch.CancelReason reason)
	{
		if (MonoSingleton<NotificationsQueue>.Exists())
		{
			MonoSingleton<NotificationsQueue>.Instance.Clear();
		}
		if (reason == MultiplayerMatch.CancelReason.Disconnection)
		{
			Dialogs.GameDisconnected();
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		_manager = Owner as SelectManager;
		MonoUtils.SetActive(MinPlayerCountButton, false);
		UpdateServerRegion(false);
		if (_manager.SelectedMode == "practice")
		{
			StartPractice(match);
		}
		else
		{
			StartMatchmaking(_manager);
		}
		if (_manager.SelectedType == "multiplayer")
		{
			TitleText.Text = SelectManager.GetLocalizedGame(_manager.SelectedGame);
		}
	}

	private void UpdateServerRegion(bool showActualServer)
	{
		if (_manager.SelectedType == "multiplayer")
		{
			RegionServer.Kind serverRegion = MonoSingleton<SettingsController>.Instance.ServerRegion;
			RegionServer.Info serverInfo = RegionServer.GetServerInfo(serverRegion);
			if (serverInfo != null)
			{
				string @string = Strings.GetString("IDS_SELECT_MATCHMAKING_SERVER_REGION");
				@string = string.Format(@string, Strings.GetString(serverInfo.ShortNameId));
				if (showActualServer && serverRegion == RegionServer.Kind.Automatic)
				{
					int currentServerIndex = NetworkManager.instance.currentServerIndex;
					RegionServer.Kind kind = RegionServer.ToKind(currentServerIndex);
					RegionServer.Info serverInfo2 = RegionServer.GetServerInfo(kind);
					@string = @string + " (" + Strings.GetString(serverInfo2.ShortNameId) + ")";
				}
				ServerRegionText.Text = @string;
			}
			else
			{
				ServerRegionText.Text = string.Empty;
			}
		}
		else
		{
			ServerRegionText.Text = string.Empty;
		}
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased())
		{
			OnBackButtonTap();
		}
	}
}
