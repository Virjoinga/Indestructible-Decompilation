public class HealthBoostConf : BuffConf
{
	public bool AbsoluteValue = true;

	public float MaxHPBoost;

	public bool HealToMax = true;

	public override Buff CreateBuff()
	{
		return new HealthBoostBuff(this);
	}
}
