public class PassiveAbilitiesBoostBuff : Buff
{
	public bool AbsoluteValue = true;

	public float PassiveAbilityBoost;

	private ModStackFloat _passiveAbilityScaleMod;

	public PassiveAbilitiesBoostBuff(PassiveAbilitiesBoostConf config)
		: base(config)
	{
		AbsoluteValue = config.AbsoluteValue;
		PassiveAbilityBoost = config.PassiveAbilityBoost;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			PassiveAbilityBase component = _target.GetComponent<PassiveAbilityBase>();
			if ((bool)component)
			{
				_passiveAbilityScaleMod = _buffSystem.SetupModStack(component.GetBaseEffectScale, component.SetEffectScale, ModPassiveAbilityScale, 0);
			}
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_passiveAbilityScaleMod.RemoveValueModifier(ModPassiveAbilityScale);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_passiveAbilityScaleMod);
	}

	public float ModPassiveAbilityScale(float modifiedPassiveAbilityScale, float basePassiveAbilityScale)
	{
		float num = ((!AbsoluteValue) ? (basePassiveAbilityScale * PassiveAbilityBoost) : PassiveAbilityBoost) * base.effectScale * (float)Stacks;
		return modifiedPassiveAbilityScale + num;
	}
}
