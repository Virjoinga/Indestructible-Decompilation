using UnityEngine;

public class TorqueBoostBuff : Buff
{
	public bool TorqueAbsoluteValue = true;

	public float TorqueBoost;

	public bool MaxSpeedAbsoluteValue = true;

	public float MaxSpeedBoost;

	public bool EffectVisible = true;

	private AbilitiesFXDispatcher _fx;

	private ModStackFloat _speedMod;

	private ModStackFloat _torqueMod;

	public TorqueBoostBuff(TorqueBoostConf config)
		: base(config)
	{
		TorqueAbsoluteValue = config.TorqueAbsoluteValue;
		TorqueBoost = config.TorqueBoost;
		MaxSpeedAbsoluteValue = config.MaxSpeedAbsoluteValue;
		MaxSpeedBoost = config.MaxSpeedBoost;
		EffectVisible = config.EffectVisible;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			Engine component = _target.GetComponent<Engine>();
			if (component != null)
			{
				_torqueMod = _buffSystem.SetupModStack(component.GetBaseMaxTorque, component.SetMaxTorque, ModTorque, 0);
				_speedMod = _buffSystem.SetupModStack(component.GetBaseMaxSpeed, component.SetMaxSpeed, ModMaxSpeed, 0);
			}
		}
		_fx = _target.GetComponent<AbilitiesFXDispatcher>();
		if (EffectVisible && (bool)_fx)
		{
			_fx.ActivateBoostEffect(true);
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_torqueMod.RemoveValueModifier(ModTorque);
			_speedMod.RemoveValueModifier(ModMaxSpeed);
			_torqueMod.Recalculate();
			_speedMod.Recalculate();
		}
		base.OnFinish();
		if (EffectVisible && (bool)_fx)
		{
			_fx.DeactivateBoostEffect(true);
		}
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_torqueMod);
		_buffSystem.RecalculateValue(_speedMod);
	}

	public float ModTorque(float modifiedTorque, float baseTorque)
	{
		float num = ((!TorqueAbsoluteValue) ? (baseTorque * TorqueBoost) : TorqueBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedTorque + num, 0f);
	}

	public float ModMaxSpeed(float modifiedSpeed, float baseSpeed)
	{
		float num = ((!MaxSpeedAbsoluteValue) ? (baseSpeed * MaxSpeedBoost) : MaxSpeedBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedSpeed + num, 0f);
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		info.Speed = ModMaxSpeed(info.Speed, baseInfo.Speed);
	}
}
