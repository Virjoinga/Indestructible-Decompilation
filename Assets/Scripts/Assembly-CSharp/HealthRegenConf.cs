public class HealthRegenConf : BuffConf
{
	public float HealPerSecond = 10f;

	public override Buff CreateBuff()
	{
		return new HealthRegenBuff(this);
	}
}
