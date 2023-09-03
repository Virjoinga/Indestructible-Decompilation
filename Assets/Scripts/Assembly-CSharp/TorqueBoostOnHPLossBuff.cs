using UnityEngine;

public class TorqueBoostOnHPLossBuff : Buff
{
	public float TorquePercentAddPerLossPercent = 0.25f;

	public float MaxSpeedPercentAddPerLossPercent = 0.25f;

	private Destructible _destructible;

	private float _lossPercent;

	private ModStackFloat _speedMod;

	private ModStackFloat _torqueMod;

	public TorqueBoostOnHPLossBuff(TorqueBoostOnHPLossConf config)
		: base(config)
	{
		TorquePercentAddPerLossPercent = config.TorquePercentAddPerLossPercent;
		MaxSpeedPercentAddPerLossPercent = config.MaxSpeedPercentAddPerLossPercent;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _target.GetComponent<Destructible>();
		if ((bool)_destructible)
		{
			_destructible.damagedEvent += OnDamaged;
			_destructible.destructedEvent += OnDestructed;
			_destructible.healedEvent += OnHealed;
		}
		TorquePercentAddPerLossPercent /= 100f;
		MaxSpeedPercentAddPerLossPercent /= 100f;
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
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_torqueMod.RemoveValueModifier(ModTorque);
			_speedMod.RemoveValueModifier(ModMaxSpeed);
		}
		if ((bool)_destructible)
		{
			_destructible.damagedEvent -= OnDamaged;
			_destructible.destructedEvent -= OnDestructed;
			_destructible.healedEvent -= OnHealed;
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_torqueMod);
		_buffSystem.RecalculateValue(_speedMod);
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		if ((bool)_destructible)
		{
			_lossPercent = (1f - _destructible.hp / _destructible.GetMaxHP()) * 100f;
			_buffSystem.RecalculateValue(_torqueMod);
			_buffSystem.RecalculateValue(_speedMod);
		}
	}

	public void OnHealed(float hpGained, Destructible destructible)
	{
		if ((bool)_destructible)
		{
			_lossPercent = (1f - _destructible.hp / _destructible.GetMaxHP()) * 100f;
			_buffSystem.RecalculateValue(_torqueMod);
			_buffSystem.RecalculateValue(_speedMod);
		}
	}

	public void OnDestructed(Destructible destructed)
	{
		_lossPercent = 0f;
	}

	public float ModTorque(float modifiedTorque, float baseTorque)
	{
		return modifiedTorque * (1f + _lossPercent * TorquePercentAddPerLossPercent * base.effectScale * (float)Stacks);
	}

	public float ModMaxSpeed(float modifiedSpeed, float baseSpeed)
	{
		return modifiedSpeed * (1f + _lossPercent * MaxSpeedPercentAddPerLossPercent * base.effectScale * (float)Stacks);
	}
}
