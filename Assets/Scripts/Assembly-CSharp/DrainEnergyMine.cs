using System.Collections;
using UnityEngine;

public class DrainEnergyMine : BaseAbilityPlacing
{
	public float DrainRadius = 4f;

	public float EnergyDrainAmount = 40f;

	public float ActivateTime = 5f;

	public GameObject ExplodeFX;

	public GameObject[] TeamAppearances;

	private CachedObject.Cache _appearanceCache;

	private CachedObject _appearanceInstance;

	private CachedObject.Cache[] _teamAppearanceCache;

	private CachedObject.Cache _explodeFXCache;

	protected override void Awake()
	{
		base.Awake();
		_explodeFXCache = ObjectCacheManager.Instance.GetCache(ExplodeFX);
		_teamAppearanceCache = new CachedObject.Cache[TeamAppearances.Length];
		for (int i = 0; i < TeamAppearances.Length; i++)
		{
			_teamAppearanceCache[i] = ObjectCacheManager.Instance.GetCache(TeamAppearances[i]);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_photonView == null || _photonView.isMine)
		{
			StartCoroutine(DelayedDetonate());
		}
		if ((bool)base.rigidbody)
		{
			base.rigidbody.velocity = Vector3.zero;
			base.rigidbody.angularVelocity = Vector3.zero;
		}
	}

	private IEnumerator DelayedDetonate()
	{
		yield return new WaitForSeconds(ActivateTime);
		Detonate(true);
		if ((bool)_photonView)
		{
			_photonView.RPC("Detonate", PhotonTargets.Others, true);
		}
	}

	[RPC]
	private void Detonate(bool force)
	{
		Vector3 position = base.gameObject.transform.position;
		Collider[] array = Physics.OverlapSphere(position, DrainRadius, base.damageLayers);
		if (array.Length > 0)
		{
			if (!CheckDamage(array) && !force)
			{
				return;
			}
		}
		else if (!force)
		{
			return;
		}
		DestroyWithFX();
	}

	public void DestroyWithFX()
	{
		Vector3 position = base.gameObject.transform.position;
		if (ExplodeFX != null)
		{
			_explodeFXCache.Activate(position, Random.rotation);
		}
		if (_appearanceInstance != null)
		{
			_appearanceInstance.transform.parent = null;
			_appearanceInstance.gameObject.SetActiveRecursively(false);
			_appearanceCache.Deactivated(_appearanceInstance);
		}
		DeactivateMe();
	}

	private bool CheckDamage(Collider[] Colliders)
	{
		bool result = false;
		int i = 0;
		for (int num = Colliders.Length; i != num; i++)
		{
			Collider collider = Colliders[i];
			if (IsFoe(collider))
			{
				DrainFromLocal(GetGameObject(collider));
			}
		}
		return result;
	}

	protected override void OnSetTeam(int teamId)
	{
		SetAppearance(_teamAppearanceCache[teamId]);
	}

	private void SetAppearance(CachedObject.Cache appearCache)
	{
		if (appearCache != null)
		{
			_appearanceCache = appearCache;
			_appearanceInstance = appearCache.Activate();
			_appearanceInstance.transform.parent = base.gameObject.transform;
			_appearanceInstance.transform.localPosition = Vector3.zero;
			_appearanceInstance.transform.localRotation = Quaternion.identity;
		}
	}

	private bool DrainFromLocal(GameObject hitObject)
	{
		if (!hitObject)
		{
			return false;
		}
		PhotonView component = hitObject.GetComponent<PhotonView>();
		if (component == null || component.isMine)
		{
			Vehicle component2 = hitObject.GetComponent<Vehicle>();
			if (component2 != null)
			{
				component2.ConsumeEnergy(EnergyDrainAmount * _effectScale);
				return true;
			}
		}
		return false;
	}

	[RPC]
	public void SetTeam(int teamID)
	{
		InternalSetTeam(teamID);
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
