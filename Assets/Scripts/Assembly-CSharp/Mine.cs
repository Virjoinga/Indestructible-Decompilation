using System.Collections;
using UnityEngine;

public class Mine : BaseAbilityPlacing
{
	public float ActivateRadius = 2f;

	public float DamageRadius = 4f;

	public float explosionForce;

	public float BurnDamage = 150f;

	public float BurnDuration = 5f;

	public GameObject ExplodeFX;

	public GameObject[] TeamAppearances;

	public float LiveTime = 5f;

	private CachedObject.Cache _appearanceCache;

	private CachedObject _appearanceInstance;

	private CachedObject.Cache[] _teamAppearanceCache;

	private CachedObject.Cache _explodeFXCache;

	private Transform _transform;

	private bool _checkCollision;

	private bool _exploded;

	private Renderer _renderer;

	protected override void Awake()
	{
		base.Awake();
		_explodeFXCache = ObjectCacheManager.Instance.GetCache(ExplodeFX);
		_teamAppearanceCache = new CachedObject.Cache[TeamAppearances.Length];
		for (int i = 0; i < TeamAppearances.Length; i++)
		{
			_teamAppearanceCache[i] = ObjectCacheManager.Instance.GetCache(TeamAppearances[i]);
		}
		_transform = base.transform;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_photonView == null || _photonView.isMine)
		{
			_checkCollision = true;
			StartCoroutine(DelayedDetonate());
		}
		if ((bool)base.rigidbody)
		{
			base.rigidbody.velocity = Vector3.zero;
			base.rigidbody.angularVelocity = Vector3.zero;
		}
		_exploded = false;
	}

	protected void FixedUpdate()
	{
		if (!_setuped || !_checkCollision || _exploded)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(_transform.position, ActivateRadius, base.damageLayers);
		if (array.Length <= 0)
		{
			return;
		}
		bool flag = false;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (IsFoe(collider))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Detonate();
			if (_exploded && (bool)_photonView)
			{
				_photonView.RPC("Detonate", PhotonTargets.Others);
			}
		}
	}

	private IEnumerator DelayedDetonate()
	{
		yield return new WaitForSeconds(LiveTime);
		Detonate();
		if ((bool)_photonView)
		{
			_photonView.RPC("Detonate", PhotonTargets.Others);
		}
	}

	[RPC]
	private void Detonate()
	{
		Vector3 position = base.gameObject.transform.position;
		Collider[] colliders = Physics.OverlapSphere(position, DamageRadius, base.damageLayers);
		DamageLocals(colliders);
		DestroyWithFX();
		_exploded = true;
	}

	public void DestroyWithFX()
	{
		bool isVisible = true;
		if (_appearanceInstance != null)
		{
			isVisible = _renderer.isVisible;
			_appearanceInstance.transform.parent = null;
			_appearanceInstance.gameObject.SetActiveRecursively(false);
			_appearanceCache.Deactivated(_appearanceInstance);
		}
		if (_explodeFXCache != null)
		{
			HitEffect.Activate(_explodeFXCache.RetainObject(), base.transform.localPosition, isVisible);
		}
		DeactivateMe();
	}

	private bool DamageLocals(Collider[] Colliders)
	{
		bool result = false;
		int i = 0;
		for (int num = Colliders.Length; i != num; i++)
		{
			Collider collider = Colliders[i];
			if (!IsFoe(collider))
			{
				continue;
			}
			result = true;
			GameObject gameObject = GetGameObject(collider);
			PhotonView componentInChildren = gameObject.GetComponentInChildren<PhotonView>();
			if (!(componentInChildren == null) && !componentInChildren.isMine)
			{
				continue;
			}
			if (GetDamage() > 0f)
			{
				Destructible component = gameObject.GetComponent<Destructible>();
				if (component != null)
				{
					component.Damage(GetDamage(), this);
				}
			}
			if (BurnDamage > 0f)
			{
				SetBurning(gameObject);
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
			_renderer = _appearanceInstance.GetComponentInChildren<Renderer>();
		}
	}

	private bool SetBurning(GameObject hitObject)
	{
		bool result = false;
		BuffSystem componentInChildren = hitObject.GetComponentInChildren<BuffSystem>();
		if ((bool)componentInChildren)
		{
			result = true;
			BurningBuff burningBuff = componentInChildren.AddBuff<BurningBuff>(this);
			burningBuff.Burn();
			burningBuff.duration = BurnDuration;
			burningBuff.effectScale = BurnDamage;
		}
		return result;
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
