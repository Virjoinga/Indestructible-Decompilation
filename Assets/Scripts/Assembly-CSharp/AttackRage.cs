using System.Collections;
using UnityEngine;

public class AttackRage : MonoBehaviour
{
	public float DamageBoostPerSec;

	public float DamageForActivate = 3f;

	public float DamageForDeactivate = 3f;

	public int Stacks;

	public float EffectScale = 1f;

	public GameObject OnActiveFX;

	private Destructible _destructible;

	private MainWeapon _mainWeapon;

	private BuffSystem _buffSystem;

	private PhotonView _photonView;

	private AbilitiesFXDispatcher _fx;

	private float _collectedDamagePerSecForActivate;

	private float _collectedDamageForDeactivate;

	private float _rageCoeff;

	private ModStackFloat _mainWeaponDamageMod;

	private YieldInstruction periodYI;

	private float _clearTime;

	private GameObject _onActiveFX;

	private void Start()
	{
		periodYI = new WaitForSeconds(1f);
		Vehicle component = GetComponent<Vehicle>();
		if (component != null)
		{
			_mainWeapon = component.weapon;
			_destructible = component.destructible;
			_buffSystem = component.buffSystem;
			_photonView = component.photonView;
			_fx = GetComponent<AbilitiesFXDispatcher>();
			if ((bool)_mainWeapon)
			{
				_mainWeapon.makeDamageEvent += OnWeaponMakeDamage;
			}
			if ((bool)_destructible)
			{
				_destructible.damagedEvent += OnDamaged;
			}
			if (_buffSystem != null)
			{
				_mainWeaponDamageMod = _buffSystem.SetupModStack(_mainWeapon.GetBaseDamage, _mainWeapon.SetDamage, ModDamage, 0);
			}
		}
	}

	private void OnEnable()
	{
		_collectedDamagePerSecForActivate = 0f;
		_collectedDamageForDeactivate = 0f;
		StartCoroutine(CheckDamageBonus());
		StartCoroutine(ClearRage());
	}

	public void OnWeaponMakeDamage(Destructible destructible, float damage)
	{
		_collectedDamagePerSecForActivate += damage;
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		_collectedDamageForDeactivate += damage;
		if (_rageCoeff != 0f && _collectedDamageForDeactivate >= DamageForDeactivate)
		{
			StopRage();
			if ((bool)_photonView)
			{
				_photonView.RPC("StopRage", PhotonTargets.Others);
			}
		}
	}

	private IEnumerator CheckDamageBonus()
	{
		while (true)
		{
			yield return periodYI;
			if (_collectedDamagePerSecForActivate >= DamageForActivate)
			{
				float rage = _rageCoeff + 1f;
				SetRage(rage);
				if ((bool)_photonView)
				{
					_photonView.RPC("SetRage", PhotonTargets.Others, rage);
				}
			}
			_collectedDamagePerSecForActivate = 0f;
			_collectedDamageForDeactivate = 0f;
		}
	}

	private IEnumerator ClearRage()
	{
		while (true)
		{
			yield return periodYI;
			if (Time.time > _clearTime)
			{
				StopRage();
			}
		}
	}

	//[RPC]
	private void SetRage(float rage)
	{
		if (rage > _rageCoeff)
		{
			_rageCoeff = rage;
			_mainWeaponDamageMod.Recalculate();
			if ((bool)_fx)
			{
				_fx.ActivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.Rage, false);
			}
		}
		_clearTime = Time.time + 1f;
	}

	//[RPC]
	private void StopRage()
	{
		if (_rageCoeff != 0f)
		{
			_rageCoeff = 0f;
			_mainWeaponDamageMod.Recalculate();
			if ((bool)_fx)
			{
				_fx.DeactivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.Rage, false);
			}
		}
	}

	public float ModDamage(float modifiedDamage, float baseDamage)
	{
		float num = baseDamage * DamageBoostPerSec * _rageCoeff * EffectScale * (float)Stacks;
		return Mathf.Max(modifiedDamage + num, 0f);
	}
}
