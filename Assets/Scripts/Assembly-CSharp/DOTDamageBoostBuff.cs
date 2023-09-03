using UnityEngine;

public class DOTDamageBoostBuff : Buff
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public bool DurationAbsoluteValue = true;

	public float DurationBoost;

	private ModStackFloat _thermalDOTDamageMod;

	private ModStackFloat _thermalDOTDurationMod;

	public DOTDamageBoostBuff(DOTDamageBoostConf config)
		: base(config)
	{
		DamageAbsoluteValue = config.DamageAbsoluteValue;
		DamageBoost = config.DamageBoost;
		DurationAbsoluteValue = config.DurationAbsoluteValue;
		DurationBoost = config.DurationBoost;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			IDOTWeapon iDOTWeapon = _vehicle.weapon as IDOTWeapon;
			if (iDOTWeapon != null)
			{
				DOTWeaponImp dotInterface = iDOTWeapon.dotInterface;
				_thermalDOTDamageMod = _buffSystem.SetupModStack(dotInterface.GetBaseDamage, dotInterface.SetDamage, ModDOTDamage, 0);
				_thermalDOTDurationMod = _buffSystem.SetupModStack(dotInterface.GetBaseDuration, dotInterface.SetDuration, ModDOTDuration, 0);
			}
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			if (_thermalDOTDamageMod != null)
			{
				_thermalDOTDamageMod.RemoveValueModifier(ModDOTDamage);
			}
			if (_thermalDOTDurationMod != null)
			{
				_thermalDOTDurationMod.RemoveValueModifier(ModDOTDuration);
			}
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_thermalDOTDamageMod);
		_buffSystem.RecalculateValue(_thermalDOTDurationMod);
	}

	public float ModDOTDamage(float modifiedDOT, float baseDOT)
	{
		float num = ((!DamageAbsoluteValue) ? (baseDOT * DamageBoost) : DamageBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedDOT + num, 0f);
	}

	public float ModDOTDuration(float modifiedDuration, float baseDuration)
	{
		float num = ((!DurationAbsoluteValue) ? (baseDuration * DurationBoost) : DurationBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedDuration + num, 0f);
	}
}
