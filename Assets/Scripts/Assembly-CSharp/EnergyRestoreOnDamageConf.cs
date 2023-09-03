using UnityEngine;

public class EnergyRestoreOnDamageConf : BuffConf
{
	public float DamageToEnergyConversion = 0.1f;

	public GameObject ActivationFX;

	public float FXPeriod = 3f;

	public override Buff CreateBuff()
	{
		return new EnergyRestoreOnDamageBuff(this);
	}
}
