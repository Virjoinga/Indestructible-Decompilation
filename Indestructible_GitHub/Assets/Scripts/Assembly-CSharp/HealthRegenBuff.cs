using UnityEngine;

public class HealthRegenBuff : Buff
{
	public float HealPerSecond = 10f;

	private Destructible _destructible;

	public HealthRegenBuff(HealthRegenConf config)
		: base(config)
	{
		HealPerSecond = config.HealPerSecond;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _target.GetComponent<Destructible>();
		base.isUpdatable = true;
		base.updateRate = 1f;
		base.shouldUpdateFromStart = false;
	}

	protected override void OnTick(int tickCount)
	{
		if ((bool)_destructible)
		{
			_destructible.Heal(HealPerSecond * (float)Stacks * (float)tickCount);
		}
	}
}
