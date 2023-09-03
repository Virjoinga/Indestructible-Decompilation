using System;
using System.Collections;
using UnityEngine;

public class CRTeamGame : TeamGame
{
	private class StopableCoroutine : IEnumerator
	{
		private IEnumerator _coroutineEnumerator;

		private bool _stop;

		private bool IsStoped
		{
			get
			{
				return _stop;
			}
		}

		public object Current
		{
			get
			{
				return _coroutineEnumerator.Current;
			}
		}

		public StopableCoroutine(IEnumerator coroutineEnumerator)
		{
			_coroutineEnumerator = coroutineEnumerator;
		}

		public void Stop()
		{
			_stop = true;
		}

		public bool MoveNext()
		{
			return !_stop && _coroutineEnumerator.MoveNext();
		}

		public void Reset()
		{
			_coroutineEnumerator.Reset();
		}
	}

	private float WatchDogDelay = 1f;

	private float PickupTime = 1f;

	private float ChargeRespawnTime = 4f;

	private float ChargeUnloadingTime = 3f;

	private float ChargeDissolveTime = 8f;

	private int _chargeCourierKills;

	private int _deliveredCharges;

	private YieldInstruction _pickupTimeYI;

	private YieldInstruction _respawnTimeYI;

	private YieldInstruction _unloadTimeYI;

	private YieldInstruction _dissolveTimeYI;

	private YieldInstruction _pickupTime_WD_YI;

	private YieldInstruction _respawnTime_WD_YI;

	private YieldInstruction _unloadTime_WD_YI;

	private PlayerVehicle _chargeCurrier;

	private int _lastCurrierId = -1;

	private StopableCoroutine[] _playersChargePickupCoroutines;

	private PlayerVehicle _chargeDissolver;

	private StopableCoroutine _unloadingCoroutine;

	private StopableCoroutine _dissolvingCoroutine;

	private int _activeLoadingAreaIdx = -1;

	private CRLoadingArea[] _playersInLoadingAreas;

	public int chargeCourierKills
	{
		get
		{
			return _chargeCourierKills;
		}
	}

	public int collectedChargesCount
	{
		get
		{
			return _deliveredCharges;
		}
	}

	public event Action<MatchPlayer> chargeCapturedEvent;

	public event Action<MatchTeam> chargeDissolvedEvent;

	public event Action chargeSpawnedEvent;

	public event Action chargeDroppedEvent;

	private StopableCoroutine StartStopableCoroutine(IEnumerator coroutine)
	{
		StopableCoroutine stopableCoroutine = new StopableCoroutine(coroutine);
		StartCoroutine(stopableCoroutine);
		return stopableCoroutine;
	}

	protected override void StartGame()
	{
		base.StartGame();
		_playersChargePickupCoroutines = new StopableCoroutine[base.match.playerCount];
		_playersInLoadingAreas = new CRLoadingArea[base.match.playerCount];
		_pickupTimeYI = new WaitForSeconds(PickupTime);
		_respawnTimeYI = new WaitForSeconds(ChargeRespawnTime);
		_unloadTimeYI = new WaitForSeconds(ChargeUnloadingTime);
		_dissolveTimeYI = new WaitForSeconds(ChargeDissolveTime);
		_pickupTime_WD_YI = new WaitForSeconds(PickupTime + WatchDogDelay);
		_respawnTime_WD_YI = new WaitForSeconds(ChargeRespawnTime + WatchDogDelay);
		_unloadTime_WD_YI = new WaitForSeconds(ChargeUnloadingTime + WatchDogDelay);
	}

	public void PlayerStartLoadingCharge(PlayerVehicle playerVehicle)
	{
		Debug.Log("PlayerStartLoadingCharge " + playerVehicle);
		if (_activeLoadingAreaIdx == -1)
		{
			PickupCharge(playerVehicle);
		}
		else if (_playersChargePickupCoroutines != null && playerVehicle.player.teamID != _activeLoadingAreaIdx)
		{
			_playersChargePickupCoroutines[playerVehicle.player.id - 1] = StartStopableCoroutine(PickupCoroutine(playerVehicle));
		}
	}

	public void PlayerLeaveCharge(PlayerVehicle playerVehicle)
	{
		Debug.Log("PlayerLeaveCharge " + playerVehicle);
		StopChargePickup(playerVehicle.player.id);
	}

	private void StopChargePickup(int playerId)
	{
		Debug.Log("StopChargePickup " + playerId);
		if (playerId < 0 || playerId > _playersChargePickupCoroutines.Length)
		{
			Debug.LogWarning("Wrong playerId = " + playerId);
		}
		if (_playersChargePickupCoroutines != null && _playersChargePickupCoroutines[playerId - 1] != null)
		{
			_playersChargePickupCoroutines[playerId - 1].Stop();
			_playersChargePickupCoroutines[playerId - 1] = null;
		}
	}

