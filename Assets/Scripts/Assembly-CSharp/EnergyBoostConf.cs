public class EnergyBoostConf : BuffConf
{
	public bool EnegryGainAbsoluteValue = true;

	public float EnegryGainBoost;

	public bool MaxEnegryAbsoluteValue = true;

	public float MaxEnegryBoost;

	public bool RestoreToMax = true;

	public override Buff CreateBuff()
	{
		return new EnergyBoostBuff(this);
	}
}
