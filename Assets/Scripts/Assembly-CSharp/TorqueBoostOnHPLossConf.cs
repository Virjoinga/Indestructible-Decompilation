public class TorqueBoostOnHPLossConf : BuffConf
{
	public float TorquePercentAddPerLossPercent = 0.25f;

	public float MaxSpeedPercentAddPerLossPercent = 0.25f;

	public override Buff CreateBuff()
	{
		return new TorqueBoostOnHPLossBuff(this);
	}
}
