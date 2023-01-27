using UnityEngine;

public class Explosive : Weapon
{
	public bool shouldExplodeOnCollision;

	public bool shouldExplodeOnTrigger;

	public float explosionRadius = 50f;

	public float explosionForce;

	public float constDamageFactor;

	public float sqrDamageFactor = 1f;

	public GameObject explosionPrefab;

	public float shakeCameraMagnitude = 2f;

	public float shakeCameraDuration = 1f;

	private PhotonView _photonView;

	private Transform _transform;

	private CachedObject.Cache _explosionEffectCache;

	private Renderer _renderer;

	private bool _isMine;

	private bool _exploded;

	public new Renderer renderer
	{
		get
		{
			return _renderer;
		}
		set
		{
			_renderer = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_transform = base.transform;
		if (PhotonNetwork.room != null)
		{
			_photonView = GetComponent<PhotonView>();
		}
		Destructible component = GetComponent<Destructible>();
		if (component != null)
		{
			component.destructedEvent += Destructed;
		}
	}

	protected virtual void Start()
	{
		Collider[] componentsInChildren = _transform.root.gameObject.GetComponentsInChildren<Collider>();
		int i = 0;
		for (int num = componentsInChildren.Length; i != num; i++)
		{
			Collider collider = componentsInChildren[i];
			if (!collider.isTrigger)
			{
				SetMainOwnerCollider(collider);
				break;
			}
		}
		UpdateOwnership();
		renderer = GetComponentInChildren<Renderer>();
		if (explosionPrefab != null)
		{
			_explosionEffectCache = ObjectCacheManager.Instance.GetCache(explosionPrefab);
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Collide");
		if (shouldExplodeOnCollision && _isMine)
		{
			Explode();
		}
	}

	protected virtual void OnTriggerEnter(Collider trigger)
	{
		if (shouldExplodeOnTrigger && _isMine)
		{
			Explode();
		}
	}

	protected void Explode()
	{
		if (_photonView != null)
		{
			_photonView.RPC("Detonate", PhotonTargets.Others);
		}
		Detonate();
	}

	//[RPC]
	protected virtual void Detonate()
	{
		if (!_exploded)
		{
			_exploded = true;
			VisualDetonation();
			Damage(_transform.position, explosionRadius, constDamageFactor, sqrDamageFactor, explosionForce);
			base.gameObject.SetActiveRecursively(false);
		}
	}

	protected virtual void VisualDetonation()
	{
		Vector3 position = _transform.position;
		HitEffect.Activate(_explosionEffectCache.RetainObject(), position, renderer.isVisible);
		CamShaker.ShakeIfExist(shakeCameraMagnitude, shakeCameraDuration, position);
	}

	protected void UpdateOwnership()
	{
		_isMine = _photonView == null || _photonView.isMine;
	}

	protected virtual void OnMasterClientSwitched(PhotonPlayer player)
	{
		UpdateOwnership();
	}

	private void OnEnable()
	{
		UpdateOwnership();
	}

	private void Destructed(Destructible destructed)
	{
		Explode();
	}
}
