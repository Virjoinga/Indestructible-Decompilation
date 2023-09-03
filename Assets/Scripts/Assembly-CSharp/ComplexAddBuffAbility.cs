using UnityEngine;

public class ComplexAddBuffAbility : CooldownAbility
{
	public BuffConf[] ActivateBuffs = new BuffConf[0];

	//[RPC]
	protected override void AbilityActivated()
	{
		base.AbilityActivated();
		Buff buff = null;
		BuffConf[] activateBuffs = ActivateBuffs;
		foreach (BuffConf buffConf in activateBuffs)
		{
			buff = null;
			if (buffConf != null)
			{
				buff = buffConf.CreateBuff();
			}
			if (buff != null)
			{
				buff = _buffSystem.AddInstancedBuff(buff, base.gameObject, false);
			}
		}
	}
}
