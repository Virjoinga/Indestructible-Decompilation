public class StasisConf : BuffConf
{
	public float dragBonus = 1f;

	public override Buff CreateBuff()
	{
		return new StasisBuff(this);
	}
}
