using System.Collections;
using UnityEngine;

public class BulletStorm : BaseAbilityPlacing
{
	public float ExplodeKick = 15f;

	public float Radius = 25f;

	public float DamageDelay = 0.1f;

	public GameObject StartFX;

	public GameObject HitFX;

	private CachedObject.Cache _startCache;

	private CachedObject.Cache _hitCache;

	protected override void Awake()
	{
		base.Awake();
		_startCache = ObjectCacheManager.Instance.GetCache(StartFX);
		_hitCache = ObjectCacheManager.Instance.GetCache(HitFX);
	}

	private void Start()
	{
		InternalSetupPhotonView();
	}

	[RPC]
	public void SetTeam(int teamID)
	{
		InternalSetTeam(teamID);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(DelayedDamage());
	}

	private IEnumerator DelayedDamage()
	{
		if (!(_photonView != null) || _photonView.isMine)
		{
			yield return new WaitForSeconds(DamageDelay);
			CauseDamage();
			if (_photonView != null)
			{
				_photonView.RPC("CauseDamage", PhotonTargets.Others);
			}
		}
	}

	[RPC]
	private void CauseDamage()
	{
		Vector3 position = base.gameObject.transform.position;
		_startCache.Activate(position, Quaternion.identity);
		Collider[] colliders = Physics.OverlapSphere(position, Radius, base.damageLayers);
		Destructible[] destructibles = null;
		int num = FilterColliders(colliders, out destructibles);
		for (int i = 0; i < num; i++)
		{
			Destructible destructible = destructibles[i];
			if ((base.damageLayers & (1 << destructible.gameObject.layer)) != 0)
			{
				destructible.Damage(GetDamage(), this);
				_hitCache.Activate(destructible.transform.position, HitFX.transform.rotation);
				ApplyKick(position, destructible);
			}
		}
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

	private void ApplyKick(Vector3 epicenter, Destructible hitDestructible)
	{
		Rigidbody component = hitDestructible.GetComponent<Rigidbody>();
		if (!(component == null))
		{
			Vector3 position = component.position;
			position.y += 2f;
			component.position = position;
			epicenter.y = position.y;
			component.AddForce(ExplodeKick * component.mass * (position - epicenter).normalized, ForceMode.Impulse);
		}
	}
}