	private void StopAllPickupCoroutines()
	{
		Debug.Log("StopAllPickupCoroutines");
		for (int i = 0; i < _playersChargePickupCoroutines.Length; i++)
		{
			if (_playersChargePickupCoroutines[i] != null)
			{
				_playersChargePickupCoroutines[i].Stop();
			}
			_playersChargePickupCoroutines[i] = null;
		}
	}

	private IEnumerator PickupCoroutine(PlayerVehicle playerVehicle)
	{
		Debug.Log("PickupCoroutine " + playerVehicle);
		yield return (!PhotonNetwork.isMasterClient) ? _pickupTime_WD_YI : _pickupTimeYI;
		PickupCharge(playerVehicle);
	}

	private void PickupCharge(PlayerVehicle playerVehicle)
	{
		StopAllPickupCoroutines();
		if (PhotonNetwork.isMasterClient)
		{
			ChargePickedUp(playerVehicle);
			base.photonView.RPC("ChargePickedUp", PhotonTargets.Others, playerVehicle.player.id);
			if (_playersInLoadingAreas[playerVehicle.player.id - 1] != null)
			{
				PlayerEnterLoadingArea(playerVehicle, _playersInLoadingAreas[playerVehicle.player.id - 1]);
			}
		}
	}

	[RPC]
	private void ChargePickedUp(int playerId)
	{
		Debug.Log("ChargePickedUp " + playerId);
		StopAllPickupCoroutines();
		MatchPlayer player = null;
		if (base.match.TryGetPlayer(playerId, out player))
		{
			ChargePickedUp((player as GamePlayer).vehicle as PlayerVehicle);
		}
		else
		{
			Debug.LogError("ChargePickedUp: wrong player id = " + playerId);
		}
	}

	private void ChargePickedUp(PlayerVehicle playerVehicle)
	{
		Debug.Log("ChargePickedUp " + playerVehicle);
		_chargeCurrier = playerVehicle;
		_lastCurrierId = _chargeCurrier.player.id;
		BreakDissolving();
		ChargeItem chargeItemInstance = CRGameSetup.Instance.ChargeItemInstance;
		chargeItemInstance.gameObject.SetActiveRecursively(true);
		chargeItemInstance.AttachToVehicle(playerVehicle);
		if (this.chargeCapturedEvent != null)
		{
			this.chargeCapturedEvent(_chargeCurrier.player);
		}
	}

	private void StartChargeRespawn()
	{
		Debug.Log("StartChargeRespawn");
		StartCoroutine(ChargeRespawnCoroutine());
	}

	private IEnumerator ChargeRespawnCoroutine()
	{
		Debug.Log("ChargeRespawnCoroutine");
		ChargeItem charge = CRGameSetup.Instance.ChargeItemInstance;
		charge.gameObject.SetActiveRecursively(false);
		yield return (!PhotonNetwork.isMasterClient) ? _respawnTime_WD_YI : _respawnTimeYI;
		if (PhotonNetwork.isMasterClient)
		{
			Vector3 pos = SelectSpawnPosition();
			charge.SetOnPos(pos);
			ulong posVal = ((ulong)(uint)((int)BitConverter32.ToUint(pos.x) & -256) >> 8) | ((ulong)(uint)((int)BitConverter32.ToUint(pos.y) & -65536) << 8) | ((ulong)(uint)((int)BitConverter32.ToUint(pos.z) & -256) << 32);
			base.photonView.RPC("ChargeRespawn", PhotonTargets.Others, (long)posVal);
		}
	}

	[RPC]
	private void ChargeRespawn(long posVal)
	{
		Debug.Log("ChargeRespawn " + posVal);
		Vector3 onPos = default(Vector3);
		onPos.x = BitConverter32.ToFloat((uint)(int)(posVal << 8) & 0xFFFFFF00u);
		onPos.y = BitConverter32.ToFloat((uint)(int)(posVal >> 8) & 0xFFFF0000u);
		onPos.z = BitConverter32.ToFloat((uint)(int)(posVal >> 32) & 0xFFFFFF00u);
		ChargeItem chargeItemInstance = CRGameSetup.Instance.ChargeItemInstance;
		chargeItemInstance.SetOnPos(onPos);
		if (this.chargeSpawnedEvent != null)
		{
			this.chargeSpawnedEvent();
		}
	}

