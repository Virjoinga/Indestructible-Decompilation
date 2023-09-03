public class DamageReduceConf : BuffConf
{
	public float ReduceInPercent;

	public override Buff CreateBuff()
	{
		return new DamageReduceBuff(this);
	}
}
