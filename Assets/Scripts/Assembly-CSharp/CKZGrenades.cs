using System.Collections;
using UnityEngine;

public class CKZGrenades : BaseAbilityPlacing
{
	private class Grenade
	{
		private CachedObject _grenadeCachedObj;

		private Transform _grenadeTransform;

		private float _gravityY = -9.8f;

		private Vector3 _speedVector = Vector3.zero;

		private Vector3 _rotSpeedVector = Vector3.zero;

		private Vector3 _rotVector = Vector3.zero;

		private float _restMovementTime;

		private bool _exploded;

		private HitEffect _hitEffect;

		public Grenade(CachedObject grenadeCachedObj, HitEffect hitEffect, float gravityY, Vector3 startPos, Vector3 landingPos, float landingTime)
		{
			_grenadeCachedObj = grenadeCachedObj;
			_grenadeTransform = grenadeCachedObj.transform;
			_grenadeTransform.position = startPos;
			_restMovementTime = landingTime;
			_hitEffect = hitEffect;
			_gravityY = gravityY;
			_speedVector = CalkDropSpeedVector(startPos, landingPos, gravityY, landingTime);
			float num = Random.Range(90f, 360f);
			_rotSpeedVector = new Vector3(num * Random.Range(0.5f, 1f), num * Random.Range(0.5f, 1f), num * Random.Range(0.5f, 1f));
		}

		public void Update(float dt)
		{
			if (!_exploded)
			{
				if (_restMovementTime <= dt)
				{
					dt = _restMovementTime;
					_restMovementTime = 0f;
				}
				else
				{
					_restMovementTime -= dt;
				}
				_rotVector += _rotSpeedVector * dt;
				_speedVector.y += _gravityY * dt;
				_grenadeTransform.position += _speedVector * dt;
				_grenadeTransform.rotation = Quaternion.Euler(_rotVector);
				if (_restMovementTime <= 0f)
				{
					Explode();
				}
			}
		}

		private Vector3 CalkDropSpeedVector(Vector3 startPos, Vector3 landingPos, float gravityY, float landingTime)
		{
			Vector2 vector = new Vector2(landingPos.x - startPos.x, landingPos.z - startPos.z);
			float magnitude = vector.magnitude;
			vector.Normalize();
			float num = magnitude / landingTime;
			float num2 = landingPos.y - startPos.y;
			float y = num2 / landingTime - gravityY * landingTime / 2f;
			return new Vector3(vector.x * num, y, vector.y * num);
		}

		private void Explode()
		{
			_grenadeCachedObj.Deactivate();
			if (_hitEffect != null)
			{
				_hitEffect.Activate(_grenadeTransform.position);
			}
			_exploded = true;
		}
	}

	public float HitRadius = 40f;

	public float explosionForce;

	public float DamageDelay = 1f;

	public Vector3 GrendesDropPosition = new Vector3(0f, 0f, 0f);

	public float GrenadesMinRadius = 3f;

	public float GrenadesMaxRadius = 30f;

	public GameObject GrenadeExplosionFX;

	public GameObject GrenadeModel;

	public int GrenadesCount = 8;

	public float GrenadesSpawnDiffer = 0.1f;

	private CachedObject.Cache _grenadeCache;

	private CachedObject.Cache _hitEffectCache;

	private float _gravityY = -9.8f;

	private Grenade[] _grenades;

	private void Start()
	{
		_gravityY = -30f;
		if ((bool)GrenadeModel)
		{
			_grenadeCache = ObjectCacheManager.Instance.GetCache(GrenadeModel);
		}
		if ((bool)GrenadeExplosionFX)
		{
			_hitEffectCache = ObjectCacheManager.Instance.GetCache(GrenadeExplosionFX);
		}
		_grenades = new Grenade[GrenadesCount];
	}

	[RPC]
	public void SetTeam(int teamID)
	{
		InternalSetTeam(teamID);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_grenades == null)
		{
			_grenades = new Grenade[GrenadesCount];
		}
		for (int i = 0; i < _grenades.Length; i++)
		{
			_grenades[i] = null;
		}
		StartCoroutine(SpawnGrenades());
		StartCoroutine(DelayedDamage());
	}

	private IEnumerator SpawnGrenades()
	{
		yield return new WaitForSeconds(0.1f);
		if (!GrenadeModel)
		{
			yield break;
		}
		float dist = 40f;
		Vector3 trace_start_pos = base.gameObject.transform.position + new Vector3(0f, dist / 2f, 0f);
		Vector3 start_pos = base.gameObject.transform.position + GrendesDropPosition;
		Vector3 dir = new Vector3(0f, -1f, 0f);
		int hitMask = 1 << LayerMask.NameToLayer("Default");
		for (int i = 0; i < GrenadesCount; i++)
		{
			Vector3 pos = trace_start_pos + Random.Range(GrenadesMinRadius, GrenadesMaxRadius) * (Quaternion.Euler(new Vector3(0f, (float)Random.Range(0, 36) * 10f, 0f)) * Vector3.forward);
			_grenades[i] = null;
			RaycastHit hit = default(RaycastHit);
			if (Physics.Raycast(pos, dir, out hit, dist, hitMask) && !hit.collider.isTrigger)
			{
				CachedObject grCO = _grenadeCache.Activate();
				HitEffect he = _hitEffectCache.RetainObject() as HitEffect;
				_grenades[i] = new Grenade(grCO, he, _gravityY, start_pos, hit.point, DamageDelay);
			}
			if (GrenadesSpawnDiffer > 0f)
			{
				yield return new WaitForSeconds(Random.Range(0f, GrenadesSpawnDiffer));
			}
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

	private void Update()
	{
		if (_grenades == null)
		{
			return;
		}
		Grenade[] grenades = _grenades;
		foreach (Grenade grenade in grenades)
		{
			if (grenade != null)
			{
				grenade.Update(Time.deltaTime);
			}
		}
	}
}
