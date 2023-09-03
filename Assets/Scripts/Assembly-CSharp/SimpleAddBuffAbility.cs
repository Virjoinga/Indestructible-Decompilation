using UnityEngine;

public class SimpleAddBuffAbility : CooldownAbility
{
	public BuffConf ActivateBuff;

	//[RPC]
	protected override void AbilityActivated()
	{
		base.AbilityActivated();
		Buff buff = null;
		if (ActivateBuff != null)
		{
			buff = ActivateBuff.CreateBuff();
		}
		if (buff != null)
		{
			buff = _buffSystem.AddInstancedBuff(buff, base.gameObject, false);
		}
	}
}
