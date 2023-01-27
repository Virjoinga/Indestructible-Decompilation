using UnityEngine;

public class DualStickController : MoveStickController
{
	private FireStick _fireStick;

	private GunTurret _gunTurret;

	private MainWeapon _weapon;

	protected override void Init(Vehicle vehicle)
	{
		base.Init(vehicle);
		_fireStick = FireStick.instance;
		_weapon = vehicle.weapon;
		_gunTurret = _weapon.gunTurret;
	}

	protected void Update()
	{
		Vector2 vector = _fireStick.GetVector();
		if (float.Epsilon < vector.sqrMagnitude)
		{
			_gunTurret.SetTargetDirection(vector);
			_weapon.shouldFire = true;
		}
		else
		{
			_weapon.shouldFire = false;
		}
	}
}
