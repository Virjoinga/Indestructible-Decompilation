public class PunisherPassiveAbility : PassiveAbilityBase
{
	public float DamageBoost = 0.15f;

	private BuffSystem _buffSystem;

	private void Start()
	{
		_buffSystem = GetComponent<BuffSystem>();
		Vehicle component = GetComponent<Vehicle>();
		component.SubscribeToMountedEvent(PartsMounted);
	}

	private void PartsMounted(Vehicle vehicle)
	{
		if (_buffSystem != null)
		{
			MainWeapon weapon = vehicle.weapon;
			if (weapon != null && weapon.damageType == DamageType.Kinetic)
			{
				ModStackFloat modStackFloat = _buffSystem.SetupModStack(weapon.GetBaseDamage, weapon.SetDamage, DamageBooster, 0);
			}
		}
	}

	public float DamageBooster(float modifiedDamage, float baseDamage)
	{
		return modifiedDamage + baseDamage * DamageBoost * _effectScale;
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Punisher;
	}
}
