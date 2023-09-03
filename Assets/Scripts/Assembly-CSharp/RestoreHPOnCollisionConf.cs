using UnityEngine;

public class RestoreHPOnCollisionConf : BuffConf
{
	public bool AbsoluteValue = true;

	public float AddHP;

	public float DamageLevel = 10f;

	public GameObject ActivateFX;

	public GameObject VictimFX;

	public override Buff CreateBuff()
	{
		return new RestoreHPOnCollisionBuff(this);
	}
}
