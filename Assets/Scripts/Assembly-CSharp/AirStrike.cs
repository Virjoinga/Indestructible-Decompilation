using System.Collections;
using UnityEngine;

public class AirStrike : BaseAbilityPlacing
{
	public float HitRadius = 40f;

	public float explosionForce;

	public float DamageDelay = 1f;

	public float RocketsMinRadius = 3f;

	public float RocketsMaxRadius = 30f;

	public GameObject RocketFX;

	public int RocketsCount = 8;

	public float RocketsSpawnDiffer = 0.1f;

	private int _spawnedRockets;

	private CachedObject.Cache _rocketCache;

	private void Start()
	{
		if ((bool)RocketFX)
		{
			_rocketCache = ObjectCacheManager.Instance.GetCache(RocketFX);
		}
	}

	[RPC]
	public void SetTeam(int teamID)
	{
		InternalSetTeam(teamID);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_spawnedRockets = 0;
		StartCoroutine(SpawnRockets());
		StartCoroutine(DelayedDamage());
	}

	private IEnumerator SpawnRockets()
	{
		yield return new WaitForSeconds(0.1f);
		float dist = 40f;
		Vector3 start_pos = base.gameObject.transform.position + new Vector3(0f, dist / 2f, 0f);
		Vector3 dir = new Vector3(0f, -1f, 0f);
		int hitMask = 1 << LayerMask.NameToLayer("Default");
		while (_spawnedRockets < RocketsCount && (bool)RocketFX)
		{
			_spawnedRockets++;
			Vector3 pos = start_pos + Random.Range(RocketsMinRadius, RocketsMaxRadius) * (Quaternion.Euler(new Vector3(0f, (float)Random.Range(0, 36) * 10f, 0f)) * Vector3.forward);
			RaycastHit hit = default(RaycastHit);
			if (Physics.Raycast(pos, dir, out hit, dist, hitMask) && !hit.collider.isTrigger)
			{
				_rocketCache.Activate(hit.point, RocketFX.transform.rotation);
			}
			yield return new WaitForSeconds(RocketsSpawnDiffer);
		}
	}

	private IEnumerator DelayedDamage()
	{
		if (PhotonNetwork.isMasterClient)
		{
			yield return new WaitForSeconds(DamageDelay);
			CauseDamage();
			if ((bool)_photonView)
			{
				_photonView.RPC("CauseDamage", PhotonTargets.Others);
			}
		}
	}

	[RPC]
	private void CauseDamage()
	{
		Damage(base.gameObject.transform.position, HitRadius, 1f, 0f, explosionForce);
		DeactivateMe();
	}

	[RPC]
	private void SetOwner(int destructibleId, int actorID)
	{
		InternalSetOwner(destructibleId, actorID);
	}

	[RPC]
	private void SetEffectScale(float scale)
	{
		InternalSetEffectScale(scale);
	}
}
