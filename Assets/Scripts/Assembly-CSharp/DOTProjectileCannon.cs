public class DOTProjectileCannon : ProjectileCannon, IDOTWeapon, INetworkWeapon
{
	public DOTWeaponImp dotImplementation = new DOTWeaponImp();

	public DOTWeaponImp dotInterface
	{
		get
		{
			return dotImplementation;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		dotImplementation.Init(this);
	}

	protected override DamageResult Damage(Destructible destructible, float damage)
	{
		DamageResult damageResult = base.Damage(destructible, damage);
		if (damageResult == DamageResult.Damaged)
		{
			dotImplementation.Heat(destructible);
		}
		return damageResult;
	}
}
