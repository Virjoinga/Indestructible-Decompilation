using System.Collections;
using UnityEngine;

public class HurricanePassiveAbility : DOTBasedPassiveAbility
{
	public float BuffDuration = 5f;

	public float ROFIntervalBoost = 0.2f;

	private bool _activated;

	protected BuffSystem _buffSystem;

	private YieldInstruction _delayYI;

	protected override void Start()
	{
		base.Start();
		_buffSystem = GetComponent<BuffSystem>();
		_delayYI = new WaitForSeconds(BuffDuration);
	}

	protected override void HandleLimitBreak()
	{
		base.HandleLimitBreak();
		if (ROFIntervalBoost != 0f && !_activated && !(_buffSystem == null))
		{
			ROFBoostBuff rOFBoostBuff = _buffSystem.AddBuffSuspended<ROFBoostBuff>(base.gameObject);
			if (rOFBoostBuff != null)
			{
				rOFBoostBuff.Type = ROFBoostBuff.BoostType.Kinetic;
				rOFBoostBuff.duration = BuffDuration;
				rOFBoostBuff.AbsoluteValue = false;
				rOFBoostBuff.RateOfFireBoost = ROFIntervalBoost * _effectScale;
				rOFBoostBuff.FXVisible = true;
				rOFBoostBuff.StartBuff();
				_activated = true;
				StartCoroutine(Reactivate());
			}
		}
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Hurricane;
	}

	protected IEnumerator Reactivate()
	{
		yield return _delayYI;
		_activated = false;
	}
}
