using System.Collections;
using UnityEngine;

[AddComponentMenu("Indestructible/Ability/SecondaryWeaponAbility")]
public class SecondaryWeaponAbility : CooldownAbility
{
	public float fireDuration = 5f;

	public GameObject weaponPrefab;

	public Transform mountPoint;

	private float _stopTime;

	private MainWeapon _weapon;

	protected override void Start()
	{
		base.Start();
		GetComponent<Vehicle>().SubscribeToMountedEvent(VehiclePartsMounted);
	}

	private void VehiclePartsMounted(Vehicle vehicle)
	{
		StartCoroutine(MountSecondaryWeapon(vehicle));
	}

	private IEnumerator MountSecondaryWeapon(Vehicle vehicle)
	{
		yield return null;
		GameObject weaponObject = Object.Instantiate(weaponPrefab) as GameObject;
		vehicle.Mount(weaponObject, mountPoint);
		_weapon = weaponObject.GetComponentInChildren<MainWeapon>();
	}

	protected override void OnAbilityStart()
	{
		base.OnAbilityStart();
		if (!_weapon.shouldFire)
		{
			_weapon.shouldFire = true;
			StartCoroutine(StopFire());
		}
		_stopTime = Time.time + fireDuration;
		if (_photonView != null)
		{
			_photonView.RPC("ActivateSecondaryWeapon", PhotonTargets.Others);
		}
	}

	private IEnumerator StopFire()
	{
		yield return new WaitForSeconds(fireDuration);
		while (Time.time < _stopTime)
		{
			yield return null;
		}
		_weapon.shouldFire = false;
	}

	[RPC]
	private void ActivateSecondaryWeapon()
	{
		OnAbilityStart();
	}
}