	private void DropCharge()
	{
		Debug.Log("DropCharge");
		ChargeItem chargeItemInstance = CRGameSetup.Instance.ChargeItemInstance;
		if (chargeItemInstance == null)
		{
			Debug.LogWarning("DropCharge charge == null");
			return;
		}
		Vector3 pos = Vector3.zero;
		ulong num = 0uL;
		if (chargeItemInstance.CheckDropPosition(out pos))
		{
			num = ((ulong)(uint)((int)BitConverter32.ToUint(pos.x) & -256) >> 8) | ((ulong)(uint)((int)BitConverter32.ToUint(pos.y) & -65536) << 8) | ((ulong)(uint)((int)BitConverter32.ToUint(pos.z) & -256) << 32);
		}
		else
		{
			StartChargeRespawn();
		}
		ChargeDropped((long)num);
		if (PhotonNetwork.isMasterClient)
		{
			base.photonView.RPC("ChargeDropped", PhotonTargets.Others, (long)num);
		}
	}

	[RPC]
	private void ChargeDropped(long posVal)
	{
		Debug.Log("ChargeDropped " + posVal);
		ChargeItem chargeItemInstance = CRGameSetup.Instance.ChargeItemInstance;
		if (chargeItemInstance == null)
		{
			Debug.LogWarning("ChargeDropped charge == null");
			return;
		}
		ChargeStopUnloading();
		if (posVal == 0L)
		{
			chargeItemInstance.HideTillRespawn();
			return;
		}
		Vector3 onPos = default(Vector3);
		onPos.x = BitConverter32.ToFloat((uint)(int)(posVal << 8) & 0xFFFFFF00u);
		onPos.y = BitConverter32.ToFloat((uint)(int)(posVal >> 8) & 0xFFFF0000u);
		onPos.z = BitConverter32.ToFloat((uint)(int)(posVal >> 32) & 0xFFFFFF00u);
		chargeItemInstance.SetOnPos(onPos);
		if (this.chargeDroppedEvent != null)
		{
			this.chargeDroppedEvent();
		}
	}

	private void OnPlayerVehicleDestroy(PlayerVehicle playerVehicle)
	{
		if (playerVehicle != null)
		{
			Debug.Log(string.Concat("_chargeCurrier = ", _chargeCurrier, " iid = ", _chargeCurrier ? _chargeCurrier.GetInstanceID() : 0, " playerVehicle = ", playerVehicle, " iid = ", playerVehicle.GetInstanceID()));
			if (_chargeCurrier == playerVehicle)
			{
				DropCharge();
				_chargeCurrier = null;
			}
			if (playerVehicle.player != null)
			{
				PlayerLeaveLoadingArea(playerVehicle, null);
				StopChargePickup(playerVehicle.player.id);
			}
			else
			{
				Debug.LogWarning("playerVehicle.player == null " + playerVehicle);
			}
		}
	}

	public void PlayerEnterLoadingArea(PlayerVehicle player, CRLoadingArea loadingArea)
	{
		Debug.Log("PlayerEnterLoadingArea " + player);
		_playersInLoadingAreas[player.player.id - 1] = loadingArea;
		if (!(player == _chargeCurrier))
		{
			return;
		}
		_activeLoadingAreaIdx = -1;
		for (int i = 0; i < CRGameSetup.Instance.TeamLoadingAreas.Length; i++)
		{
			if (CRGameSetup.Instance.TeamLoadingAreas[i] == loadingArea)
			{
				_activeLoadingAreaIdx = i;
				break;
			}
		}
		if (_activeLoadingAreaIdx >= 0 && _chargeCurrier.player.teamID == _activeLoadingAreaIdx)
		{
			ChargeStartUnloading(_activeLoadingAreaIdx);
		}
	}

	public void PlayerLeaveLoadingArea(PlayerVehicle player, CRLoadingArea loadingArea)
	{
		Debug.Log("PlayerLeaveLoadingArea " + player);
		if (player == _chargeCurrier)
		{
			ChargeStopUnloading();
		}
		_playersInLoadingAreas[player.player.id - 1] = null;
	}

	private IEnumerator UnloadingChargeCoroutine()
	{
		yield return (!PhotonNetwork.isMasterClient) ? _unloadTime_WD_YI : _unloadTimeYI;
		if (PhotonNetwork.isMasterClient)
		{
			ChargeStartDissolving(_activeLoadingAreaIdx);
			base.photonView.RPC("ChargeStartDissolving", PhotonTargets.Others, _activeLoadingAreaIdx);
		}
	}

	private void ChargeStartUnloading(int loadingAreaIdx)
	{
		Debug.Log("ChargeStartUnloading " + loadingAreaIdx);
		if (loadingAreaIdx >= 0)
		{
			CRLoadingArea cRLoadingArea = CRGameSetup.Instance.TeamLoadingAreas[loadingAreaIdx];
			if (!(cRLoadingArea == null))
			{
				cRLoadingArea.OnChargeUnloadStart();
				_unloadingCoroutine = StartStopableCoroutine(UnloadingChargeCoroutine());
			}
		}
	}

