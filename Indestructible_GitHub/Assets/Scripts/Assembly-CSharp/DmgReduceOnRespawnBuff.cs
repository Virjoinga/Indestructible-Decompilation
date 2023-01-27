using System.Collections;
using UnityEngine;

public class DmgReduceOnRespawnBuff : Buff
{
	public float ReduceInPercent;

	public float ActiveTime = 2f;

	public GameObject OnActiveFX;

	private bool _active;

	private YieldInstruction _delayInstruction;

	private ModStackFloat _damageReducerMod;

	private GameObject _onActiveFX;

	public DmgReduceOnRespawnBuff(DmgReduceOnRespawnConf config)
		: base(config)
	{
		ReduceInPercent = config.ReduceInPercent;
		ActiveTime = config.ActiveTime;
		OnActiveFX = config.OnActiveFX;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		ReduceInPercent /= 100f;
		_delayInstruction = new WaitForSeconds(ActiveTime);
		if ((bool)OnActiveFX && (bool)_buffSystem)
		{
			_onActiveFX = (GameObject)Object.Instantiate(OnActiveFX);
			_onActiveFX.transform.parent = _buffSystem.transform;
			_onActiveFX.transform.localPosition = Vector3.zero;
			_onActiveFX.SetActiveRecursively(false);
		}
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			_damageReducerMod = _buffSystem.SetupModStack(_vehicle.destructible.GetBaseDamageReducer, _vehicle.destructible.SetDamageReducer, ModDamageReducer, 0);
			if ((bool)_onActiveFX)
			{
				_onActiveFX.SetActiveRecursively(true);
			}
			_active = true;
			_buffSystem.StartCoroutine(DelayedDeactivate());
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_damageReducerMod.RemoveValueModifier(ModDamageReducer);
			if ((bool)_onActiveFX)
			{
				Object.Destroy(_onActiveFX);
			}
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
		return modifiedDamageReducer + ((!_active) ? 0f : (ReduceInPercent * base.effectScale * (float)Stacks));
	}

	private IEnumerator DelayedDeactivate()
	{
		yield return _delayInstruction;
		_active = false;
		_buffSystem.RecalculateValue(_damageReducerMod);
		if ((bool)_onActiveFX)
		{
			_onActiveFX.SetActiveRecursively(false);
		}
	}

	public override void Reactivate()
	{
		_active = true;
		if ((bool)_onActiveFX)
		{
			_onActiveFX.SetActiveRecursively(true);
		}
		_buffSystem.StartCoroutine(DelayedDeactivate());
		if (_damageReducerMod != null)
		{
			_damageReducerMod.Recalculate();
		}
	}
}
