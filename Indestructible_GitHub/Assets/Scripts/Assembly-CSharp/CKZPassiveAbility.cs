using UnityEngine;

public class CKZPassiveAbility : PassiveAbilityBase
{
	public float ActivationHealthFraction = 0.25f;

	public float DamageBoost = 2f;

	private GameObject _rootObject;

	private Destructible _destructible;

	private BuffSystem _buffSystem;

	private Buff _buff;

	protected virtual void Start()
	{
		_rootObject = base.transform.root.gameObject;
		_buffSystem = _rootObject.GetComponent<BuffSystem>();
		_destructible = _rootObject.GetComponent<Destructible>();
	}

	private void Update()
	{
		if ((bool)_destructible && _destructible.hp <= _destructible.GetMaxHP() * ActivationHealthFraction)
		{
			if (_buff == null && !(_buffSystem == null))
			{
				DamageBoostBuff damageBoostBuff = _buffSystem.AddBuffSuspended<DamageBoostBuff>(this);
				if (damageBoostBuff != null)
				{
					damageBoostBuff.BoostDamageType = DamageBoostBuff.BoostType.MainWeapon;
					damageBoostBuff.duration = -1f;
					damageBoostBuff.AbsoluteValue = false;
					damageBoostBuff.DamageBoost = DamageBoost * _effectScale;
					damageBoostBuff.FXVisible = true;
					damageBoostBuff.isVisible = true;
					damageBoostBuff.StartBuff();
					_buff = damageBoostBuff;
				}
			}
		}
		else if (_buff != null)
		{
			_buffSystem.RemoveBuff(_buff, false);
			_buff = null;
		}
	}
}