	private void ChargeStopUnloading()
	{
		Debug.Log("ChargeStopUnloading");
		if (_activeLoadingAreaIdx >= 0)
		{
			CRLoadingArea cRLoadingArea = CRGameSetup.Instance.TeamLoadingAreas[_activeLoadingAreaIdx];
			_activeLoadingAreaIdx = -1;
			cRLoadingArea.OnChargeUnloadBreak();
			if (_unloadingCoroutine != null)
			{
				_unloadingCoroutine.Stop();
				_unloadingCoroutine = null;
			}
		}
	}

	[RPC]
	private void ChargeStartDissolving(int loadingAreaIdx)
	{
		Debug.Log("ChargeStartDissolving " + loadingAreaIdx + "chargeCurrier=" + _chargeCurrier);
		_activeLoadingAreaIdx = loadingAreaIdx;
		if (_activeLoadingAreaIdx >= 0)
		{
			_chargeDissolver = _chargeCurrier;
			_chargeCurrier = null;
			CRLoadingArea cRLoadingArea = CRGameSetup.Instance.TeamLoadingAreas[_activeLoadingAreaIdx];
			ChargeItem chargeItemInstance = CRGameSetup.Instance.ChargeItemInstance;
			_dissolvingCoroutine = StartStopableCoroutine(DissolvingCoroutine());
			cRLoadingArea.OnChargeStartDissolving(ChargeDissolveTime);
			chargeItemInstance.SetOnPos(cRLoadingArea.transform.position);
		}
	}

	private void BreakDissolving()
	{
		if (_activeLoadingAreaIdx >= 0)
		{
			if (_dissolvingCoroutine != null)
			{
				_dissolvingCoroutine.Stop();
				CRLoadingArea cRLoadingArea = CRGameSetup.Instance.TeamLoadingAreas[_activeLoadingAreaIdx];
				cRLoadingArea.OnChargeDissolvingBreak();
			}
			_dissolvingCoroutine = null;
		}
	}

	private IEnumerator DissolvingCoroutine()
	{
		yield return _dissolveTimeYI;
		ChargeDissolved();
	}

	private Vector3 SelectSpawnPosition()
	{
		Transform transform = CRGameSetup.Instance.SpawnPoints[UnityEngine.Random.Range(0, CRGameSetup.Instance.SpawnPoints.Length - 1)];
		return transform.position;
	}

	[RPC]
	protected override void PlayerKillEnemy(int playerID, int enemyID, int damageType)
	{
		Debug.Log(string.Format("PlayerKillEnemy({0}, {1}, {2})", playerID, enemyID, damageType));
		base.PlayerKillEnemy(playerID, enemyID, damageType);
		if (_lastCurrierId == enemyID)
		{
			if (playerID == base.match.localPlayer.id)
			{
				_chargeCourierKills++;
			}
			_lastCurrierId = -1;
		}
	}

	[RPC]
	protected override void PlayerDied(int playerID)
	{
		base.PlayerDied(playerID);
	}

	protected override void VehicleDeactivated(Vehicle vehicle)
	{
		base.VehicleDeactivated(vehicle);
		PlayerVehicle playerVehicle = vehicle as PlayerVehicle;
		OnPlayerVehicleDestroy(playerVehicle);
	}

	protected override void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		GamePlayer gamePlayer = player as GamePlayer;
		Vehicle vehicle = gamePlayer.vehicle;
		PlayerVehicle playerVehicle = vehicle as PlayerVehicle;
		OnPlayerVehicleDestroy(playerVehicle);
		base.PlayerDisconnected(match, player);
	}

	private void ChargeDissolved()
	{
		Debug.Log("ChargeDissolved " + _chargeDissolver);
		GamePlayer gamePlayer = _chargeDissolver.player as GamePlayer;
		GamePlayer.MultiplayerGameBase.AddScore(gamePlayer, 1);
		MatchTeam team = base.match.GetTeam(gamePlayer.teamID);
		GetInternalData(team).score++;
		if (base.match.localPlayer == gamePlayer)
		{
			_deliveredCharges++;
		}
		CRLoadingArea cRLoadingArea = CRGameSetup.Instance.TeamLoadingAreas[_activeLoadingAreaIdx];
		cRLoadingArea.OnChargeDissolved();
		StopAllPickupCoroutines();
		StartChargeRespawn();
		_dissolvingCoroutine = null;
		_activeLoadingAreaIdx = -1;
		_chargeDissolver = null;
		TeamScoreChanged(team);
		if (this.chargeDissolvedEvent != null)
		{
			this.chargeDissolvedEvent(team);
		}
	}
}
