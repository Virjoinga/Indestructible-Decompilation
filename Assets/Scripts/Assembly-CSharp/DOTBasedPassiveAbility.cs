using System.Collections.Generic;
using UnityEngine;

public class DOTBasedPassiveAbility : PassiveAbilityBase
{
	public struct DamageFraction
	{
		public float time;

		public float damage;

		public DamageFraction(float _time, float _damage)
		{
			time = _time;
			damage = _damage;
		}
	}

	private const int MaxDamageFractions = 10;

	public float AccumulationPeriod = 1f;

	public float DamageLimit = 20f;

	public bool inbound;

	protected List<DamageFraction> _dmgList = new List<DamageFraction>(10);

	protected MainWeapon _weapon;

	protected Destructible _destructible;

	protected virtual void Start()
	{
		_weapon = GetComponentInChildren<MainWeapon>();
		_destructible = GetComponentInChildren<Destructible>();
		if (!inbound)
		{
			if (_weapon != null)
			{
				_weapon.makeDamageEvent += OnWeaponMakeDamage;
			}
		}
		else if (_destructible != null)
		{
			_destructible.damagedEvent += OnDamaged;
		}
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		OnDamage(destructible, damage);
	}

	private void OnWeaponMakeDamage(Destructible destructible, float damage)
	{
		OnDamage(destructible, damage);
	}

	public virtual void OnDamage(Destructible destructible, float damage)
	{
		float num = damage;
		float num2 = Time.time - AccumulationPeriod;
		for (int i = 0; i < _dmgList.Count; i++)
		{
			if (_dmgList[i].time < num2)
			{
				_dmgList.RemoveAt(i--);
			}
			else
			{
				num += _dmgList[i].damage;
			}
		}
		_dmgList.Add(new DamageFraction(Time.time, damage));
		if (num > DamageLimit)
		{
			HandleLimitBreak();
		}
	}

	protected virtual void HandleLimitBreak()
	{
		_dmgList.Clear();
	}
}
