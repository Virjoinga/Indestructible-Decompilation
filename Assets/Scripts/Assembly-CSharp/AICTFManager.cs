using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICTFManager : AIManager
{
	private class TeamAIData
	{
		public MatchTeam Team;

		public AIMoveAndFireController FlagCarrier;

		public FlagItem Flag;

		public FlagItem EnemyFlag;

		public bool EnemyFlagCaptured;
	}

	private const int TeamCount = 2;

	private CTFGame _game;

	private TeamAIData[] _aiTeamsInfo = new TeamAIData[2];

	private bool _activated;

	protected override void Awake()
	{
		base.Awake();
		_game = IDTGame.Instance as CTFGame;
		if (_game == null || _game.match.isOnline)
		{
			base.enabled = false;
			Object.Destroy(this);
		}
	}

	private void SubscribeToCTFGame()
	{
		if (_game != null)
		{
			_game.flagCapturedEvent += OnActorTakeFlag;
			_game.flagDeliveredEvent += OnActorDeliverFlag;
			_game.courierKilledEvent += OnActorKillCourier;
			_game.flagReturnedEvent += OnActorReturnFlag;
			FlagItem[] array = (FlagItem[])Object.FindObjectsOfType(typeof(FlagItem));
			FlagItem[] array2 = new FlagItem[2];
			for (int i = 0; i < array.Length; i++)
			{
				array2[array[i].TeamID] = array[i];
			}
			for (int j = 0; j < 2; j++)
			{
				_aiTeamsInfo[j] = new TeamAIData();
				_aiTeamsInfo[j].Team = _game.match.GetTeam(j);
				_aiTeamsInfo[j].Flag = array2[j];
				_aiTeamsInfo[j].EnemyFlag = array2[OppositeTeamId(j)];
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		UnsubscribeFromVehiclesManager();
		if (_game != null)
		{
			_game.flagCapturedEvent -= OnActorTakeFlag;
			_game.flagDeliveredEvent -= OnActorDeliverFlag;
			_game.courierKilledEvent -= OnActorKillCourier;
			_game.flagReturnedEvent -= OnActorReturnFlag;
		}
	}

	protected override void InitManager()
	{
		base.InitManager();
		SubscribeToCTFGame();
	}

	protected override void StartManager()
	{
		EnableCheckReturnToBase(false);
		EnableProtectionPoints(false);
		StartCoroutine(DelayedActivate());
	}

	private IEnumerator DelayedActivate()
	{
		yield return new WaitForSeconds(5f);
		_activated = true;
		StartCoroutine(SendActorCaptureFlagCoroutine(0));
		StartCoroutine(SendActorCaptureFlagCoroutine(1));
	}

	private void OnActorDeliverFlag(GamePlayer player)
	{
		if (_activated)
		{
			if (player == _game.localPlayer)
			{
				OnPlayerDeliverFlag();
				return;
			}
			int teamID = player.teamID;
			int teamID2 = OppositeTeamId(teamID);
			_aiTeamsInfo[teamID].EnemyFlagCaptured = false;
			SendActorCaptureFlag(teamID);
			SendActorCaptureFlag(teamID2);
		}
	}

	private void OnActorReturnFlag(GamePlayer player)
	{
		if (!_activated)
		{
			return;
		}
		if (player == _game.localPlayer)
		{
			OnPlayerReturnFlag();
			return;
		}
		int teamID = player.teamID;
		int teamID2 = OppositeTeamId(teamID);
		if (_aiTeamsInfo[teamID].EnemyFlagCaptured)
		{
			SendFlagCarrierHome(teamID);
		}
		SendActorCaptureFlag(teamID2);
	}

	private void OnActorKillCourier(GamePlayer player)
	{
		if (!_activated)
		{
			return;
		}
		if (player == _game.localPlayer)
		{
			OnPlayerKillCourier();
			return;
		}
		int teamID = player.teamID;
		int num = OppositeTeamId(teamID);
		_aiTeamsInfo[num].FlagCarrier = null;
		_aiTeamsInfo[num].EnemyFlagCaptured = false;
		ChaseStealedFlag(teamID);
		if (_aiTeamsInfo[teamID].EnemyFlagCaptured)
		{
			SendFlagCarrierHome(teamID);
		}
		SendActorCaptureFlag(num);
	}

	private void OnActorTakeFlag(GamePlayer player)
	{
		if (!_activated)
		{
			return;
		}
		if (player == _game.localPlayer)
		{
			OnPlayerTakeFlag();
			return;
		}
		int teamID = player.teamID;
		int num = OppositeTeamId(teamID);
		Vehicle vehicle = player.vehicle;
		_aiTeamsInfo[teamID].FlagCarrier = vehicle.GetComponent<AIMoveAndFireController>();
		_aiTeamsInfo[teamID].EnemyFlagCaptured = true;
		if ((_aiTeamsInfo[num].FlagCarrier == null || !_aiTeamsInfo[num].FlagCarrier.gameObject.active || _aiTeamsInfo[teamID].Flag.IsOnBase || _game.localPlayer.teamID == teamID) && TeamGame.GetData(_game.match.GetTeam(teamID)).score < _game.victoryScore - 1)
		{
			SendFlagCarrierHome(teamID);
		}
		else if (_aiTeamsInfo[num].FlagCarrier != null)
		{
			AllChaseActor(_aiTeamsInfo[num].FlagCarrier.AIVehicle.player as GamePlayer);
		}
		if (_aiTeamsInfo[num].EnemyFlagCaptured)
		{
			AllChaseActor(player);
		}
		else
		{
			ChaseStealedFlag(num);
		}
	}

	private void OnPlayerDeliverFlag()
	{
		if (_activated)
		{
			SendActorCaptureFlag(_game.localPlayer.teamID);
			SendActorCaptureFlag(OppositeTeamId(_game.localPlayer.teamID));
		}
	}

	private void OnPlayerReturnFlag()
	{
		if (_activated)
		{
			int teamID = _game.localPlayer.teamID;
			if (_aiTeamsInfo[teamID].EnemyFlagCaptured)
			{
				SendFlagCarrierHome(teamID);
			}
			else
			{
				SendActorCaptureFlag(teamID);
			}
			SendActorCaptureFlag(OppositeTeamId(teamID));
		}
	}

	private void OnPlayerKillCourier()
	{
		if (_activated)
		{
			GamePlayer localPlayer = _game.localPlayer;
			_aiTeamsInfo[OppositeTeamId(localPlayer.teamID)].EnemyFlagCaptured = false;
			SendActorCaptureFlag(localPlayer.teamID);
		}
	}

	private void OnPlayerTakeFlag()
	{
		if (_activated)
		{
			GamePlayer localPlayer = _game.localPlayer;
			int teamID = localPlayer.teamID;
			int num = OppositeTeamId(teamID);
			_aiTeamsInfo[teamID].FlagCarrier = localPlayer.vehicle.GetComponent<AIMoveAndFireController>();
			_aiTeamsInfo[teamID].EnemyFlagCaptured = true;
			if (_aiTeamsInfo[num].EnemyFlagCaptured || TeamGame.GetData(_game.match.GetTeam(num)).score >= _game.victoryScore - 1)
			{
				AllChaseActor(localPlayer);
			}
			else
			{
				ChaseStealedFlag(num);
			}
		}
	}

	protected override void VehicleActivated(Vehicle vehicle)
	{
		Debug.Log("AICTFManager.VehicleActivated");
		base.VehicleActivated(vehicle);
		if (!_activated)
		{
			StartCoroutine(DelayedVehicleActivatedHandler(vehicle));
		}
		else
		{
			VehicleActivationHandler(vehicle);
		}
	}

	private void VehicleActivationHandler(Vehicle vehicle)
	{
		Debug.Log("AICTFManager.VehicleActivationHandler 0");
		int teamID = vehicle.player.teamID;
		int num = OppositeTeamId(teamID);
		if (_aiTeamsInfo != null && _aiTeamsInfo[teamID] != null)
		{
			Debug.Log("AICTFManager.VehicleActivationHandler 1");
			if (teamID != _game.localPlayer.teamID)
			{
				EnemyChasePlayerCompanion();
			}
			if (_aiTeamsInfo[teamID].EnemyFlagCaptured)
			{
				SendFlagCarrierHome(teamID);
			}
			else
			{
				SendActorCaptureFlag(teamID);
			}
			ChaseStealedFlag(teamID);
		}
	}

	private IEnumerator DelayedVehicleActivatedHandler(Vehicle vehicle)
	{
		do
		{
			yield return null;
		}
		while (!_activated);
		VehicleActivationHandler(vehicle);
	}

	protected override void VehicleDeactivated(Vehicle vehicle)
	{
		base.VehicleDeactivated(vehicle);
		if (!_activated)
		{
			return;
		}
		int teamID = vehicle.player.teamID;
		int teamID2 = OppositeTeamId(teamID);
		TeamAIData teamAIData = _aiTeamsInfo[teamID];
		if (teamAIData.FlagCarrier != null && teamAIData.FlagCarrier.AIVehicle == vehicle)
		{
			if (teamAIData.EnemyFlagCaptured)
			{
				SendFlagCarrierHome(teamID2);
				ChaseStealedFlag(teamID2);
			}
			teamAIData.EnemyFlagCaptured = false;
			teamAIData.FlagCarrier = null;
		}
		if (teamAIData.EnemyFlagCaptured)
		{
			SendFlagCarrierHome(teamID);
		}
		else
		{
			SendActorCaptureFlag(teamID);
		}
	}

	private int OppositeTeamId(int teamID)
	{
		return 1 - teamID;
	}

	private void EnemyChasePlayerCompanion()
	{
		Debug.Log("AICTFManager.EnemyChasePlayerCompanion");
		int teamID = _game.localPlayer.teamID;
		int num = OppositeTeamId(teamID);
		LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[teamID + 1].AICarsList;
		AIMoveAndFireController aIMoveAndFireController = null;
		foreach (AIMoveAndFireController item in aICarsList)
		{
			if ((bool)item && item.gameObject.active && item.AIVehicle.player != _game.localPlayer)
			{
				aIMoveAndFireController = item;
				break;
			}
		}
		if (aIMoveAndFireController != null)
		{
			AllChaseActor(aIMoveAndFireController.AIVehicle.player as GamePlayer);
		}
	}

	private void AllChaseActor(GamePlayer player)
	{
		if (player == null)
		{
			return;
		}
		Vehicle vehicle = player.vehicle;
		if (vehicle == null || !vehicle.isActive)
		{
			return;
		}
		EnableCheckReturnToBase(false);
		EnableProtectionPoints(false);
		int num = OppositeTeamId(player.teamID);
		int num2 = num + 1;
		if (num2 >= 3)
		{
			return;
		}
		LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[num2].AICarsList;
		foreach (AIMoveAndFireController item in aICarsList)
		{
			if ((bool)item && item.gameObject.active)
			{
				item.SetFireTarget(vehicle.transform);
				item.SetChaseTarget(vehicle.transform);
				item.StartMovement();
			}
		}
	}

	private IEnumerator SendActorCaptureFlagCoroutine(int teamID)
	{
		YieldInstruction yi = new WaitForSeconds(0.1f);
		while (!SendActorCaptureFlag(teamID))
		{
			yield return yi;
		}
	}

	private bool SendActorCaptureFlag(int teamID)
	{
		Debug.Log("SendActorCaptureFlag " + teamID);
		if (_aiTeamsInfo == null || _aiTeamsInfo[teamID] == null || !_aiTeamsInfo[teamID].EnemyFlag)
		{
			return false;
		}
		if (TeamGame.GetData(_game.match.GetTeam(teamID)).score >= _game.victoryScore - 1)
		{
			return true;
		}
		if (_aiTeamsInfo[teamID].EnemyFlagCaptured)
		{
			return true;
		}
		Vector3 position = _aiTeamsInfo[teamID].EnemyFlag.transform.position;
		AIMoveAndFireController aIMoveAndFireController = _aiTeamsInfo[teamID].FlagCarrier;
		if (!aIMoveAndFireController || aIMoveAndFireController.gameObject.active)
		{
			aIMoveAndFireController = null;
			LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[teamID + 1].AICarsList;
			float num = 999999f;
			foreach (AIMoveAndFireController item in aICarsList)
			{
				float sqrMagnitude = (position - item.gameObject.transform.position).sqrMagnitude;
				if ((bool)item && item.gameObject.active && num > sqrMagnitude)
				{
					num = sqrMagnitude;
					aIMoveAndFireController = item;
					break;
				}
			}
			if (!aIMoveAndFireController)
			{
				return false;
			}
		}
		aIMoveAndFireController.SetChaseTarget(_aiTeamsInfo[teamID].EnemyFlag.transform);
		aIMoveAndFireController.StartMovement();
		_aiTeamsInfo[teamID].FlagCarrier = aIMoveAndFireController;
		return true;
	}

	private bool SendFlagCarrierHome(int teamID)
	{
		if (_aiTeamsInfo == null || _aiTeamsInfo[teamID] == null || !_aiTeamsInfo[teamID].Flag)
		{
			return false;
		}
		if (TeamGame.GetData(_game.match.GetTeam(teamID)).score >= _game.victoryScore - 1)
		{
			return true;
		}
		AIMoveAndFireController flagCarrier = _aiTeamsInfo[teamID].FlagCarrier;
		if (!flagCarrier || !flagCarrier.gameObject.active)
		{
			return false;
		}
		flagCarrier.SetChaseTarget(_aiTeamsInfo[teamID].Flag.BaseObject);
		flagCarrier.StartMovement();
		return true;
	}

	private bool ChaseStealedFlag(int teamID)
	{
		if (_aiTeamsInfo == null || _aiTeamsInfo[teamID] == null || _aiTeamsInfo[OppositeTeamId(teamID)] == null || !_aiTeamsInfo[OppositeTeamId(teamID)].FlagCarrier)
		{
			return false;
		}
		if (_aiTeamsInfo[teamID].Flag.IsOnBase)
		{
			return false;
		}
		Transform chaseTarget = _aiTeamsInfo[teamID].Flag.transform;
		AIMoveAndFireController flagCarrier = _aiTeamsInfo[teamID].FlagCarrier;
		AIMoveAndFireController flagCarrier2 = _aiTeamsInfo[OppositeTeamId(teamID)].FlagCarrier;
		Transform fireTarget = ((!(flagCarrier2 != null)) ? null : flagCarrier2.gameObject.transform);
		AIMoveAndFireController aIMoveAndFireController = null;
		LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[teamID + 1].AICarsList;
		foreach (AIMoveAndFireController item in aICarsList)
		{
			if (item != flagCarrier && (bool)item && item.gameObject.active)
			{
				aIMoveAndFireController = item;
				item.SetChaseTarget(chaseTarget);
				item.SetFireTarget(fireTarget);
				item.StartMovement();
			}
		}
		return aIMoveAndFireController != null;
	}
}
