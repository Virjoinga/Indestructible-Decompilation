using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class NotificationRespawnMultiplayer : UINotification
{
	public SpriteText RespawnText;

	public SpriteText LastColumn;

	public SpriteText KillerText;

	public NotificationRespawnMultiplayerItem[] Items;

	private float _time;

	private int _timeCached;

	private MultiplayerMatch _match;

	private bool _done;

	protected override void Awake()
	{
		base.Awake();
		if (IDTGame.Instance is MultiplayerGame)
		{
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			multiplayerGame.playerKillEnemyEvent += OnPlayerKillEnemy;
			_match = multiplayerGame.match;
			_match.playerDisconnectedEvent += OnMatchPlayerDisconnected;
			_time = multiplayerGame.respawnDelay;
			SetTimeText();
		}
		if (IDTGame.Instance is CTFGame)
		{
			CTFGame cTFGame = IDTGame.Instance as CTFGame;
			cTFGame.flagDeliveredEvent += OnFlagDelivered;
		}
		else if (IDTGame.Instance is CRTeamGame)
		{
			CRTeamGame cRTeamGame = IDTGame.Instance as CRTeamGame;
			cRTeamGame.chargeDissolvedEvent += OnChargeDissolved;
		}
		if (VehiclesManager.instance != null)
		{
			VehiclesManager.instance.playerVehicleActivatedEvent += OnPlayerVehicleActivated;
		}
	}

	private void OnDestroy()
	{
		if (IDTGame.Instance is MultiplayerGame)
		{
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			multiplayerGame.playerKillEnemyEvent -= OnPlayerKillEnemy;
		}
		if (_match != null)
		{
			_match.playerDisconnectedEvent -= OnMatchPlayerDisconnected;
		}
		if (IDTGame.Instance is CTFGame)
		{
			CTFGame cTFGame = IDTGame.Instance as CTFGame;
			cTFGame.flagDeliveredEvent -= OnFlagDelivered;
		}
		else if (IDTGame.Instance is CRTeamGame)
		{
			CRTeamGame cRTeamGame = IDTGame.Instance as CRTeamGame;
			cRTeamGame.chargeDissolvedEvent -= OnChargeDissolved;
		}
		if (VehiclesManager.instance != null)
		{
			VehiclesManager.instance.playerVehicleActivatedEvent -= OnPlayerVehicleActivated;
		}
	}

	private void OnChargeDissolved(MatchTeam team)
	{
		UpdatePlayers();
	}

	private void OnFlagDelivered(GamePlayer player)
	{
		UpdatePlayers();
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		UpdatePlayers();
	}

	private void OnMatchPlayerDisconnected(MultiplayerMatch match, MatchPlayer actor)
	{
		UpdatePlayers();
	}

	private void OnPlayerVehicleActivated(Vehicle vehicle)
	{
		_done = true;
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent -= OnPlayerVehicleActivated;
	}

	private void UpdatePlayers()
	{
		List<MatchPlayer> list = new List<MatchPlayer>(_match.players);
		list.Sort(delegate(MatchPlayer a, MatchPlayer b)
		{
			GamePlayer gamePlayer = a as GamePlayer;
			GamePlayer gamePlayer2 = b as GamePlayer;
			return gamePlayer2.score - gamePlayer.score;
		});
		int num = 0;
		NotificationRespawnMultiplayerItem[] items = Items;
		foreach (NotificationRespawnMultiplayerItem notificationRespawnMultiplayerItem in items)
		{
			if (num < list.Count)
			{
				MonoUtils.SetActive(notificationRespawnMultiplayerItem, true);
				notificationRespawnMultiplayerItem.SetData(list[num] as GamePlayer, _match);
			}
			else
			{
				MonoUtils.SetActive(notificationRespawnMultiplayerItem, false);
			}
			num++;
		}
	}

	public void SetKiller(GamePlayer player, GamePlayer enemy)
	{
		if (player != enemy && player != null)
		{
			string @string = Strings.GetString("IDS_GAMEPLAY_RESPAWN_KILLER");
			KillerText.Text = string.Format(@string, player.name);
		}
		else
		{
			KillerText.Text = Strings.GetString("IDS_GAMEPLAY_RESPAWN_KILLER_SUICIDE");
		}
	}

	protected override void Start()
	{
		base.Start();
		string lastColumnLabelId = DialogMultiplayerGameEnd.GetLastColumnLabelId();
		LastColumn.Text = Strings.GetString(lastColumnLabelId);
		UpdatePlayers();
	}

	private void SetTimeText()
	{
		int num = Mathf.CeilToInt(_time);
		if (num != _timeCached)
		{
			string @string = Strings.GetString("IDS_GAMEPLAY_RESPAWN_TIMER");
			RespawnText.Text = string.Format(@string, num);
			_timeCached = num;
		}
	}

	private void Update()
	{
		if (_time > 0f)
		{
			_time -= Time.deltaTime;
			SetTimeText();
			if (_time <= 0f)
			{
				_time = 0f;
				RespawnText.Text = Strings.GetString("IDS_GAMEPLAY_RESPAWN_SPAWNING");
			}
		}
		if (_time == 0f && _done)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
