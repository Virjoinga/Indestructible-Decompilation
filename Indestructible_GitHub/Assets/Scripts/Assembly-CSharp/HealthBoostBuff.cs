using UnityEngine;

public class HealthBoostBuff : Buff
{
	public bool AbsoluteValue = true;

	public float MaxHPBoost;

	public bool HealToMax = true;

	private Destructible _destructible;

	private ModStackFloat _maxHPMod;

	public HealthBoostBuff(HealthBoostConf config)
		: base(config)
	{
		AbsoluteValue = config.AbsoluteValue;
		MaxHPBoost = config.MaxHPBoost;
		HealToMax = config.HealToMax;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _vehicle.destructible;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			_maxHPMod = _buffSystem.SetupModStack(_destructible.GetBaseMaxHP, _destructible.SetMaxHP, ModMaxHealth, 0);
			if (HealToMax)
			{
				_destructible.Heal(_maxHPMod.Value);
			}
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_maxHPMod.RemoveValueModifier(ModMaxHealth);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_maxHPMod);
	}

	public float ModMaxHealth(float modifiedHealth, float baseHealth)
	{
		float num = ((!AbsoluteValue) ? (baseHealth * MaxHPBoost) : MaxHPBoost) * base.effectScale * (float)Stacks;
		return modifiedHealth + num;
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		info.Health = ModMaxHealth(info.Health, baseInfo.Health);
	}
}
