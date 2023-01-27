using System.Collections;
using UnityEngine;

public class AddBuffsPassiveAbility : PassiveAbilityBase
{
	public BuffConf[] ActivateBuffs;

	public bool BuffGUIVisible;

	private Buff[] _buffs;

	private GameObject _rootObject;

	private BuffSystem _buffSystem;

	protected virtual void Start()
	{
		_rootObject = base.transform.root.gameObject;
		_buffSystem = _rootObject.GetComponent<BuffSystem>();
		StartCoroutine(Activate());
	}

	private IEnumerator Activate()
	{
		yield return null;
		if (ActivateBuffs == null || ActivateBuffs.Length <= 0)
		{
			yield break;
		}
		_buffs = new Buff[ActivateBuffs.Length];
		for (int i = 0; i < ActivateBuffs.Length; i++)
		{
			BuffConf activateBuff = ActivateBuffs[i];
			if (activateBuff != null)
			{
				_buffs[i] = activateBuff.CreateBuff();
			}
			if (_buffs[i] != null)
			{
				_buffs[i].isVisible = BuffGUIVisible;
				_buffs[i] = _buffSystem.AddInstancedBuff(_buffs[i], base.gameObject, true);
			}
		}
	}

	public override void SetEffectScale(float newEffectScale)
	{
		base.SetEffectScale(newEffectScale);
		if (_buffs != null)
		{
			Buff[] buffs = _buffs;
			foreach (Buff buff in buffs)
			{
				buff.effectScale = newEffectScale;
			}
		}
	}
}
