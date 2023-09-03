using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiplayerGame : GamePlayer.MultiplayerGameBase
{
	protected enum FoeRatio
	{
		Equal = 0,
		Stronger = 1,
		Weaker = 2
	}

	private struct PlayerEnumerable : IEnumerable, IEnumerable<GamePlayer>
	{
		private IEnumerable<MatchPlayer> _matchPlayerEnumerable;

		public PlayerEnumerable(IEnumerable<MatchPlayer> matchPlayerEnumerable)
		{
			_matchPlayerEnumerable = matchPlayerEnumerable;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new PlayerEnumerator(_matchPlayerEnumerable.GetEnumerator());
		}

		public IEnumerator<GamePlayer> GetEnumerator()
		{
			return new PlayerEnumerator(_matchPlayerEnumerable.GetEnumerator());
		}
	}

	private struct PlayerEnumerator : IDisposable, IEnumerator, IEnumerator<GamePlayer>
	{
		private IEnumerator<MatchPlayer> _matchPlayerEnumerator;

		object IEnumerator.Current
		{
			get
			{
				return _matchPlayerEnumerator.Current;
			}
		}

		public GamePlayer Current
		{
			get
			{
				return _matchPlayerEnumerator.Current as GamePlayer;
			}
		}

		public PlayerEnumerator(IEnumerator<MatchPlayer> matchPlayerEnumerator)
		{
			_matchPlayerEnumerator = matchPlayerEnumerator;
		}

		public bool MoveNext()
		{
			return _matchPlayerEnumerator.MoveNext();
		}

		public void Reset()
		{
			_matchPlayerEnumerator.Reset();
		}

		public void Dispose()
		{
			_matchPlayerEnumerator.Dispose();
		}
	}

	public delegate void PlayerKillEnemyDelegate(GamePlayer player, GamePlayer enemy, DamageType damageType);

	public int killEnemyScore;

	public int diedScore;

	public float respawnDelay = 10f;

	public int victoryScore = 3;

	private MultiplayerMatch _match;

	private int _playerGeneralLayer;

	public MultiplayerMatch match
	{
		get
		{
			return _match;
		}
	}

	public GamePlayer localPlayer
	{
		get
		{
			return _match.localPlayer as GamePlayer;
		}
	}

	public IEnumerable<GamePlayer> players
	{
		get
		{
			return new PlayerEnumerable(_match.players);
		}
	}

	protected PhotonView photonView
	{
		get
		{
			return _match.photonView;
		}
	}

	protected int playerGeneralLayer
	{
		get
		{
			return _playerGeneralLayer;
		}
	}

	public event PlayerKillEnemyDelegate playerKillEnemyEvent;

	public bool TryGetPlayer(int id, out GamePlayer player)
	{
		MatchPlayer player2;
		bool result = _match.TryGetPlayer(id, out player2);
		player = player2 as GamePlayer;
		return result;
	}

	protected override void Awake()
	{
		base.Awake();
		_match = GetComponent<MultiplayerMatch>();
		_match.matchStartedEvent += MatchStarted;
		_playerGeneralLayer = LayerMask.NameToLayer("Player");
	}

	protected override void Clear()
	{
		Debug.Log("MultiplayerGame.Clear");
		base.Clear();
	}

	protected override void ClearEvents()
	{
		Debug.Log("MultiplayerGame.ClearEvents");
		base.ClearEvents();
		this.playerKillEnemyEvent = null;
	}

	protected override void OnDestroy()
	{
		Debug.Log("MultiplayerGame.OnDestroy");
		base.OnDestroy();
		if (_match != null)
		{
			match.matchStartedEvent -= MatchStarted;
			match.playerDisconnectedEvent -= PlayerDisconnected;
		}
	}

	private void MatchStarted(MultiplayerMatch match)
	{
		MatchStartedAnalyticsEvent();
		match.playerDisconnectedEvent += PlayerDisconnected;
		StartGame();
	}

	protected override void StartGame()
	{
		VehiclesManager instance = VehiclesManager.instance;
		instance.vehicleDeactivatedEvent += VehicleDeactivated;
		instance.vehicleActivatedEvent += VehicleActivated;
		foreach (Vehicle otherVehicle in instance.otherVehicles)
		{
			if (otherVehicle.isActive)
			{
				VehicleActivated(otherVehicle);
			}
		}
		base.StartGame();
		if (match.isOnline)
		{
			return;
		}
		SetupVehicle(instance.playerVehicle, localPlayer);
		foreach (GamePlayer player in players)
		{
			if (player != localPlayer)
			{
				OfflineGamePlayer offlineGamePlayer = player as OfflineGamePlayer;
				if (offlineGamePlayer != null && !string.IsNullOrEmpty(offlineGamePlayer.vehicleName))
				{
					SetupVehicle(instance.SpawnVehicle(offlineGamePlayer.vehicleName, SelectPlayerSpawnPoint(player, instance.spawnPoints)), player);
				}
			}
		}
	}

	protected virtual void VehicleActivated(Vehicle vehicle)
	{
		if (match.isOnline && vehicle.player == null && ((1 << vehicle.gameObject.layer) & (7 << _playerGeneralLayer)) != 0 && vehicle.photonView != null)
		{
			PhotonPlayer owner = vehicle.photonView.owner;
			GamePlayer player;
			if (owner != null && TryGetPlayer(owner.ID, out player))
			{
				SetupVehicle(vehicle, player);
			}
		}
	}

	protected virtual void VehicleDeactivated(Vehicle vehicle)
	{
		if (((1 << vehicle.gameObject.layer) & (7 << _playerGeneralLayer)) != 0 && vehicle.player != null && !vehicle.player.isDisconnected)
		{
			if (vehicle.player == localPlayer)
			{
				Died();
			}
			else if (!match.isOnline)
			{
				PlayerDied(vehicle.player as GamePlayer);
			}
			bool isMasterClient = PhotonNetwork.isMasterClient;
			if (!match.isOnline || isMasterClient || vehicle == VehiclesManager.instance.playerVehicle)
			{
				float num = ((match.isOnline && !isMasterClient) ? 2f : 0f);
				VehiclesManager.instance.RespawnVehicle(respawnDelay + num, vehicle, SelectSpawnPoint);
			}
		}
	}

	protected virtual void SetupVehicle(Vehicle vehicle, GamePlayer player)
	{
		GamePlayer.MultiplayerGameBase.SetVehicle(player, vehicle);
		vehicle.SetPlayer(player);
	}

	protected override Transform SelectSpawnPoint(Vehicle vehicle, Transform[] spawnPoints)
	{
		MatchPlayer player;
		if (vehicle != null)
		{
			player = vehicle.player;
			if (player == null)
			{
				player = match.localPlayer;
			}
		}
		else
		{
			player = match.localPlayer;
		}
		return SelectPlayerSpawnPoint(player, spawnPoints);
	}

	protected virtual Transform SelectPlayerSpawnPoint(MatchPlayer player, Transform[] spawnPoints)
	{
		return VehiclesManager.SelectSpawnPoint(null, spawnPoints);
	}

	protected virtual void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		GamePlayer gamePlayer = player as GamePlayer;
		Vehicle vehicle = gamePlayer.vehicle;
		if (vehicle != null)
		{
			vehicle.destructible.Die(DestructionReason.Disconnect);
		}
	}

	public override void Destructed(Destructible destructed, DestructionReason reason, INetworkWeapon weapon)
	{
		if (destructed.vehicle == null || reason == DestructionReason.Disconnect)
		{
			return;
		}
		GamePlayer gamePlayer = destructed.vehicle.player as GamePlayer;
		if (gamePlayer != null)
		{
			GamePlayer gamePlayer2;
			DamageType damageType;
			if (weapon == null)
			{
				gamePlayer2 = gamePlayer;
				damageType = DamageType.Generic;
			}
			else
			{
				gamePlayer2 = weapon.player as GamePlayer;
				damageType = weapon.damageType;
			}
			PlayerKillEnemy(gamePlayer2, gamePlayer, damageType);
			if (match.isOnline)
			{
				photonView.RPC("PlayerKillEnemy", PhotonTargets.Others, (gamePlayer2 == null) ? (-1) : gamePlayer2.id, gamePlayer.id, (int)damageType);
			}
		}
	}

	protected virtual void PlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (player != null)
		{
			if (player == enemy)
			{
				GamePlayer.MultiplayerGameBase.AddSelfKillCount(player, 1);
			}
			else
			{
				GamePlayer.MultiplayerGameBase.AddKillCount(player, 1);
			}
			if (Weapon.GetBaseDamageType(damageType) == DamageType.Collision)
			{
				GamePlayer.MultiplayerGameBase.AddCollisionKillCount(player, 1);
			}
		}
		if (this.playerKillEnemyEvent != null)
		{
			this.playerKillEnemyEvent(player, enemy, damageType);
		}
		AddScoreOnKill(player, enemy, damageType);
	}

	protected virtual void AddScoreOnKill(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (player != null && player != enemy)
		{
			GamePlayer.MultiplayerGameBase.AddScore(player, killEnemyScore);
		}
	}

	protected virtual void Died()
	{
		PlayerDied(localPlayer);
		if (match.isOnline)
		{
			photonView.RPC("PlayerDied", PhotonTargets.Others, _match.localPlayer.id);
		}
	}

	protected virtual void PlayerDied(GamePlayer player)
	{
		Debug.Log("PlayerDied " + player.id);
		GamePlayer.MultiplayerGameBase.AddScore(player, diedScore);
		GamePlayer.MultiplayerGameBase.AddDeathCount(player, 1);
	}

	protected virtual void PlayerKillEnemy(int playerID, int enemyID, int damageType)
	{
		GamePlayer player;
		if (TryGetPlayer(enemyID, out player))
		{
			GamePlayer player2;
			TryGetPlayer(playerID, out player2);
			PlayerKillEnemy(player2, player, (DamageType)damageType);
		}
	}

	protected virtual void PlayerDied(int playerID)
	{
		MatchPlayer player;
		if (_match.TryGetPlayer(playerID, out player))
		{
			PlayerDied(player as GamePlayer);
		}
	}

	protected override void CalculateReward(bool win, ref Reward reward)
	{
		bool flag = GetComponent<GCMatchmaker>() != null;
		if (base.IsBossFight)
		{
			SetBossFightReward(win, ref reward);
		}
		else if (flag)
		{
			reward.MoneySoft = 0;
			reward.ExperiencePoints = 0;
			reward.InfluencePoints = 0;
		}
		else
		{
			MonoSingleton<Player>.Instance.SetMultiplayerReward(win, ref reward);
			reward.InfluencePoints = CalculateBaseIP(win, MonoSingleton<Player>.Instance.League);
		}
		reward.Victory = win;
	}

	protected int CalculateBaseRewardXP(int curLvl, bool win)
	{
		int[] array = new int[20]
		{
			6, 9, 16, 22, 29, 35, 42, 48, 55, 61,
			68, 74, 81, 87, 94, 100, 107, 113, 120, 126
		};
		int[] array2 = new int[20]
		{
			6, 5, 8, 12, 15, 19, 22, 26, 29, 33,
			36, 40, 43, 47, 50, 54, 57, 61, 64, 68
		};
		int[] array3 = ((!win) ? array2 : array);
		return (curLvl > 0 && curLvl < array3.Length) ? array3[curLvl - 1] : 0;
	}

	protected int CalculateBaseRewardMoney(int curLvl, bool win, int place, FoeRatio ratio)
	{
		float[] array = new float[3] { 1f, 1.5f, 0.5f };
		float num = 1.1f - (float)place / 10f;
		float num2 = ((!win) ? 1f : 2f);
		float num3 = array[(int)ratio];
		float num4 = ((curLvl > 1) ? (Mathf.Log(curLvl, 2f) * num * num2 * num3 * 100f) : 50f);
		return (int)num4;
	}

	protected int CalculateBaseIP(bool win, int legue)
	{
		int[] array = new int[3] { 10, 10, 10 };
		int[] array2 = new int[3] { 3, -3, -15 };
		return (!win) ? array2[legue] : array[legue];
	}

	private void MatchStartedAnalyticsEvent()
	{
		if (match.isOnline)
		{
			GameAnalytics.EventMultiplayerStart();
		}
	}
}
