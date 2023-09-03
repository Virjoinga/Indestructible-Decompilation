using UnityEngine;

public class DamageReflectionBuff : Buff
{
	public int ChancePercent = 1;

	public float ReactivateTime = 2f;

	public GameObject StartFX;

	public GameObject HitFX;

	private DamageReflector _damageReflector;

	public DamageReflectionBuff(DamageReflectionConf config)
		: base(config)
	{
		ChancePercent = config.ChancePercent;
		ReactivateTime = config.ReactivateTime;
		StartFX = config.StartFX;
		HitFX = config.HitFX;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_damageReflector = targetGO.GetComponent<DamageReflector>();
		_damageReflector = _damageReflector ?? targetGO.AddComponent<DamageReflector>();
		_damageReflector.Stacks = 1;
		_damageReflector.StartFX = StartFX;
		_damageReflector.HitFX = HitFX;
		_damageReflector.ChancePercent = ChancePercent;
		_damageReflector.ReactivateTime = ReactivateTime;
	}

	protected override void OnFinish()
	{
		if ((bool)_damageReflector)
		{
			Object.Destroy(_damageReflector);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		if ((bool)_damageReflector)
		{
			_damageReflector.Stacks = Stacks;
		}
	}
}
