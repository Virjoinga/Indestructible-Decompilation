using UnityEngine;

public class AIVehicle : Vehicle
{
	protected override void Awake()
	{
		base.Awake();
		LocateWeapon();
	}

	[RPC]
	protected override void RemoteMountParts(string bodyName, string armorName, string weaponName)
	{
		base.RemoteMountParts(bodyName, armorName, weaponName);
	}

	[RPC]
	protected override void RemoteMountComponents(string components)
	{
		base.RemoteMountComponents(components);
	}
}
