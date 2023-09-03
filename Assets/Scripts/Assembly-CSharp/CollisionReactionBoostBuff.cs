using UnityEngine;

public class CollisionReactionBoostBuff : Buff
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public float FragilityBoost;

	private ModStackFloat _collisionDamageMod;

	private ModStackFloat _fragilityMod;

	public CollisionReactionBoostBuff(CollisionReactionBoostConf config)
		: base(config)
	{
		DamageAbsoluteValue = config.DamageAbsoluteValue;
		DamageBoost = config.DamageBoost;
		FragilityBoost = config.FragilityBoost;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null && _vehicle != null)
		{
			_collisionDamageMod = _buffSystem.SetupModStack(_vehicle.GetBaseDamage, _vehicle.SetDamage, ModDamage, 0);
			_fragilityMod = _buffSystem.SetupModStack(_vehicle.destructible.GetBaseFragilityFactor, _vehicle.destructible.SetFragilityFactor, ModFragility, 0);
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_collisionDamageMod.RemoveValueModifier(ModDamage);
			_fragilityMod.RemoveValueModifier(ModFragility);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		_buffSystem.RecalculateValue(_collisionDamageMod);
		_buffSystem.RecalculateValue(_fragilityMod);
	}

	public float ModDamage(float modifiedDamage, float baseDamage)
	{
		float num = ((!DamageAbsoluteValue) ? (baseDamage * DamageBoost) : DamageBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(modifiedDamage + num, 0f);
	}

	public float ModFragility(float modifiedFragility, float baseFragility)
	{
		return Mathf.Max(modifiedFragility + FragilityBoost * base.effectScale * (float)Stacks, 0f);
	}
}
