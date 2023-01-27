public class PassiveAbilitiesBoostConf : BuffConf
{
	public bool AbsoluteValue = true;

	public float PassiveAbilityBoost;

	public override Buff CreateBuff()
	{
		return new PassiveAbilitiesBoostBuff(this);
	}
}
