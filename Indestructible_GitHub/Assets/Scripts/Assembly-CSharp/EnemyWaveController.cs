using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Game Mode/Enemy Wave Controller")]
public class EnemyWaveController : Glu.MonoBehaviour, IVehicleDeactivationObserver
{
	private class UniqueEnemy
	{
		public string key;

		public bool mountBody;

		public string prefabName;

		public string bodyPrefabName;

		public float hp;

		public float damage;

		public int count;
	}

	private class Wave
	{
		public int rewardSC;

		public int rewardXP;

		public List<UniqueEnemy> uniqueEnemies;

		public int enemyCount;
	}

	public int maxCurrentEnemyCount = 4;

	public Transform[] spawnPoints;

	public string wavesConfigPath;

	private int _customWave = -1;

	private int _activeEnemyCount;

	private int _currentWaveIndex;

	private List<Wave> _waves;

	private Dictionary<string, Stack<Vehicle>> _inactiveVehicles;

	private bool _isSpawning;

	public void SetCustomWave(int idx)
	{
		_customWave = idx;
	}

	private void Start()
	{
		StartCoroutine(Starting());
	}

	private IEnumerator Starting()
	{
		yield return null;
		SurvivalGame game = IDTGame.Instance as SurvivalGame;
		_currentWaveIndex = ((_customWave > 0) ? _customWave : 0);
		if (game != null)
		{
			_inactiveVehicles = new Dictionary<string, Stack<Vehicle>>();
			LoadWaves();
			VehiclesManager vehiclesManager = VehiclesManager.instance;
			if (vehiclesManager.playerVehicle != null && vehiclesManager.playerVehicle.isActive)
			{
				PlayerVehicleActivated(vehiclesManager.playerVehicle);
			}
			else
			{
				vehiclesManager.playerVehicleActivatedEvent += PlayerVehicleActivated;
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void PlayerVehicleActivated(Vehicle vehicle)
	{
		if (_currentWaveIndex < _waves.Count && !_isSpawning)
		{
			StartCoroutine(SpawnEnemiesLoop());
		}
		VehiclesManager.instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
	}

	public void VehicleDeactivated(Vehicle vehicle)
	{
		_activeEnemyCount--;
		Stack<Vehicle> value;
		if ((!(vehicle.BodyMountPoint == null) && _inactiveVehicles.TryGetValue(vehicle.BodyMountPoint.name, out value)) || _inactiveVehicles.TryGetValue(vehicle.name, out value))
		{
			value.Push(vehicle);
			if (!_isSpawning)
			{
				StartCoroutine(SpawnEnemiesLoop());
			}
		}
	}

	private IEnumerator SpawnEnemiesLoop()
	{
		_isSpawning = true;
		yield return null;
		while (_activeEnemyCount < maxCurrentEnemyCount && (SpawnEnemy(_waves[_currentWaveIndex], SelectSpawnPoint()) || (0 >= _activeEnemyCount && WaveComplete())))
		{
			yield return null;
		}
		_isSpawning = false;
	}

	private bool SpawnEnemy(Wave wave, Transform spawnPoint)
	{
		if (0 < wave.enemyCount)
		{
			List<UniqueEnemy> uniqueEnemies = wave.uniqueEnemies;
			int num = uniqueEnemies.Count - 1;
			int num2 = Random.Range(0, num);
			int num3 = num2;
			do
			{
				if (0 < uniqueEnemies[num3].count)
				{
					SpawnEnemy(wave, uniqueEnemies[num3], spawnPoint);
					return true;
				}
			}
			while (++num3 <= num);
			for (num3 = 0; num3 < num2; num3++)
			{
				if (0 < uniqueEnemies[num3].count)
				{
					SpawnEnemy(wave, uniqueEnemies[num3], spawnPoint);
					return true;
				}
			}
		}
		return false;
	}

	private void SpawnEnemy(Wave wave, UniqueEnemy uniqueEnemy, Transform spawnPoint)
	{
		wave.enemyCount--;
		uniqueEnemy.count--;
		Stack<Vehicle> value;
		if (!_inactiveVehicles.TryGetValue(uniqueEnemy.key, out value))
		{
			value = new Stack<Vehicle>();
			_inactiveVehicles.Add(uniqueEnemy.key, value);
		}
		Vehicle vehicle;
		if (value.Count == 0)
		{
			GameObject gameObject = PhotonNetwork.Instantiate(uniqueEnemy.prefabName, spawnPoint.position, spawnPoint.rotation, 2);
			gameObject.name = uniqueEnemy.prefabName;
			vehicle = gameObject.GetComponent<Vehicle>();
			if (uniqueEnemy.mountBody)
			{
				vehicle.MountParts(uniqueEnemy.bodyPrefabName, null, null);
			}
			vehicle.AddDeactivationObserver(this);
		}
		else
		{
			vehicle = value.Pop();
			Transform transform = vehicle.transform;
			transform.localPosition = spawnPoint.position;
			transform.localRotation = spawnPoint.rotation;
			vehicle.destructible.Activate();
		}
		vehicle.destructible.SetMaxHP(uniqueEnemy.hp);
		vehicle.weapon.SetDamage(uniqueEnemy.damage);
		_activeEnemyCount++;
	}

	private bool WaveComplete()
	{
		int count = _waves.Count;
		if (_currentWaveIndex < count)
		{
			SurvivalGame survivalGame = IDTGame.Instance as SurvivalGame;
			if (survivalGame != null)
			{
				Wave wave = _waves[_currentWaveIndex];
				survivalGame.WaveComplete(_currentWaveIndex, wave.rewardSC, wave.rewardXP);
			}
			if (_customWave < 0 && ++_currentWaveIndex < count)
			{
				return true;
			}
		}
		return false;
	}

	private Transform SelectSpawnPoint()
	{
		return spawnPoints[Random.Range(0, spawnPoints.Length - 1)];
	}

	private void LoadWaves()
	{
		_waves = new List<Wave>();
		XmlTextReader xmlTextReader = new XmlTextReader(new MemoryStream((BundlesUtils.Load(wavesConfigPath) as TextAsset).bytes, false));
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name.ToLower() == "wave")
			{
				Wave wave = new Wave();
				_waves.Add(wave);
				LoadWave(wave, xmlTextReader);
			}
		}
	}

	private void LoadWave(Wave wave, XmlTextReader reader)
	{
		while (reader.MoveToNextAttribute())
		{
			if (reader.Name.ToLower() == "sc")
			{
				wave.rewardSC = int.Parse(reader.Value);
			}
			else if (reader.Name.ToLower() == "xp")
			{
				wave.rewardXP = int.Parse(reader.Value);
			}
		}
		wave.uniqueEnemies = new List<UniqueEnemy>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				if (reader.Name.ToLower() == "enemy")
				{
					UniqueEnemy uniqueEnemy = new UniqueEnemy();
					wave.uniqueEnemies.Add(uniqueEnemy);
					wave.enemyCount += LoadUniqueEnemy(uniqueEnemy, reader);
				}
			}
			else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.ToLower() == "wave")
			{
				break;
			}
		}
	}

	private int LoadUniqueEnemy(UniqueEnemy uniqueEnemy, XmlTextReader reader)
	{
		while (reader.MoveToNextAttribute())
		{
			if (reader.Name.ToLower() == "prefab")
			{
				uniqueEnemy.prefabName = reader.Value;
			}
			else if (reader.Name.ToLower() == "body")
			{
				uniqueEnemy.bodyPrefabName = reader.Value;
			}
			else if (reader.Name.ToLower() == "hp")
			{
				uniqueEnemy.hp = float.Parse(reader.Value);
			}
			else if (reader.Name.ToLower() == "damage")
			{
				uniqueEnemy.damage = float.Parse(reader.Value);
			}
			else if (reader.Name.ToLower() == "count")
			{
				uniqueEnemy.count = int.Parse(reader.Value);
			}
		}
		if (string.IsNullOrEmpty(uniqueEnemy.bodyPrefabName))
		{
			uniqueEnemy.mountBody = false;
			uniqueEnemy.key = uniqueEnemy.prefabName.Substring(uniqueEnemy.prefabName.LastIndexOf('/') + 1);
		}
		else
		{
			uniqueEnemy.mountBody = true;
			uniqueEnemy.key = uniqueEnemy.bodyPrefabName.Substring(uniqueEnemy.bodyPrefabName.LastIndexOf('/') + 1);
		}
		return uniqueEnemy.count;
	}
}
