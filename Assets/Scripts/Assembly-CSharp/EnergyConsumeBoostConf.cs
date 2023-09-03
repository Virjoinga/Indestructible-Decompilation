public class EnergyConsumeBoostConf : BuffConf
{
	public EnergyConsumeBoostBuff.ConsumerType EnergyConsumerType;

	public bool AbsoluteValue = true;

	public float EnergyConsumeBoost;

	public override Buff CreateBuff()
	{
		return new EnergyConsumeBoostBuff(this);
	}
}
