public class TorqueBoostConf : BuffConf
{
	public bool TorqueAbsoluteValue = true;

	public float TorqueBoost;

	public bool MaxSpeedAbsoluteValue = true;

	public float MaxSpeedBoost;

	public bool EffectVisible = true;

	public override Buff CreateBuff()
	{
		return new TorqueBoostBuff(this);
	}
}
