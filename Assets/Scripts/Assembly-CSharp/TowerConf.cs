public class TowerConf : BuffConf
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public bool SpeedAbsoluteValue = true;

	public float SpeedBoost;

	public float DamageReduceInPercent;

	public override Buff CreateBuff()
	{
		return new TowerBuff(this);
	}
}
