using UnityEngine;

public class DamageBoostBuff : Buff
{
	public enum BoostType
	{
		MainWeapon = 0,
		Collision = 1,
		MainWeaponAndCollision = 2,
		Explosive = 3,
		Kinetic = 4,
		Thermal = 5
	}

	public BoostType BoostDamageType;

	public bool ModBaseValue = true;

	public bool AbsoluteValue = true;

	public float DamageBoost;

	public bool FXVisible;

	private ModStackFloat _damageMod;

	private ModStackFloat _collideDamageMod;

	private AbilitiesFXDispatcher _fx;

	public DamageBoostBuff()
	{
	}

	public DamageBoostBuff(DamageBoostConf config)
		: base(config)
	{
		BoostDamageType = config.BoostDamageType;
		ModBaseValue = config.ModBaseValue;
		AbsoluteValue = config.AbsoluteValue;
		DamageBoost = config.DamageBoost;
		FXVisible = config.FXVisible;
	}

	protected override void OnStart()
	{
		if (!(_buffSystem != null))
		{
			return;
		}
		switch (BoostDamageType)
		{
		case BoostType.MainWeapon:
			SetupWeaponDamageMod();
			break;
		case BoostType.Collision:
			SetupColisionDamageMod();
			break;
		case BoostType.MainWeaponAndCollision:
			SetupWeaponDamageMod();
			SetupColisionDamageMod();
			break;
		case BoostType.Explosive:
			if (_vehicle.weapon.damageType == DamageType.Explosive)
			{
				SetupWeaponDamageMod();
			}
			break;
		case BoostType.Kinetic:
			if (_vehicle.weapon.damageType == DamageType.Kinetic)
			{
				SetupWeaponDamageMod();
			}
			break;
		case BoostType.Thermal:
			if (_vehicle.weapon.damageType == DamageType.Thermal)
			{
				SetupWeaponDamageMod();
			}
			break;
		}
		if (_damageMod == null && _collideDamageMod == null)
		{
			_finished = true;
			_isStarted = false;
			base.startTime = 0f;
		}
		_fx = _target.GetComponent<AbilitiesFXDispatcher>();
		if (BoostDamageType != BoostType.Collision && FXVisible && (bool)_fx)
		{
			_fx.ActivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.DamageBoost, false);
		}
	}

	private void SetupWeaponDamageMod()
	{
		_damageMod = _buffSystem.SetupModStack(_vehicle.weapon.GetBaseDamage, _vehicle.weapon.SetDamage, ModDamage, (!ModBaseValue) ? 10 : 0);
	}

	private void SetupColisionDamageMod()
	{
		_collideDamageMod = _buffSystem.SetupModStack(_vehicle.GetBaseDamage, _vehicle.SetDamage, ModDamage, (!ModBaseValue) ? 10 : 0);
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			if (_damageMod != null)
			{
				_damageMod.RemoveValueModifier(ModDamage);
			}
			if (_collideDamageMod != null)
			{
				_collideDamageMod.RemoveValueModifier(ModDamage);
			}
			_buffSystem.RecalculateValue(_damageMod);
			_buffSystem.RecalculateValue(_collideDamageMod);
		}
		if ((bool)_fx)
		{
			_fx.DeactivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.DamageBoost, false);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_damageMod);
		_buffSystem.RecalculateValue(_collideDamageMod);
	}

	public float ModDamage(float modifiedDamage, float baseDamage)
	{
		float num = ((!ModBaseValue) ? modifiedDamage : baseDamage);
		float num2 = ((!AbsoluteValue) ? (num * DamageBoost) : DamageBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedDamage + num2, 0f);
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		DamageType[] array = new DamageType[6]
		{
			DamageType.Generic,
			DamageType.Collision,
			DamageType.Generic,
			DamageType.Explosive,
			DamageType.Kinetic,
			DamageType.Thermal
		};
		if (BoostDamageType == BoostType.MainWeapon || BoostDamageType == BoostType.MainWeaponAndCollision || baseInfo.WeaponDamageType == array[(int)BoostDamageType])
		{
			info.Damage = ModDamage(info.Damage, baseInfo.Damage);
		}
	}
}
