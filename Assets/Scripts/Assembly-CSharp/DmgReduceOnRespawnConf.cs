using UnityEngine;

public class DmgReduceOnRespawnConf : BuffConf
{
	public float ReduceInPercent;

	public float ActiveTime = 2f;

	public GameObject OnActiveFX;

	public override Buff CreateBuff()
	{
		return new DmgReduceOnRespawnBuff(this);
	}
}
