public class ZambeziPassiveAbility : PassiveAbilityBase
{
	public float IgniteDamageLevel = 15f;

	public float BurnDamage = 5f;

	public float BurnDuration = 3f;

	private BuffSystem _buffSystem;

	private Vehicle _ownerVehicle;

	private void Start()
	{
		_buffSystem = GetComponent<BuffSystem>();
		_ownerVehicle = GetComponent<Vehicle>();
		if ((bool)_ownerVehicle)
		{
			_ownerVehicle.makeDamageEvent += OnMakeDamage;
		}
	}

	private void OnMakeDamage(Destructible destructible, float damage)
	{
		if (!(destructible != null) || !(damage >= IgniteDamageLevel))
		{
			return;
		}
		BuffSystem component = destructible.GetComponent<BuffSystem>();
		if ((bool)component)
		{
			BurningBuff burningBuff = component.AddBuffSuspended<BurningBuff>(this);
			if (burningBuff != null)
			{
				burningBuff.duration = BurnDuration;
				burningBuff.effectScale = BurnDamage * _effectScale;
				burningBuff.StartBuff();
				burningBuff.Burn();
			}
		}
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Zambezi;
	}
}
