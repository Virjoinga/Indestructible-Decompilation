public class LaserGunHighImpact : LaserGun
{
	public ImpactWeaponImp impactImplementation = new ImpactWeaponImp();

	protected override void Start()
	{
		base.Start();
		impactImplementation.Init();
	}

	protected override DamageResult Damage(Destructible destructible, float damage)
	{
		DamageResult damageResult = base.Damage(destructible, damage);
		if (damageResult == DamageResult.Damaged)
		{
			impactImplementation.AddHitImpulse(destructible, base.lastHitPoint, base.gunTurret.forward);
		}
		return damageResult;
	}
}
