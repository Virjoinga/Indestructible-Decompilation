public class TorqueBoostOnDamageConf : BuffConf
{
	public bool TorqueAbsoluteValue = true;

	public float TorqueBoost;

	public bool MaxSpeedAbsoluteValue = true;

	public float MaxSpeedBoost;

	public float DamageForActivate = 1f;

	public float AccumulatedDamageReducePerSec = 1f;

	public float ActiveTime = 2f;

	public override Buff CreateBuff()
	{
		return new TorqueBoostOnDamageBuff(this);
	}
}
