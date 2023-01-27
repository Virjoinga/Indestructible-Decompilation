public class ROFBoostConf : BuffConf
{
	public ROFBoostBuff.BoostType Type;

	public bool AbsoluteValue = true;

	public float RateOfFireBoost;

	public bool FXVisible;

	public override Buff CreateBuff()
	{
		return new ROFBoostBuff(this);
	}
}
