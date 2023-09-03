using UnityEngine;

public class BuffAroundAbility : CooldownAbility
{
	public AudioClip StartSound;

	public float Radius;

	public BuffConf buffConf;

	public GameObject StartFX;

	public GameObject HitFX;

	private CachedObject.Cache _startCache;

	private CachedObject.Cache _hitCache;

	private int _damageLayers;

	protected override void Start()
	{
		base.Start();
		InitDamageLayers();
		_startCache = ObjectCacheManager.Instance.GetCache(StartFX);
		_hitCache = ObjectCacheManager.Instance.GetCache(HitFX);
	}

	protected void InitDamageLayers()
	{
		MainWeapon componentInChildren = _rootObject.GetComponentInChildren<MainWeapon>();
		if ((bool)componentInChildren)
		{
			_damageLayers = componentInChildren.damageLayers;
		}
	}

	protected override void OnAbilityStart()
	{
		base.OnAbilityStart();
		BuffEnemies();
		if (_photonView != null)
		{
			_photonView.RPC("BuffEnemies", PhotonTargets.Others);
		}
	}

	[RPC]
	protected void BuffEnemies()
	{
		Vector3 position = _rootObject.transform.position;
		if (_startCache != null)
		{
			_startCache.Activate(position, Quaternion.identity);
		}
		Collider[] array = Physics.OverlapSphere(position, Radius, _damageLayers);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (base.vehicle.mainOwnerCollider != collider)
			{
				BuffSystem component = collider.transform.root.GetComponent<BuffSystem>();
				if ((bool)component)
				{
					component.AddInstancedBuff(buffConf.CreateBuff(), this, false);
				}
			}
		}
	}
}
