using System.Collections;
using UnityEngine;

public class TorqueBoostOnDamageBuff : Buff
{
	public bool TorqueAbsoluteValue = true;

	public float TorqueBoost;

	public bool MaxSpeedAbsoluteValue = true;

	public float MaxSpeedBoost;

	public float DamageForActivate = 1f;

	public float AccumulatedDamageReducePerSec = 1f;

	public float ActiveTime = 2f;

	private bool _active;

	private Destructible _destructible;

	private float _accumulatedDamage;

	private YieldInstruction _delayInstruction;

	private ModStackFloat _speedMod;

	private ModStackFloat _torqueMod;

	public TorqueBoostOnDamageBuff(TorqueBoostOnDamageConf config)
		: base(config)
	{
		TorqueAbsoluteValue = config.TorqueAbsoluteValue;
		TorqueBoost = config.TorqueBoost;
		MaxSpeedAbsoluteValue = config.MaxSpeedAbsoluteValue;
		MaxSpeedBoost = config.MaxSpeedBoost;
		DamageForActivate = config.DamageForActivate;
		AccumulatedDamageReducePerSec = config.AccumulatedDamageReducePerSec;
		ActiveTime = config.ActiveTime;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _vehicle.destructible;
		if ((bool)_destructible)
		{
			_destructible.damagedEvent += OnDamaged;
			_destructible.destructedEvent += OnDestructed;
		}
		_delayInstruction = new WaitForSeconds(ActiveTime);
		base.isUpdatable = true;
		base.updateRate = 1f;
		base.shouldUpdateFromStart = false;
	}

	protected override void OnTick(int tickCount)
	{
		_accumulatedDamage -= AccumulatedDamageReducePerSec;
		if (_accumulatedDamage < 0f)
		{
			_accumulatedDamage = 0f;
		}
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
		if (!_active)
		{
			_accumulatedDamage += damage;
			if (_accumulatedDamage > DamageForActivate)
			{
				_accumulatedDamage = 0f;
				_active = true;
				_buffSystem.RecalculateValue(_torqueMod);
				_buffSystem.RecalculateValue(_speedMod);
				_buffSystem.StartCoroutine(DelayedDeactivate());
			}
		}
	}

	public void OnDestructed(Destructible destructed)
	{
		_accumulatedDamage = 0f;
		_active = false;
	}

	private IEnumerator DelayedDeactivate()
	{
		yield return _delayInstruction;
		_active = false;
		_buffSystem.RecalculateValue(_torqueMod);
		_buffSystem.RecalculateValue(_speedMod);
	}

	public float ModTorque(float modifiedTorque, float baseTorque)
	{
		if (!_active)
		{
			return modifiedTorque;
		}
		float num = ((!TorqueAbsoluteValue) ? (baseTorque * TorqueBoost) : TorqueBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedTorque + num, 0f);
	}

	public float ModMaxSpeed(float modifiedSpeed, float baseSpeed)
	{
		if (!_active)
		{
			return modifiedSpeed;
		}
		float num = ((!MaxSpeedAbsoluteValue) ? (baseSpeed * MaxSpeedBoost) : MaxSpeedBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedSpeed + num, 0f);
	}

	public override void Reactivate()
	{
		base.Reactivate();
		_accumulatedDamage = 0f;
		_active = false;
	}
}
