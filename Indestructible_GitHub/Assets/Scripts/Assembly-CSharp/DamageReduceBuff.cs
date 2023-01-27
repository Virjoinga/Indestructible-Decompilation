using UnityEngine;

public class DamageReduceBuff : Buff
{
	public float ReduceInPercent;

	private YieldInstruction _delayInstruction;

	private ModStackFloat _damageReducerMod;

	public DamageReduceBuff(DamageReduceConf config)
		: base(config)
	{
		ReduceInPercent = config.ReduceInPercent;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		ReduceInPercent /= 100f;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			_damageReducerMod = _buffSystem.SetupModStack(_vehicle.destructible.GetBaseDamageReducer, _vehicle.destructible.SetDamageReducer, ModDamageReducer, 0);
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_damageReducerMod.RemoveValueModifier(ModDamageReducer);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_damageReducerMod);
	}

	public float ModDamageReducer(float modifiedDamageReducer, float baseDamageReducer)
	{
		return modifiedDamageReducer + ReduceInPercent * base.effectScale * (float)Stacks;
	}
}
