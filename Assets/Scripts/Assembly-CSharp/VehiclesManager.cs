using System.Collections;
using System.Collections.Generic;
using Glu;
using UnityEngine;

public class VehiclesManager : Glu.MonoBehaviour
{
	public delegate void VehicleActivatedDelegate(Vehicle vehicle);

	public delegate void VehicleDeactivatedDelegate(Vehicle vehicle);

	public delegate Transform SpawnDelegate(Vehicle vehicle, Transform[] spawnPoints);

	public const int NetGroupStartID = 2;

	public Transform[] spawnPoints;

	private Vehicle _playerVehicle;

	private List<Vehicle> _otherVehicles;

	private static VehiclesManager _instance;

	public static VehiclesManager instance
	{
		get
		{
			return _instance;
		}
	}

	public Vehicle playerVehicle
	{
		get
		{
			return _playerVehicle;
		}
	}

	public IEnumerable<Vehicle> otherVehicles
	{
		get
		{
			return _otherVehicles;
		}
	}

	public event VehicleActivatedDelegate vehicleActivatedEvent;

	public event VehicleActivatedDelegate playerVehicleActivatedEvent;

	public event VehicleDeactivatedDelegate vehicleDeactivatedEvent;

	public event VehicleDeactivatedDelegate playerVehicleDeactivatedEvent;

	public Vehicle SpawnVehicle(string prefabName, Transform spawnPoint)
	{
		return PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation, 2).GetComponent<Vehicle>();
	}

	public void SpawnVehicle(Vehicle vehicle, Transform spawnPoint)
	{
		if (!vehicle.isActive)
		{
			Transform transform = vehicle.transform;
			transform.localPosition = spawnPoint.position;
			transform.localRotation = spawnPoint.rotation;
			vehicle.destructible.Activate();
		}
	}

	public void RespawnVehicle(float delay, Vehicle vehicle, SpawnDelegate spawnDelegate)
	{
		StartCoroutine(VehicleRespawning(delay, vehicle, spawnDelegate));
	}

	public Vehicle SpawnLocalPlayerVehicle(Transform spawnPoint)
	{
		Debug.Log("VehiclesManager.SpawnLocalPlayerVehicle");
		if (_playerVehicle == null)
		{
			_playerVehicle = SpawnVehicle(MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.prefab, spawnPoint);
		}
		else
		{
			SpawnVehicle(_playerVehicle, spawnPoint);
		}
		return _playerVehicle;
	}

	public static Transform SelectSpawnPoint(Vehicle vehicle, Transform[] spawnPoints)
	{
		Transform transform = spawnPoints[0];
		int num = spawnPoints.Length;
		if (num != 1)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
			if (array.Length != 0)
			{
				Transform[] array2 = new Transform[array.Length];
				for (int i = 0; i != array.Length; i++)
				{
					array2[i] = array[i].transform;
				}
				float num2 = ScoreSpawnPoint(transform.position, array2);
				for (int j = 0; j < num; j++)
				{
					Transform transform2 = spawnPoints[j];
					float num3 = ScoreSpawnPoint(transform2.position, array2);
					if (num2 < num3)
					{
						num2 = num3;
						transform = transform2;
					}
				}
			}
		}
		return transform;
	}

	public void VehicleActivated(Vehicle vehicle, bool isFirstTime)
	{
		if (isFirstTime)
		{
			_otherVehicles.Add(vehicle);
		}
		if (this.vehicleActivatedEvent != null)
		{
			this.vehicleActivatedEvent(vehicle);
		}
		if (this.playerVehicleActivatedEvent != null && vehicle == _playerVehicle)
		{
			this.playerVehicleActivatedEvent(_playerVehicle);
		}
	}

	public void VehicleDeactivated(Vehicle vehicle)
	{
		if (this.playerVehicleDeactivatedEvent != null && vehicle == _playerVehicle)
		{
			this.playerVehicleDeactivatedEvent(vehicle);
		}
		if (this.vehicleDeactivatedEvent != null)
		{
			this.vehicleDeactivatedEvent(vehicle);
		}
	}

	private void Awake()
	{
		Debug.Log("VehiclesManager.Awake");
		if (_instance != null)
		{
			Debug.LogError("Vehicle manager instance already present while creating another instance");
			Debug.Break();
		}
		_instance = this;
		_otherVehicles = new List<Vehicle>();
	}

	private void Start()
	{
		Debug.Log("VehiclesManager.Start");
		if (IDTGame.Instance == null)
		{
			base.gameObject.AddComponent<SurvivalGame>();
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private IEnumerator VehicleRespawning(float delay, Vehicle vehicle, SpawnDelegate spawnDelegate)
	{
		yield return new WaitForSeconds(delay);
		SpawnVehicle(vehicle, spawnDelegate(vehicle, spawnPoints));
	}

	private static float ScoreSpawnPoint(Vector3 spawnPoint, Transform[] otherPlayersTransforms)
	{
		float num = (otherPlayersTransforms[0].localPosition - spawnPoint).sqrMagnitude;
		for (int i = 1; i != otherPlayersTransforms.Length; i++)
		{
			float sqrMagnitude = (otherPlayersTransforms[i].localPosition - spawnPoint).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
			}
		}
		return num;
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "PlayerManager.png", false);
	}
}
