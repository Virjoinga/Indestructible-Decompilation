using UnityEngine;

public class DamageBoostOnAttackBuff : Buff
{
	public float DamageBoostPerSec;

	public float DamageForActivate = 3f;

	public float DamageForDeactivate = 3f;

	public GameObject OnActiveFX;

	private AttackRage _attackRage;

	public DamageBoostOnAttackBuff(DamageBoostOnAttackConf config)
		: base(config)
	{
		DamageBoostPerSec = config.DamageBoostPerSec;
		DamageForActivate = config.DamageForActivate;
		DamageForDeactivate = config.DamageForDeactivate;
		OnActiveFX = config.OnActiveFX;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_attackRage = targetGO.GetComponent<AttackRage>();
		_attackRage = _attackRage ?? targetGO.AddComponent<AttackRage>();
		_attackRage.Stacks = Stacks;
		_attackRage.DamageBoostPerSec = DamageBoostPerSec;
		_attackRage.DamageForActivate = DamageForActivate;
		_attackRage.DamageForDeactivate = DamageForDeactivate;
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		if ((bool)_attackRage)
		{
			Object.Destroy(_attackRage);
		}
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_attackRage.Stacks = Stacks;
	}
}
