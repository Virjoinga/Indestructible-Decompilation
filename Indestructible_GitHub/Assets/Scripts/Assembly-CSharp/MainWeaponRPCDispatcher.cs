using UnityEngine;

public class MainWeaponRPCDispatcher : MonoBehaviour
{
	public MainWeapon weapon;

	//[RPC]
	private void SetFireInterval(float value)
	{
		weapon.SetFireInterval(value);
	}

	//[RPC]
	public virtual void SetRange(float value)
	{
		weapon.SetRange(value);
	}
}
