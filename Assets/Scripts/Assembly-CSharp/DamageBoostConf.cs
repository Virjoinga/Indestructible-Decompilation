public class DamageBoostConf : BuffConf
{
	public DamageBoostBuff.BoostType BoostDamageType;

	public bool ModBaseValue = true;

	public bool AbsoluteValue = true;

	public float DamageBoost;

	public bool FXVisible;

	public override Buff CreateBuff()
	{
		return new DamageBoostBuff(this);
	}
}
