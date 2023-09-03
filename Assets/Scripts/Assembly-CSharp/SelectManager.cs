using System.Runtime.InteropServices;
using Glu.Localization;
using UnityEngine;

public class SelectManager : PanelManager
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Game
	{
		public const string CaptureTheFlag = "CTFConf";

		public const string CollectResources = "CRConf";

		public const string Deathmatch = "DeathmatchConf";

		public const string TeamDeathmatch = "TeamDeathmatchConf";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Type
	{
		public const string Single = "single";

		public const string Multiplayer = "multiplayer";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Mode
	{
		public const string Matchmaking = "matchmaking";

		public const string Survival = "survival";

		public const string Practice = "practice";

		public const string Boss = "boss";

		public const string Custom = "custom";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Map
	{
		public const string Aircrash = "ctf_aircrash";

		public const string Iceberg = "koh_iceberg";

		public const string RocketBase = "dtb_rocketbase";

		public const string Island = "island";
	}

	public string SelectedType = "single";

	public string SelectedMode = "practice";

	public string SelectedArea = "Public";

	public string SelectedMap = "ctf_aircrash";

	public string SelectedGame = "CTFConf";

	public int SelectedPlayers = 4;

	public int SelectedCustomMatchPlayersMin = 2;

	public int SelectedCustomMatchPlayersMax = 2;

	public static string GetGameModeConfig(string game)
	{
		return "Gameplay/" + game;
	}

	public static string GetLocalizedGame(string game)
	{
		string id = string.Empty;
		switch (game)
		{
		case "CTFConf":
			id = "IDS_SELECT_TITLE_CAPTURE_THE_FLAG";
			break;
		case "CRConf":
			id = "IDS_SELECT_TITLE_COLLECT_RESOURCES";
			break;
		case "DeathmatchConf":
			id = "IDS_SELECT_TITLE_DEATHMATCH";
			break;
		case "TeamDeathmatchConf":
			id = "IDS_SELECT_TITLE_TEAM_DEATHMATCH";
			break;
		}
		return Strings.GetString(id);
	}

	protected override void Start()
	{
		base.Start();
		ActivatePanel("SelectModePanel");
	}

	public void StartGame()
	{
		MonoSingleton<Player>.Instance.LastPlayedType = SelectedType;
		MonoSingleton<Player>.Instance.LastPlayedMode = SelectedMode;
		MonoSingleton<Player>.Instance.LastPlayedGame = SelectedGame;
		MonoSingleton<Player>.Instance.LastPlayedMap = SelectedMap;
		MonoSingleton<Player>.Instance.LastPlayedPlayers = SelectedPlayers;
		MonoSingleton<Player>.Instance.Statistics.Reset();
		if (SelectedType == "multiplayer")
		{
			if (SelectedMode == "matchmaking" || SelectedMode == "practice")
			{
				string[] array = new string[4] { "koh_iceberg", "ctf_aircrash", "dtb_rocketbase", "island" };
				SelectedMap = array[Random.Range(0, array.Length)];
				MonoSingleton<Player>.Instance.LastPlayedMap = SelectedMap;
				ActivatePanel("SelectMatchmakingPanel");
			}
			else if (SelectedMode == "custom")
			{
				Invite();
			}
		}
		else if (SelectedType == "single")
		{
			if (SelectedMode == "survival")
			{
				MonoSingleton<Player>.Instance.StartMatchLevel(SelectedMap, null);
			}
			GameAnalytics.EventSingleStart(this);
		}
	}

	private void Invite()
	{
		GCMatchmaker.HostMatch(SelectedCustomMatchPlayersMin, SelectedCustomMatchPlayersMax, (!(SelectedGame == "DeathmatchConf")) ? 2 : 0, SelectedMap, GetGameModeConfig(SelectedGame), delegate(GCMatchmaker.Result result, MultiplayerMatch match)
		{
			switch (result)
			{
			case GCMatchmaker.Result.Success:
				match.canceledEvent += SelectMatchmakingPanel.MatchCancelledEvent;
				break;
			case GCMatchmaker.Result.ConnectionFail:
				Dialogs.ConnectionFailed();
				break;
			}
		});
	}
}
