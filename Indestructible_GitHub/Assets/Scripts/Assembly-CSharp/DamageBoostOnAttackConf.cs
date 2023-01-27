using UnityEngine;

public class DamageBoostOnAttackConf : BuffConf
{
	public float DamageBoostPerSec;

	public float DamageForActivate = 3f;

	public float DamageForDeactivate = 3f;

	public GameObject OnActiveFX;

	public override Buff CreateBuff()
	{
		return new DamageBoostOnAttackBuff(this);
	}
}
