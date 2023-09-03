using System.Collections;
using UnityEngine;

public class CollisionHealer : MonoBehaviour
{
	public int Stacks = 1;

	public float EffectScale = 1f;

	public bool AbsoluteValue = true;

	public float AddHP;

	public GameObject ActivateFX;

	public GameObject VictimFX;

	public float ActivateDamageLevel;

	protected PhotonView _photonView;

	private Destructible _destructible;

	private Vehicle _vehicle;

	private float _accumulatedDamage;

	private CachedObject.Cache _activateFXCache;

	private CachedObject.Cache _victimFXCache;

	private YieldInstruction _periodYI = new WaitForSeconds(1f);

	private void Start()
	{
		_vehicle = base.gameObject.GetComponent<Vehicle>();
		if ((bool)_vehicle)
		{
			_vehicle.collisionDamageDelegate += OnCollideEnemy;
			_destructible = _vehicle.destructible;
			_photonView = _vehicle.photonView;
		}
		if ((bool)ActivateFX)
		{
			_activateFXCache = ObjectCacheManager.Instance.GetCache(ActivateFX);
		}
		if ((bool)VictimFX)
		{
			_victimFXCache = ObjectCacheManager.Instance.GetCache(VictimFX);
		}
	}

	private void OnCollideEnemy(float damage, Vehicle vehicle)
	{
		_accumulatedDamage += damage;
		if (!(_accumulatedDamage >= ActivateDamageLevel))
		{
			return;
		}
		_accumulatedDamage = 0f;
		if ((bool)_destructible)
		{
			float num = ((!AbsoluteValue) ? (_destructible.GetMaxHP() * AddHP) : AddHP) * EffectScale * (float)Stacks;
			HealMe(num, vehicle.destructible.id);
			if ((bool)_photonView)
			{
				_photonView.RPC("HealMe", PhotonTargets.Others, num, vehicle.destructible.id);
			}
		}
	}

	private void OnEnable()
	{
		StartCoroutine(DamageReset());
	}

	//[RPC]
	private void HealMe(float addHP, int victimDestructibleId)
	{
		if ((bool)_destructible && _destructible.isMine)
		{
			_destructible.Heal(addHP);
		}
		Destructible destructible = Destructible.Find(victimDestructibleId);
		if ((bool)destructible && _victimFXCache != null)
		{
			_victimFXCache.Activate(destructible.transform.position);
		}
		if (_activateFXCache != null)
		{
			_activateFXCache.Activate(base.gameObject.transform.position);
		}
	}

	private IEnumerator DamageReset()
	{
		while (true)
		{
			yield return _periodYI;
			_accumulatedDamage = 0f;
		}
	}
}
