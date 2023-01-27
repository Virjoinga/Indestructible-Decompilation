using UnityEngine;

public class DamageReflectionConf : BuffConf
{
	public int ChancePercent = 1;

	public float ReactivateTime = 2f;

	public GameObject StartFX;

	public GameObject HitFX;

	public override Buff CreateBuff()
	{
		return new DamageReflectionBuff(this);
	}
}
