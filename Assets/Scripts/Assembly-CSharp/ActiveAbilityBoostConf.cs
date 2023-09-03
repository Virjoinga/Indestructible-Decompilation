public class ActiveAbilityBoostConf : BuffConf
{
	public bool AbsoluteValue = true;

	public float CooldownBoost;

	public float AddEffectMultiplier;

	public override Buff CreateBuff()
	{
		return new ActiveAbilityBoostBuff(this);
	}
}
