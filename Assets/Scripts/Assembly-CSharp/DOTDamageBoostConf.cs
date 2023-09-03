public class DOTDamageBoostConf : BuffConf
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public bool DurationAbsoluteValue = true;

	public float DurationBoost;

	public override Buff CreateBuff()
	{
		return new DOTDamageBoostBuff(this);
	}
}
