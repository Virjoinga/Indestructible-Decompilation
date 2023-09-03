using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
	public GameObject ObjectPrefabToSpawn;

	public int SpawnCount = 1;

	public float SpawnMinPeriod = 10f;

	public float SpawnMaxPeriod = 100f;

	public bool FullRespawn;

	public float StartDelay;

	private int NetworkSpawnGroup;

	private GameObject _spawnedObject;

	private float _lastSpawnTimestamp;

	private bool _wasObjectDeadLastFrame;

	private float _spawnCurrentPeriod;

	private void Awake()
	{
		int a = base.gameObject.layer - LayerMask.NameToLayer("AI");
		NetworkSpawnGroup = Mathf.Max(a, 0);
		_lastSpawnTimestamp = Time.time + StartDelay;
	}

	private void Spawn()
	{
		if (FullRespawn || _spawnedObject == null)
		{
			int group = 2 + NetworkSpawnGroup;
			_spawnedObject = PhotonNetwork.Instantiate(ObjectPrefabToSpawn.name, base.transform.position, base.transform.rotation, group);
		}
		else
		{
			_spawnedObject.SetActiveRecursively(true);
			_spawnedObject.transform.position = base.transform.position;
			_spawnedObject.transform.rotation = base.transform.rotation;
		}
		SpawnCount--;
		_wasObjectDeadLastFrame = false;
	}

	private bool IsObjectDead()
	{
		return _spawnedObject == null || (!FullRespawn && _spawnedObject != null && !_spawnedObject.active);
	}

	private void Update()
	{
		float num = Time.time - _lastSpawnTimestamp;
		if (num < SpawnMinPeriod || SpawnCount == 0 || !PhotonNetwork.isMasterClient)
		{
			return;
		}
		if (IsObjectDead())
		{
			if (!_wasObjectDeadLastFrame)
			{
				_lastSpawnTimestamp = Time.time;
				_spawnCurrentPeriod = Random.Range(SpawnMinPeriod, SpawnMaxPeriod);
				_wasObjectDeadLastFrame = true;
			}
			if (Time.time - _lastSpawnTimestamp > _spawnCurrentPeriod)
			{
				Spawn();
			}
		}
		else
		{
			_wasObjectDeadLastFrame = false;
		}
	}
}
