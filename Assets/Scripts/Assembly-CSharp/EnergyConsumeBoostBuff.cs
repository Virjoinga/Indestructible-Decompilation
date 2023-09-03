public class EnergyConsumeBoostBuff : Buff
{
	public enum ConsumerType
	{
		MainWeapon = 0,
		Ability = 1,
		Both = 2
	}

	public ConsumerType EnergyConsumerType;

	public bool AbsoluteValue = true;

	public float EnergyConsumeBoost;

	private ModStackFloat _mainWeaponEnergyConsume;

	private ModStackFloat _abilityEnergyConsume;

	public EnergyConsumeBoostBuff(EnergyConsumeBoostConf config)
		: base(config)
	{
		EnergyConsumerType = config.EnergyConsumerType;
		AbsoluteValue = config.AbsoluteValue;
		EnergyConsumeBoost = config.EnergyConsumeBoost;
	}

	protected override void OnStart()
	{
		if (!(_buffSystem != null))
		{
			return;
		}
		switch (EnergyConsumerType)
		{
		case ConsumerType.MainWeapon:
			_mainWeaponEnergyConsume = _buffSystem.SetupModStack(_vehicle.weapon.GetBaseShotEnergyConsumption, _vehicle.weapon.SetShotEnergyConsumption, ModEnergyConsume, 0);
			break;
		case ConsumerType.Ability:
		{
			BaseActiveAbility component2 = _target.GetComponent<BaseActiveAbility>();
			if ((bool)component2)
			{
				_abilityEnergyConsume = _buffSystem.SetupModStack(component2.GetBaseEnergyConsume, component2.SetEnergyConsume, ModEnergyConsume, 0);
			}
			break;
		}
		case ConsumerType.Both:
		{
			_mainWeaponEnergyConsume = _buffSystem.SetupModStack(_vehicle.weapon.GetBaseShotEnergyConsumption, _vehicle.weapon.SetShotEnergyConsumption, ModEnergyConsume, 0);
			BaseActiveAbility component = _target.GetComponent<BaseActiveAbility>();
			if ((bool)component)
			{
				_abilityEnergyConsume = _buffSystem.SetupModStack(component.GetBaseEnergyConsume, component.SetEnergyConsume, ModEnergyConsume, 0);
			}
			break;
		}
		}
	}

	protected override void OnFinish()
	{
		if (_mainWeaponEnergyConsume != null)
		{
			_mainWeaponEnergyConsume.RemoveValueModifier(ModEnergyConsume);
		}
		if (_abilityEnergyConsume != null)
		{
			_abilityEnergyConsume.RemoveValueModifier(ModEnergyConsume);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_mainWeaponEnergyConsume);
		_buffSystem.RecalculateValue(_abilityEnergyConsume);
	}

	public float ModEnergyConsume(float modifiedEnergyConsume, float baseEnergyConsume)
	{
		float num = ((!AbsoluteValue) ? (baseEnergyConsume * EnergyConsumeBoost) : EnergyConsumeBoost) * base.effectScale * (float)Stacks;
		return modifiedEnergyConsume + num;
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		if (EnergyConsumerType == ConsumerType.MainWeapon || EnergyConsumerType == ConsumerType.Both)
		{
			info.EnergyShot = ModEnergyConsume(info.EnergyShot, baseInfo.EnergyShot);
		}
	}
}
