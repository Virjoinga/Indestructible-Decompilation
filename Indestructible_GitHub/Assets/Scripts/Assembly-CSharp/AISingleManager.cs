using System.Collections.Generic;
using UnityEngine;

public class AISingleManager : AIManager
{
	private int _playerLayer;

	protected override void Awake()
	{
		base.Awake();
		_playerLayer = LayerMask.NameToLayer("Player");
	}

	private void OnDestroy()
	{
		UnsubscribeFromVehiclesManager();
		UnSubscribeFromPlayer();
	}

	protected override void InitManager()
	{
		SubscribeToVehiclesManager();
		SubscribeToPlayer();
	}

	private void SubscribeToPlayer()
	{
		if (VehiclesManager.instance.playerVehicle != null)
		{
			SubscribeToGhostAbility();
		}
		else
		{
			VehiclesManager.instance.playerVehicleActivatedEvent += OnPlayerVehicleActivated;
		}
	}

	private void UnSubscribeFromPlayer()
	{
		if (VehiclesManager.instance != null && VehiclesManager.instance.playerVehicle != null)
		{
			UnSubscribeFromGhostAbility();
		}
	}

	private void OnPlayerVehicleActivated(Vehicle player)
	{
		VehiclesManager.instance.playerVehicleActivatedEvent -= OnPlayerVehicleActivated;
		SubscribeToGhostAbility();
	}

	private void SubscribeToGhostAbility()
	{
		GhostAbility component = VehiclesManager.instance.playerVehicle.GetComponent<GhostAbility>();
		if ((bool)component)
		{
			component.AbilityActivatedEvent += OnGhostAbilityActivated;
			component.ghostAbilityDeactivatedEvent += OnGhostAbilityDeactivated;
		}
	}

	private void UnSubscribeFromGhostAbility()
	{
		GhostAbility component = VehiclesManager.instance.playerVehicle.GetComponent<GhostAbility>();
		if ((bool)component)
		{
			component.AbilityActivatedEvent -= OnGhostAbilityActivated;
			component.ghostAbilityDeactivatedEvent -= OnGhostAbilityDeactivated;
		}
	}

	private void OnGhostAbilityActivated(BaseActiveAbility ability)
	{
		AllReturnToBase();
	}

	private void OnGhostAbilityDeactivated(GhostAbility ability)
	{
		AllChasePlayer(VehiclesManager.instance.playerVehicle.transform);
	}

	private void AllReturnToBase()
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[0].AICarsList;
		foreach (AIMoveAndFireController item in aICarsList)
		{
			if ((bool)item && item.gameObject.active)
			{
				item.SetFireTarget(null);
				item.SetMoveTarget(item.HomePoint);
				item.StartMovement();
			}
		}
	}

	private void AllChasePlayer(Transform playerTransform)
	{
		if (!PhotonNetwork.isMasterClient || playerTransform == null)
		{
			return;
		}
		EnableCheckReturnToBase(false);
		LinkedList<AIMoveAndFireController> aICarsList = _teamsInfo[0].AICarsList;
		foreach (AIMoveAndFireController item in aICarsList)
		{
			if ((bool)item && item.gameObject.active)
			{
				ChaseCar(item, playerTransform);
			}
		}
	}

	private void ChaseCar(AIMoveAndFireController chaser, Transform carTransform)
	{
		if ((bool)chaser)
		{
			chaser.SetFireTarget(carTransform);
			chaser.SetChaseTarget(carTransform);
			chaser.StartMovement();
		}
	}

	protected override void VehicleActivated(Vehicle vehicle)
	{
		base.VehicleActivated(vehicle);
		if (vehicle.gameObject.layer == _playerLayer)
		{
			AllChasePlayer(vehicle.transform);
			return;
		}
		VehiclesManager instance = VehiclesManager.instance;
		Vehicle playerVehicle = instance.playerVehicle;
		if (playerVehicle != null && playerVehicle.isActive)
		{
			ChaseCar(vehicle.GetComponent<AIMoveAndFireController>(), playerVehicle.transform);
		}
	}

	protected override void VehicleDeactivated(Vehicle vehicle)
	{
		base.VehicleDeactivated(vehicle);
		if (vehicle.gameObject.layer == _playerLayer)
		{
			AllReturnToBase();
		}
	}
}
