using System.Collections;
using UnityEngine;

public class DamageReflector : MonoBehaviour
{
	public AudioClip StartSound;

	public AudioClip EndSound;

	public GameObject StartFX;

	public GameObject HitFX;

	public int Stacks = 1;

	public int ChancePercent = 1;

	public float ReactivateTime = 2f;

	private CachedObject.Cache _startCache;

	private CachedObject.Cache _hitCache;

	protected PhotonView _photonView;

	private bool _active = true;

	private Destructible _destructible;

	private YieldInstruction _delayInstruction;

	private void Start()
	{
		if (PhotonNetwork.room != null)
		{
			_photonView = base.gameObject.GetComponent<PhotonView>();
		}
		_destructible = base.gameObject.GetComponent<Destructible>();
		if ((bool)_destructible)
		{
			_destructible.damagedEvent += OnDamaged;
			_destructible.destructedEvent += OnDestructed;
		}
		_delayInstruction = new WaitForSeconds(ReactivateTime);
		if ((bool)StartFX)
		{
			_startCache = ObjectCacheManager.Instance.GetCache(StartFX);
		}
		if ((bool)HitFX)
		{
			_hitCache = ObjectCacheManager.Instance.GetCache(HitFX);
		}
		_active = !_photonView || _photonView.isMine;
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon iweapon)
	{
		if (!_active || ChancePercent <= 0 || Stacks <= 0)
		{
			return;
		}
		MainWeapon mainWeapon = iweapon as MainWeapon;
		if (mainWeapon == null)
		{
			return;
		}
		int num = 100 / (ChancePercent * Stacks);
		int num2 = Random.Range(0, num);
		if (num2 != num / 2)
		{
			return;
		}
		_destructible.Heal(damage);
		Destructible component = mainWeapon.transform.root.GetComponent<Destructible>();
		if (component != null)
		{
			component.Damage(damage, null);
			if (_hitCache != null)
			{
				CachedObject cachedObject = _hitCache.Activate();
				cachedObject.transform.parent = base.transform;
				cachedObject.transform.localPosition = Vector3.zero;
			}
		}
		if (_startCache != null)
		{
			CachedObject cachedObject2 = _startCache.Activate();
			cachedObject2.transform.parent = base.transform;
			cachedObject2.transform.localPosition = Vector3.zero;
		}
		if ((bool)component && (bool)_photonView)
		{
			_photonView.RPC("ReflectDmg", PhotonTargets.All, component.id, damage);
		}
		_active = false;
		StartCoroutine(DelayedReactivate());
	}

	//[RPC]
	private void ReflectDmg(int victimDestructibleId, float damage)
	{
		Destructible destructible = Destructible.Find(victimDestructibleId);
		if ((bool)destructible)
		{
			PhotonView component = destructible.GetComponent<PhotonView>();
			if (!component || !component.isMine)
			{
				destructible = null;
			}
		}
		ReflectDamageAndHeal(destructible, damage);
	}

	private void ReflectDamageAndHeal(Destructible victim, float damage)
	{
		_destructible.Heal(damage);
		if (_startCache != null)
		{
			_startCache.Activate(_destructible.transform.position, StartFX.transform.rotation);
		}
		if (victim != null)
		{
			victim.Damage(damage, null);
			if (_hitCache != null)
			{
				_hitCache.Activate(victim.transform.position, HitFX.transform.rotation);
			}
		}
	}

	public void OnDestructed(Destructible destructed)
	{
		_active = !_photonView || _photonView.isMine;
	}

	private IEnumerator DelayedReactivate()
	{
		yield return _delayInstruction;
		_active = true;
	}
}
