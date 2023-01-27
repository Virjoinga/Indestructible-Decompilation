using System.Collections;
using UnityEngine;

public class CachedTmpObject : CachedObject
{
	public float baseDeactivationTimeout = 10f;

	private float _deactivationTime;

	private YieldInstruction _deactivationTimeoutInstruction;

	private float _deactivationTimeout;

	public float deactivationTimeout
	{
		get
		{
			return _deactivationTimeout;
		}
		set
		{
			_deactivationTimeout = value;
			_deactivationTimeoutInstruction = new WaitForSeconds(value + 0.05f);
		}
	}

	public override void Activate()
	{
		base.Activate();
		bool flag = _deactivationTime != 0f;
		_deactivationTime = Time.time + _deactivationTimeout;
		if (!flag)
		{
			StartCoroutine(DelayedDeactivate());
		}
	}

	protected override void Awake()
	{
		base.Awake();
		deactivationTimeout = baseDeactivationTimeout;
	}

	private IEnumerator DelayedDeactivate()
	{
		do
		{
			yield return _deactivationTimeoutInstruction;
		}
		while (Time.time < _deactivationTime);
		_deactivationTime = 0f;
		Deactivate();
	}
}
