using UnityEngine;

public class ActiveAbilityBoostBuff : Buff
{
	public bool AbsoluteValue = true;

	public float CooldownBoost;

	public float AddEffectMultiplier;

	private CooldownAbility _cooldownAbility;

	private ModStackFloat _abilityCooldownMod;

	public ActiveAbilityBoostBuff(ActiveAbilityBoostConf config)
		: base(config)
	{
		AbsoluteValue = config.AbsoluteValue;
		CooldownBoost = config.CooldownBoost;
		AddEffectMultiplier = config.AddEffectMultiplier;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_cooldownAbility = _target.GetComponent<CooldownAbility>();
	}

	protected override void OnStart()
	{
		if (_cooldownAbility != null)
		{
			_cooldownAbility.EffectScale += AddEffectMultiplier;
			if (_buffSystem != null)
			{
				_abilityCooldownMod = _buffSystem.SetupModStack(_cooldownAbility.GetBaseCooldown, _cooldownAbility.SetCooldown, ModCooldown, 0);
			}
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null && _abilityCooldownMod != null)
		{
			_abilityCooldownMod.RemoveValueModifier(ModCooldown);
		}
		if (_cooldownAbility != null)
		{
			_cooldownAbility.EffectScale -= AddEffectMultiplier;
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		if (_buffSystem != null && _abilityCooldownMod != null)
		{
			_buffSystem.RecalculateValue(_abilityCooldownMod);
		}
		base.OnNewStackCount(deltaStack);
	}

	public float ModCooldown(float modifiedCooldown, float baseCooldown)
	{
		float num = ((!AbsoluteValue) ? (baseCooldown * CooldownBoost) : CooldownBoost) * base.effectScale * (float)Stacks;
		return modifiedCooldown + num;
	}
}
