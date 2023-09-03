using UnityEngine;

public class RavagerPassiveAbility : PassiveAbilityBase
{
	public float StealHP;

	public float StealEnergy;

	private Vehicle _vehicle;

	private void Start()
	{
		IDOTWeapon componentIface = GetComponentIface<IDOTWeapon>();
		_vehicle = GetComponent<Vehicle>();
		_vehicle.SubscribeToMountedEvent(PartsMounted);
	}

	private void PartsMounted(Vehicle vehicle)
	{
		IDOTWeapon iDOTWeapon = vehicle.weapon as IDOTWeapon;
		if (iDOTWeapon != null)
		{
			iDOTWeapon.dotInterface.onTargetDOTDamagedEvent += OnDOTDamage;
		}
	}

	private void OnDOTDamage(Destructible victim, Vehicle victimVehicle, float damage)
	{
		if (victimVehicle != null)
		{
			victimVehicle.destructible.ConsumeHP(StealHP * _effectScale);
			victimVehicle.ConsumeEnergy(StealEnergy * _effectScale);
			if ((bool)_vehicle.photonView)
			{
				_vehicle.photonView.RPC("OnSteal", PhotonTargets.All);
			}
			else
			{
				OnSteal();
			}
		}
	}

	//[RPC]
	private void OnSteal()
	{
		_vehicle.destructible.Heal(StealHP);
		_vehicle.AddEnergy(StealEnergy);
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Ravager;
	}
}
