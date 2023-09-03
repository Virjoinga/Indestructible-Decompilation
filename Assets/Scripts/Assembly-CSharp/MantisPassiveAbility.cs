using UnityEngine;

public class MantisPassiveAbility : PassiveAbilityBase
{
	public float speedLimit = 20f;

	public float damageBonus = 0.01f;

	private VehiclePhysics _vehiclePhysics;

	private BuffSystem _buffSystem;

	private ModStackFloat _explDamageMod;

	private float _speedExcess;

	private void Start()
	{
		_vehiclePhysics = GetComponentIface<VehiclePhysics>();
		if (_vehiclePhysics == null)
		{
			base.enabled = false;
		}
		_buffSystem = GetComponent<BuffSystem>();
		GetComponent<Vehicle>().SubscribeToMountedEvent(VehiclePartsMounted);
	}

	private void OnEnable()
	{
		if (_explDamageMod != null)
		{
			_explDamageMod.Recalculate();
		}
	}

	private float BoostDamage(float modifiedDamage, float baseDamage)
	{
		if (_speedExcess > 0f)
		{
			return modifiedDamage * (1f + damageBonus * _speedExcess * _effectScale);
		}
		return modifiedDamage;
	}

	private void Update()
	{
		if (_explDamageMod == null)
		{
			return;
		}
		if (_vehiclePhysics.sqrSpeed > speedLimit * speedLimit)
		{
			_speedExcess = Mathf.Sqrt(_vehiclePhysics.sqrSpeed) - speedLimit;
			_explDamageMod.Recalculate();
			return;
		}
		if (_speedExcess > 0f)
		{
			_explDamageMod.Recalculate();
		}
		_speedExcess = 0f;
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Mantis;
	}

	private void VehiclePartsMounted(Vehicle vehicle)
	{
		if (vehicle.weapon.damageType == DamageType.Explosive)
		{
			_explDamageMod = _buffSystem.SetupModStack(vehicle.weapon.GetBaseDamage, vehicle.weapon.SetDamage, BoostDamage, 0);
		}
	}
}
