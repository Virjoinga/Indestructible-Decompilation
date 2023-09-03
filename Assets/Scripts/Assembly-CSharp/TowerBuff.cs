using UnityEngine;

public class TowerBuff : Buff
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public bool SpeedAbsoluteValue = true;

	public float SpeedBoost;

	public float DamageReduceInPercent;

	private ModStackFloat _damageMod;

	private ModStackFloat _speedMod;

	private ModStackFloat _damageReducerMod;

	public TowerBuff(TowerConf config)
		: base(config)
	{
		DamageAbsoluteValue = config.DamageAbsoluteValue;
		DamageBoost = config.DamageBoost;
		SpeedAbsoluteValue = config.SpeedAbsoluteValue;
		SpeedBoost = config.SpeedBoost;
		DamageReduceInPercent = config.DamageReduceInPercent;
	}

	protected override void OnStart()
	{
		DamageReduceInPercent /= 100f;
		if (_buffSystem != null)
		{
			SetupWeaponDamageMod();
			SetupSpeedMod();
			SetupDmgReducerMod();
		}
	}

	private void SetupWeaponDamageMod()
	{
		_damageMod = _buffSystem.SetupModStack(_vehicle.weapon.GetBaseDamage, _vehicle.weapon.SetDamage, ModDamage, 0);
	}

	private void SetupSpeedMod()
	{
		Engine component = _target.GetComponent<Engine>();
		if (component != null)
		{
			_speedMod = _buffSystem.SetupModStack(component.GetBaseMaxSpeed, component.SetMaxSpeed, ModMaxSpeed, 0);
		}
	}

	private void SetupDmgReducerMod()
	{
		_damageReducerMod = _buffSystem.SetupModStack(_vehicle.destructible.GetDamageReducer, _vehicle.destructible.SetDamageReducer, ModDamageReducer, 0);
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			if (_damageMod != null)
			{
				_damageMod.RemoveValueModifier(ModDamage);
			}
			if (_speedMod != null)
			{
				_speedMod.RemoveValueModifier(ModMaxSpeed);
			}
			if (_damageReducerMod != null)
			{
				_damageReducerMod.RemoveValueModifier(ModDamageReducer);
			}
			_buffSystem.RecalculateValue(_damageMod);
			_buffSystem.RecalculateValue(_speedMod);
			_buffSystem.RecalculateValue(_damageReducerMod);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_damageMod);
		_buffSystem.RecalculateValue(_speedMod);
		_buffSystem.RecalculateValue(_damageReducerMod);
	}

	private float ModDamage(float modifiedDamage, float baseDamage)
	{
		float num = ((!DamageAbsoluteValue) ? (baseDamage * DamageBoost) : DamageBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedDamage + num, 0f);
	}

	private float ModMaxSpeed(float modifiedSpeed, float baseSpeed)
	{
		float num = ((!SpeedAbsoluteValue) ? (baseSpeed * SpeedBoost) : SpeedBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedSpeed + num, 0f);
	}

	public float ModDamageReducer(float modifiedDamageReducer, float baseDamageReducer)
	{
		return modifiedDamageReducer + DamageReduceInPercent * base.effectScale * (float)Stacks;
	}
}
