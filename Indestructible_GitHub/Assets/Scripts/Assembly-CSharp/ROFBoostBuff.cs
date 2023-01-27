using UnityEngine;

public class ROFBoostBuff : Buff
{
	public enum BoostType
	{
		MainWeapon = 0,
		Explosive = 1,
		Kinetic = 2
	}

	public BoostType Type;

	public bool AbsoluteValue = true;

	public float RateOfFireBoost;

	public bool FXVisible;

	private AbilitiesFXDispatcher _fx;

	private ModStackFloat _curFireintervalMod;

	public ROFBoostBuff()
	{
	}

	public ROFBoostBuff(ROFBoostConf config)
		: base(config)
	{
		Type = config.Type;
		AbsoluteValue = config.AbsoluteValue;
		RateOfFireBoost = config.RateOfFireBoost;
		FXVisible = config.FXVisible;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			Cannon cannon = _vehicle.weapon as Cannon;
			switch (Type)
			{
			case BoostType.MainWeapon:
				if (cannon != null)
				{
					_curFireintervalMod = _buffSystem.SetupModStack(cannon.GetBaseFireInterval, cannon.SetFireInterval, ModRateOfFire, 0);
				}
				break;
			case BoostType.Explosive:
				if (cannon != null && cannon.damageType == DamageType.Explosive)
				{
					_curFireintervalMod = _buffSystem.SetupModStack(cannon.GetBaseFireInterval, cannon.SetFireInterval, ModRateOfFire, 0);
				}
				break;
			case BoostType.Kinetic:
				if (cannon != null && cannon.damageType == DamageType.Kinetic)
				{
					_curFireintervalMod = _buffSystem.SetupModStack(cannon.GetBaseFireInterval, cannon.SetFireInterval, ModRateOfFire, 0);
				}
				break;
			}
		}
		if (_curFireintervalMod == null)
		{
			_finished = true;
			_isStarted = false;
			base.startTime = 0f;
		}
		_fx = _target.GetComponent<AbilitiesFXDispatcher>();
		if (FXVisible && (bool)_fx)
		{
			_fx.ActivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.ROFBoost, true);
		}
	}

	protected override void OnFinish()
	{
		if (_curFireintervalMod != null)
		{
			_curFireintervalMod.RemoveValueModifier(ModRateOfFire);
		}
		_buffSystem.RecalculateValue(_curFireintervalMod);
		base.OnFinish();
		if (FXVisible && (bool)_fx)
		{
			_fx.DeactivateWeaponEffect(AbilitiesFXDispatcher.WeaponEffect.ROFBoost, true);
		}
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_curFireintervalMod);
	}

	public float ModRateOfFire(float modifiedRate, float baseRate)
	{
		float num = ((baseRate == 0f) ? 0f : (1f / baseRate));
		float num2 = ((modifiedRate == 0f) ? 0f : (1f / modifiedRate));
		float num3 = ((!AbsoluteValue) ? (num * RateOfFireBoost) : RateOfFireBoost) * base.effectScale * (float)Stacks;
		float num4 = Mathf.Max(0f, num2 + num3);
		return (num4 == 0f) ? 0f : (1f / num4);
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		if (Type == BoostType.MainWeapon)
		{
			if (baseInfo.WeaponDamageType != DamageType.Thermal)
			{
				info.FireInterval = ModRateOfFire(info.FireInterval, baseInfo.FireInterval);
			}
		}
		else if (Type == BoostType.Explosive && baseInfo.WeaponDamageType == DamageType.Explosive)
		{
			info.FireInterval = ModRateOfFire(info.FireInterval, baseInfo.FireInterval);
		}
		else if (Type == BoostType.Kinetic && baseInfo.WeaponDamageType == DamageType.Kinetic)
		{
			info.FireInterval = ModRateOfFire(info.FireInterval, baseInfo.FireInterval);
		}
	}
}
