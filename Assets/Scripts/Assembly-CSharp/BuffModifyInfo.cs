public class BuffModifyInfo
{
	public DamageType WeaponDamageType;

	public float Health;

	public float Power;

	public float EnergyShot;

	public float Energy;

	public float Speed;

	public float FireInterval;

	public float Damage;

	public BuffModifyInfo()
	{
	}

	public BuffModifyInfo(BuffModifyInfo other)
	{
		WeaponDamageType = other.WeaponDamageType;
		Health = other.Health;
		Power = other.Power;
		EnergyShot = other.EnergyShot;
		Energy = other.Energy;
		Speed = other.Speed;
		FireInterval = other.FireInterval;
		Damage = other.Damage;
	}
}
